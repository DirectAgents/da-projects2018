using ClientPortal.Data.Contexts;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientPortal.Data.Contexts
{
    public partial class Creative
    {
        [NotMapped]
        public string DisplayName
        {
            get { return CreativeId + (string.IsNullOrEmpty(CreativeName) ? "" : " - " + CreativeName); }
        }

        [NotMapped]
        public string DisplayNameWithType
        {
            get { return DisplayName + " [" + this.CreativeType.CreativeTypeName + "]"; }
        }
    }
}
