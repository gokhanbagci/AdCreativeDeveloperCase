namespace AdCreativeDeveloperCase.Application.Business
{
    public class ImageDownloadCancelEventArgs : EventArgs
    {
        public int Index { get; set; }
        public string ImageUrl { get; set; }
        public ImageDownloadCancelEventArgs(int index, string imageUrl)
        {
            Index = index;
            ImageUrl = imageUrl;
        }
    }
}