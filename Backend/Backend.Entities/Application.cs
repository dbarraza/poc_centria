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
        /// Proecess just created
        /// </summary>
        Created = 1,

        /// <summary>
        /// Excel is being processed
        /// </summary>
        Pending = 2,

        /// <summary>
        /// Excel already processed and loaded successfully
        /// </summary>
        BaseLoaded = 3,

        /// <summary>
        /// Candidates prefiltered successfully
        /// </summary>
        Prefiltered = 4
    }
}
