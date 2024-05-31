namespace Backend.Models.In
{
    public class CvUploadIn
    {
        /// <summary>
        /// Cv Document to be uploaded
        /// </summary>
        public byte[] File { get; set; }
        
        /// <summary>
        /// Application Id of the process
        /// </summary>
        public Guid ApplicationId{ get; set; }
        
    }
}
