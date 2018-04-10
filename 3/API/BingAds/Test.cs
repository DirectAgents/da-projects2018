using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using Microsoft.BingAds.V11.CampaignManagement;

namespace BingAds
{
    public class Test
    {
        private readonly string m_token = ConfigurationManager.AppSettings["BingApiToken"];
        private readonly string m_username = ConfigurationManager.AppSettings["BingApiUsername"];
        private readonly string m_password = ConfigurationManager.AppSettings["BingApiPassword"];

        private static CampaignManagementServiceClient service = null;

        // 234647, 886985
        public IList<Campaign> GetCampaigns(long customerId, long accountId)
        {
            service = new CampaignManagementServiceClient();

            GetCampaignsByAccountIdRequest request = new GetCampaignsByAccountIdRequest();
            GetCampaignsByAccountIdResponse response = null;

            // Set the header information.

            request.CustomerId = customerId.ToString();
            request.CustomerAccountId = accountId.ToString();
            request.DeveloperToken = m_token;
            request.UserName = m_username;
            request.Password = m_password;

            // Set the request information.

            request.AccountId = accountId;

            try
            {
                response = service.GetCampaignsByAccountId(request);
            }
            catch (FaultException<AdApiFaultDetail> fault)
            {
                // Log this fault.

                Console.WriteLine("GetCampaignsByAccountId failed with the following faults:\n");

                foreach (AdApiError error in fault.Detail.Errors)
                {
                    if (105 == error.Code) //  InvalidCredentials
                    {
                        Console.WriteLine("The specified credentials are not valid " +
                            "or the account is inactive.");
                    }
                    else
                    {
                        Console.WriteLine("Error code: {0} ({1})\nMessage: {2}\nDetail: {3}\n",
                            error.ErrorCode, error.Code, error.Message, error.Detail);
                    }
                }

                throw new Exception("", fault);
            }
            catch (FaultException<ApiFaultDetail> fault)
            {
                // Log this fault.

                Console.WriteLine("GetCampaignsByAccountId failed with the following faults:\n");

                foreach (OperationError error in fault.Detail.OperationErrors)
                {
                    switch (error.Code)
                    {
                        case 106: //  UserIsNotAuthorized
                            Console.WriteLine("The user is not authorized to call this operation.");
                            break;

                        case 1030: //  CampaignServiceAccountIdHasToBeSpecified
                            Console.WriteLine("The CustomerAccountId header element " +
                                "cannot be null or empty.");
                            break;

                        case 1102: //  CampaignServiceInvalidAccountId
                            Console.WriteLine("The account ID is not valid");
                            break;

                        default:
                            Console.WriteLine("Error code: {0} ({1})\nMessage: {2}\nDetail: {3}\n",
                                error.ErrorCode, error.Code, error.Message, error.Details);
                            break;
                    }
                }

                // This is not a batch operation, so there should be no batch errors.

                foreach (BatchError error in fault.Detail.BatchErrors)
                {
                    Console.WriteLine("Unable to add extension #{0}", error.Index);
                    Console.WriteLine("Error code: {0} ({1})\nMessage: {2}\nDetail: {3}\n",
                        error.ErrorCode, error.Code, error.Message, error.Details);
                }

                throw new Exception("", fault);
            }

            return response.Campaigns;
        }
    }
}
