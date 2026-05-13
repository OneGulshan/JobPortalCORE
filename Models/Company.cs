using System.ComponentModel.DataAnnotations;

namespace JobPortalCORE.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Comment { get; set; }
        public int JobProfileId { get; set; }
        [System.ComponentModel.DefaultValue(false)]
        public bool Status { get; set; }
    }
}
