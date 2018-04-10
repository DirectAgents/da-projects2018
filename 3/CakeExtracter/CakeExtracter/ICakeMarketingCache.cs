using System.Collections.Generic;
using ClientPortal.Web.Models.Cake;

namespace CakeExtracter
{
    public interface  ICakeMarketingCache : ICakeMarketing
    {
        void PutClicks(List<click> clicks);
    }
}