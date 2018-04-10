using System;
using System.Collections.Generic;
using AdRoll.Entities;

namespace AdRoll.Clients
{
    #region get_advertisables

    public class GetAdvertisablesClient : ApiClient
    {
        public GetAdvertisablesClient()
            : base(1, "organization", "get_advertisables")
        { }

        public GetAdvertisablesResponse Get()
        {
            var request = new ApiRequest();
            var result = Execute<GetAdvertisablesResponse>(request);
            return result;
        }
    }
    public class GetAdvertisablesResponse
    {
        public List<Advertisable> results { get; set; }
    }

    #endregion
    #region get_ad

    public class GetAdClient : ApiClient
    {
        public GetAdClient()
            : base(1, "ad", "get")
        { }

        public GetAdResponse Get(GetAdRequest request)
        {
            var result = Execute<GetAdResponse>(request);
            return result;
        }
    }
    public class GetAdRequest : ApiRequest
    {
        public string ad { get; set; }
    }
    public class GetAdResponse
    {
        public Ad results { get; set; }
    }

    #endregion
}
