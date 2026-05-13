namespace JobPortalCORE.ViewModels
{
    public class ManageJobPostViewModel
    {
        public int JobPostId { get; set; }
        public int JobProfileId { get; set; }
        public int MinExp { get; set; }
        public int MaxExp { get; set; }
        public int MinSal { get; set; }
        public int MaxSal { get; set; }
        public int NoOfVac { get; set; }
        public int NoticePeriod { get; set; }
        public string Comment { get; set; }
        [System.ComponentModel.DefaultValue(false)]
        public bool Status { get; set; }
    }
}
