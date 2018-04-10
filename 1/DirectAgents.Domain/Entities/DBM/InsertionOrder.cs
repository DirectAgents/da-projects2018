using System.ComponentModel.DataAnnotations.Schema;

namespace DirectAgents.Domain.Entities.DBM
{
    public class InsertionOrder
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        public string Name { get; set; }
        public string Bucket { get; set; }
    }
}
