using System.Net.Mail;

namespace CakeExtracter.Reports
{
    public interface IReport
    {
        string Subject { get; }
        string Generate();

        AlternateView GenerateView();
        Attachment GenerateSpreadsheetAttachment();
        void DisposeResources();
    }
}
