using System.ComponentModel.DataAnnotations;

namespace JobPortalCORE.Models
{
    public class Category
    {
        [Key]
        public int CatId { get; set; }
        public string Name { get; set; }
    }
}
