using CookComputing.XmlRpc;
using System;
using System.Configuration;
using System.Net;

namespace LocalConnex
{
    [XmlRpcUrl("https://api.marchex.io/api/xmlrpc/1")]
    public interface IVoicestar : IXmlRpcProxy
    {
        [XmlRpcMethod("call.search")]
        Call[] callSearch(string accid, CallSearchParams searchParams);

        [XmlRpcMethod("ad.name.get")]
        string adName(string cmpid);
    }

    public class LCUtility
    {
        private readonly string _username = ConfigurationManager.AppSettings["LocalConnexUsername"];
        private readonly string _password = ConfigurationManager.AppSettings["LocalConnexPassword"];

        private IVoicestar _client;

        // --- Logging ---
        private Action<string> _LogInfo;
        private Action<string> _LogError;

        private void LogInfo(string message)
        {
            if (_LogInfo == null)
                Console.WriteLine(message);
            else
                _LogInfo("[LCUtility] " + message);
        }

        private void LogError(string message)
        {
            if (_LogError == null)
                Console.WriteLine(message);
            else
                _LogError("[LCUtility] " + message);
        }

        // --- Constructors ---
        public LCUtility(Action<string> logInfo, Action<string> logError)
        {
            _LogInfo = logInfo;
            _LogError = logError;

            _client = (IVoicestar)XmlRpcProxyGen.Create(typeof(IVoicestar));
            _client.Credentials = new NetworkCredential(_username, _password);
        }

        public Call[] GetCalls(string accid, DateTime date)
        {
            return GetCalls(accid, date, date);
        }
        public Call[] GetCalls(string accid, DateTime start, DateTime end)
        {
            var searchParams = new CallSearchParams
            {
                start = start,
                end = new DateTime(end.Year, end.Month, end.Day, 23, 59, 59)
            };
            var calls = _client.callSearch(accid, searchParams);
            return calls;
        }

        public string GetCampaignName(string cmpid)
        {
            return _client.adName(cmpid);
        }
    }
}
