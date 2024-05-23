namespace Backend.Models.In
{
    public class ExcelUploadIn
    {
        /// <summary>
        /// Excel Document to be uploaded
        /// </summary>
        public byte[] File { get; set; }

        /// <summary>
        /// Application Id
        /// </summary>
        public Guid ApplicationId { get; set; }
    }
}
