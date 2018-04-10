using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using WatiN.Core;
using WatiN.Core.Native;

namespace Amazon
{

    public class AmazonAuth
    {
        private const string AuthorizeUri = "https://www.amazon.com/ap/oa";
        private const string TokenUri = "https://api.amazon.com/auth/o2/token";
        private const string ClientUrl = "https://portal.directagents.com";
        public const string ConsentUriFormatter = "{0}?client_id={1}&scope=cpc_advertising:campaign_management&response_type=code&redirect_uri={2}";
        public const string AccessUriFormatter = "{0}?client_id={1}&code={2}&grant_type=authorization_code&redirect_uri={3}";
        public const string RefreshUriFormatter = "{0}?client_id={1}&grant_type=refresh_token&redirect_uri={2}&refresh_token={3}";
        public const string AuthorizeUriFormatter = "{0}?client_id={1}&code={2}&grant_type=authorization_code&redirect_uri={3}";

        private string UserName, Password, ClientId, ClientSecret, ApplicationAccessCode="NEED TO ADD CODE";

        public string _error = null;

        // --- Logging ---
        private Action<string> _LogInfo;
        private Action<string> _LogError;

        private void LogInfo(string message)
        {
            if (_LogInfo == null)
                Console.WriteLine(message);
            else
                _LogInfo("[AmazonAuth] " + message);
        }

        private void LogError(string message)
        {
            if (_LogError == null)
                Console.WriteLine(message);
            else
                _LogError("[AmazonAuth] " + message);
        }
        public AmazonAuth(Action<string> logInfo, Action<string> logError)
        {
            _LogInfo = logInfo;
            _LogError = logError;
        }

        // accessCode == authorization code

        public AmazonAuth(string clientId, string clientSecret, string accessCode)//, string refreshToken)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            ApplicationAccessCode = accessCode;
            //RefreshToken = refreshToken;
        }

        public AmazonAuth(string clientId, string clientSecret, string username, string password)//, string refreshToken)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            UserName = username;
            Password = password;
            //RefreshToken = refreshToken;
        }

        //// not used?
        //public AccessRefreshTokens GetInitialTokens(string refreshToken = null)
        //{
        //    if (string.IsNullOrEmpty(ApplicationAccessCode))
        //    {
        //        var code = DoLoginForm();
        //        if (string.IsNullOrEmpty(code))
        //            return null;

        //        ApplicationAccessCode = code;
        //    }
        //    return GetAccessRefreshTokens(refreshToken);
        //}

        //public AccessTokens GetNewTokens(string refreshToken)
        //{
        //    //var uri = string.Format(RefreshUriFormatter, TokenUri, ClientId, DesktopUri, refreshToken);
        //    //var uri = string.Format(RefreshUriFormatter, TokenUri, ClientId, refreshToken);
        //    AccessTokens tokens = GetAccessTokens();
        //    return tokens;
        //}

        [STAThread]
        private string DoLoginForm()
        {
            var uri = string.Format(ConsentUriFormatter, AuthorizeUri, ClientId, ClientUrl);
            //var uri = string.Format(AuthorizeUri, ClientId);
            string code = null;

            using (var browser = new IE(uri))
            {
                var page = browser.Page<MyPage>();
                page.PasswordField.TypeText(Password);
                try
                {
                    StringBuilder js = new StringBuilder();
                    js.Append(@"var myTextField = document.getElementById('ap_email');");
                    js.Append(@"myTextField.setAttribute('value', '" + UserName + "');");
                    browser.RunScript(js.ToString());
                    var field = browser.ElementOfType<TextFieldExtended>("ap_email");
                    field.TypeText(UserName);
                }
                catch (Exception ex)
                {
                    LogError(ex.Message);
                    Console.Write(ex.Message + ex.StackTrace);
                }
                page.LoginButton.Click();
                browser.WaitForComplete();
                //browser.Button(Find.ById("idBtn_Accept")).Click();

                //return uri should look like this: https://portal.directagents.com/Account/Login?ReturnUrl=%2f%3fcode%3dANrCxMDnqHYdFuNLGHYg%26scope%3dprofile&code=ANrCxMDnqHYdFuNLGHYg&scope=profile
                Uri myUri = new Uri(browser.Url);
                code = HttpUtility.ParseQueryString(myUri.Query).Get("code");
                if (string.IsNullOrEmpty(code))
                {
                    _error = "Missing code value. The URL is missing code value. Check the URL for Code or check if Amazon changed their AUTH mechanism.";
                }
            }
            return code;
        }

        public AccessRefreshTokens GetAccessRefreshTokens(string refreshToken = null)
        {
            AccessRefreshTokens tokenResponse = null;
            try
            {
                var restClient = new RestClient
                {
                    BaseUrl = new Uri(TokenUri),
                    Authenticator = new HttpBasicAuthenticator(ClientId, ClientSecret)
                };
                restClient.AddHandler("application/x-www-form-urlencoded", new JsonDeserializer());

                var request = new RestRequest();
                request.AddParameter("redirect_uri", "https://portal.directagents.com");
                if (String.IsNullOrWhiteSpace(refreshToken))
                {
                    request.AddParameter("grant_type", "authorization_code");
                    request.AddParameter("code", ApplicationAccessCode);
                }
                else
                {
                    request.AddParameter("grant_type", "refresh_token");
                    request.AddParameter("refresh_token", refreshToken);
                }
                var response = restClient.ExecuteAsPost<GetTokenResponse>(request, "POST");

                if (response.Data == null || response.Data.access_token == null)
                    System.Console.WriteLine("Failed to get access token");//LogError("Failed to get access token");

                if (response.Data != null && response.Data.refresh_token == null)
                    System.Console.WriteLine("Failed to get refresh token");//LogError("Failed to get refresh token");

                if (response.Data != null)
                {
                    tokenResponse = new AccessRefreshTokens
                    {
                        access_token = response.Data.access_token,
                        refresh_token = response.Data.refresh_token         // update this in case it changed
                    };
                }

            }
            catch (Exception x)
            {
                LogError(x.Message);
            }
            return tokenResponse;
        }
    }

    public class MyPage : WatiN.Core.Page
    {
        public TextField PasswordField
        {
            get { return Document.TextField(Find.ByName("password")); }
        }
        public TextField EmailField
        {
            get { return Document.TextField(Find.ById("ap_email")); }
        }
        public WatiN.Core.Button LoginButton
        {
            get { return Document.Button(Find.ById("signInSubmit")); }
        }
    }

    [ElementTag("input", InputType = "text", Index = 0)]
    [ElementTag("input", InputType = "password", Index = 1)]
    [ElementTag("input", InputType = "textarea", Index = 2)]
    [ElementTag("input", InputType = "hidden", Index = 3)]
    [ElementTag("textarea", Index = 4)]
    [ElementTag("input", InputType = "email", Index = 5)]
    [ElementTag("input", InputType = "url", Index = 6)]
    [ElementTag("input", InputType = "number", Index = 7)]
    [ElementTag("input", InputType = "range", Index = 8)]
    [ElementTag("input", InputType = "search", Index = 9)]
    [ElementTag("input", InputType = "color", Index = 10)]
    public class TextFieldExtended : TextField
    {
        public TextFieldExtended(DomContainer domContainer, INativeElement element)
            : base(domContainer, element)
        {
        }

        public TextFieldExtended(DomContainer domContainer, ElementFinder finder)
            : base(domContainer, finder)
        {
        }

        public static void Register()
        {
            Type typeToRegister = typeof(TextFieldExtended);
            ElementFactory.RegisterElementType(typeToRegister);
        }
    }


    [DataContract]
    public class AccessRefreshTokens
    {
        //[DataMember]
        // Indicates the duration in seconds until the access token will expire.
        //internal int expires_in = 0;

        [DataMember]
        // When calling Bing Ads service operations, the access token is used as  
        // the AuthenticationToken header element.
        internal string access_token = null;

        [DataMember]
        // May be used to get a new access token with a fresh expiration duration.
        internal string refresh_token = null;

        public string AccessToken { get { return access_token; } }
        //public int ExpiresIn { get { return expires_in; } }
        public string RefreshToken { get { return refresh_token; } }
    }

}
