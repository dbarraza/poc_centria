namespace Backend.Models.Out
{
    public class CandidateModelOut
    {
        public string ApplicationId { get; set; }
        public int CandidateId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int SalaryExpectation { get; set; }
        public string AvailabilityForWork { get; set; }
        public string Content { get; set; }
        public string PoliceRecord { get; set; }
        public string CriminalRecord { get; set; }
        public string JudicialRecord { get; set; }
        public string Consent { get; set; }
        public string HasFamiliar { get; set; }
    }
}
