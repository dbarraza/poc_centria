namespace Backend.Entities
{
    // Represents a Chat Entity
    public class Application : BaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public string Name { get; set; }
        public string ExcelUrl { get; set; }
        public ApplicationStatus Status { get; set; }
        public string JobDescription { get; set; }
        public string ErrorMessage { get; set; }
    }

    public enum ApplicationStatus
    {
        /// <summary>
        /// Error ocurred
        /// </summary>
        Error = -1,

        /// <summary>
        /// Proecess just created
        /// </summary>
        Created = 1,

        /// <summary>
        /// Candidates loaded successfully
        /// </summary>
        CandidatesLoaded = 2,

        /// <summary>
        /// Candidates prefiltered successfully
        /// </summary>
        Prefiltered = 3
    }
}
