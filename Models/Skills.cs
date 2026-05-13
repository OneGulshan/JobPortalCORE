using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortalCORE.Models
{
    public class Skills
    {
        [Key]
        public int SklId { get; set; }
        public int JPId { get; set; }
        public string Name { get; set; }
    }
}
