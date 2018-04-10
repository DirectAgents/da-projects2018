namespace Cake.Data.Wsdl.ExportService
{
    public partial class advertiser
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

        public string AdManagerName
        {
            get
            {
                if (this.tags.Length > 0)
                {
                    return this.tags[0].tag_name;
                }
                else
                {
                    return "default";
                }
            }
        }
    }
}
