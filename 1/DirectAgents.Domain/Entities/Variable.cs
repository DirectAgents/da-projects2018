using System.ComponentModel.DataAnnotations;

namespace DirectAgents.Domain.Entities
{
    public class Variable
    {
        [Key]
        public string Name { get; set; }
        public string StringVal { get; set; }
        public int? IntVal { get; set; }
        public decimal? DecVal { get; set; }
    }
}
