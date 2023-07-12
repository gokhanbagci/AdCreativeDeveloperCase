namespace AdCreativeDeveloperCase.Application.Models
{
    public class DownloadSettings
    {
        public int Count { get; set; }
        public int Parallelism { get; set; }
        public string? SavePath { get; set; }
    }
}
