using System.ComponentModel.DataAnnotations.Schema;

namespace DirectAgents.Domain.Entities.Cake
{
    public class Vertical
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int VerticalId { get; set; }
        public string VerticalName { get; set; }
    }
}
