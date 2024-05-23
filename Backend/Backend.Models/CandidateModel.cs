namespace Backend.Models
{
    public class CandidateModel
    {
        public string ApplicationId { get; set; }
        public string CandidateId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string SalaryExpectation { get; set; }
        public string AvailabilityForWork { get; set; }
        public string Content { get; set; }
        public string PoliceRecord { get; set; }
        public string CriminalRecord { get; set; }
        public string JudicialRecord { get; set; }
        public string Consent { get; set; }
        public string HasFamiliar { get; set; }

        public ReadOnlyMemory<float> ContentVector { get; set; }

        public override string ToString()
        {
            return $"FormId: {CandidateId} " +
                   $"Name: {Name} " +
                   $"Email: {Email} " +
                   $"Content: {Content} ";
        }
    }
}
