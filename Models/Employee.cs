using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http; // IFormFile ke liye

namespace JobPortalCORE.Models
{
    public class Employee
    {
        [Key]
        public int EId { get; set; }
        public string Name { get; set; }
        public Gender? Gender { get; set; }

        public int JobProfileId { get; set; }
        public int SkillsId { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
        public long? Mobno { get; set; }
        public int? Age { get; set; }

        public int CountryId { get; set; }
        public int StateId { get; set; }
        public int CityId { get; set; }

        public string ImagePath { get; set; }
        [NotMapped]
        public IFormFile ImageFile { get; set; }
        public string Comment { get; set; }
        [System.ComponentModel.DefaultValue(false)]
        public bool Status { get; set; }

        [NotMapped]
        public IFormFile ResumeFile { get; set; }
        public string ResumePath { get; set; }

        // 👇 NAYA LOGIC: RELATIONS (Foreign Keys)
        [ForeignKey("JobProfileId")]
        public virtual JobProfile JobProfile { get; set; }

        [ForeignKey("SkillsId")]
        public virtual Skills Skill { get; set; }

        [ForeignKey("CityId")]
        public virtual City City { get; set; }
    }

    public enum Gender
    {
        Male = 1,
        Female = 2,
        Others = 3
    }
}