using System.ComponentModel.DataAnnotations;

namespace JobPortalCORE.Models
{
    public class JobPost
    {
        [Key]
        public int JobPostId { get; set; }
        public int JobProfileId { get; set; }
        public int? MinExp { get; set; }
        public int? MaxExp { get; set; }
        public int? MinSal { get; set; }
        public int? MaxSal { get; set; }
        public int? NoOfVac { get; set; }
        public int? NoticePeriod { get; set; }
        public string Comment { get; set; }
        public DateTime InsertedDate { get; set; }
        public bool Status { get; set; }
    }
}
