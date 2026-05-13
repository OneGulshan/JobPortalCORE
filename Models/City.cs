using System.ComponentModel.DataAnnotations;

namespace JobPortalCORE.Models
{
    public class City
    {
        [Key]
        public int CityId { get; set; }
        public int SId { get; set; }
        public string Name { get; set; }
    }
}
