using JobPortalCORE.Models;
using System.ComponentModel.DataAnnotations;

namespace JobPortalCORE.ViewModels
{
    public class ManageJobSeekerViewModel
    {
        [Key]
        public int Id { get; set; }
        public string EName { get; set; }
        public Gender? Gender { get; set; }
        public string JPName { get; set; }
        public string SName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public long Mobno { get; set; }
        public int Age { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string CityName { get; set; }
        public string ImagePath { get; set; }
        public string Comment { get; set; }
        [System.ComponentModel.DefaultValue(false)]
        public bool Status { get; set; }
    }
}
