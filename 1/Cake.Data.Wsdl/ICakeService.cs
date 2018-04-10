using System.Collections.Generic;

namespace Cake.Data.Wsdl
{
    //[LogMethodBoundary]
    public interface ICakeService
    {
        Cake.Data.Wsdl.ReportsService.conversion[] Conversions();
        Cake.Data.Wsdl.ExportService.advertiser[] ExportAdvertisers();
        Cake.Data.Wsdl.ExportService.affiliate[] ExportAffiliates();
        IEnumerable<Cake.Data.Wsdl.ExportService.campaign[]> ExportCampaigns();
        Cake.Data.Wsdl.ExportService.offer1[] ExportOffers();
    }
}
