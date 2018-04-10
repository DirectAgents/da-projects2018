namespace Cake.Data.Wsdl.ExportService
{
    public partial class affiliate
    {
        public int AccountManagerId
        {
            get
            {
                return this.account_managers[0].contact_id;
            }
        }

        public string AccountManagerName
        {
            get
            {
                return this.account_managers[0].contact_name;
            }
        }

        public string Currency
        {
            get
            {
                return this.currency_settings.currency.currency_abbr;
            }
        }
    }
}
