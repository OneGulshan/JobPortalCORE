using System.ComponentModel.DataAnnotations;

namespace JobPortalCORE.Models
{
    public class JobProfile
    {
        [Key]
        public int JPId { get; set; }
        public string Name { get; set; }
    }
}
