namespace Backend.Models.Out
{
    public class ApplicationModelOut
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ExcelUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
    }
}
