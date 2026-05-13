using System.ComponentModel.DataAnnotations;

namespace JobPortalCORE.ViewModels
{
    public class JobsViewJobSeekerViewModel
    {
        public int JobPostId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string JPName { get; set; }
        public int MinExp { get; set; }
        public int MaxExp { get; set; }
        public int MinSal { get; set; }
        public int MaxSal { get; set; }
        public int NoOfVac { get; set; }
        public int NoticePeriod { get; set; }
        public string Comment { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime InsertedDate { get; set; }
        public DateOnly InsertedDateOnly { get; set; }
    }
}
