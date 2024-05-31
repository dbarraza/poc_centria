namespace Backend.Models.In
{
    public class ExcelUploadIn
    {
        /// <summary>
        /// Excel Document to be uploaded
        /// </summary>
        public byte[] File { get; set; }

        /// <summary>
        /// The name of the application process
        /// </summary>
        public string Name{ get; set; }

        /// <summary>
        /// The job description of the application process
        /// </summary>
        public string JobDescription { get; set; }
    }
}
