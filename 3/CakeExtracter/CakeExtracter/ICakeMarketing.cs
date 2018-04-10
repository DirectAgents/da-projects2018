using System;
using System.Collections.Generic;
using ClientPortal.Web.Models.Cake;

namespace CakeExtracter
{
    public interface ICakeMarketing
    {
        List<click> Clicks(int advertiserId, DateTime date);
        List<conversion> Conversions(int advertiserId, DateTime startDate);
    }
}