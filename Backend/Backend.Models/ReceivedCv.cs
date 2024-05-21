using System;

namespace Backend.Models
{
    /// <summary>
    /// Cv received and processed from storage container
    /// </summary>
    public class ReceivedCv : BaseModel
    {      
        public Guid ReceivedCvId { get; set; }

        public string JobName { get; set; }

        public string CandidateName { get; set; }

        public string CandidateEmail { get; set; }

        public string Calification { get; set; }

        public string Explanation { get; set; }

        public DateTime ProcessingDate { get; set; }
        public string FileUri { get; set; }


    }

}


