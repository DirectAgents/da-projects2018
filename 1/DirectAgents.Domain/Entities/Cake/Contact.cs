using System.ComponentModel.DataAnnotations.Schema;

namespace DirectAgents.Domain.Entities.Cake
{
    public class Contact
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ContactId { get; set; }

        public int RoleId { get; set; }
        public virtual Role Role { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string Title { get; set; }
        public string PhoneWork { get; set; }
        public string PhoneCell { get; set; }
        public string PhoneFax { get; set; }

        [NotMapped]
        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }
    }
}
