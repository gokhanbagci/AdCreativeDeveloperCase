namespace AdCreativeDeveloperCase.Application.Business
{
    public class ImageDownloadStartEventArgs : EventArgs
    {
        public int Index { get; set; }
        public string ImageUrl { get; set; }
        public ImageDownloadStartEventArgs(int index, string imageUrl)
        {
            Index = index;
            ImageUrl = imageUrl;
        }
    }
}