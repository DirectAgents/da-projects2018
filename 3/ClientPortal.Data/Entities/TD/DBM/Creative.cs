using System.ComponentModel.DataAnnotations.Schema;

namespace ClientPortal.Data.Entities.TD.DBM
{
    public class Creative
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CreativeID { get; set; }
        public string CreativeName { get; set; }

        public int InsertionOrderID { get; set; }
        public virtual InsertionOrder InsertionOrder { get; set; }
    }
}
