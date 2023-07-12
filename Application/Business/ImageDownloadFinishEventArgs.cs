namespace AdCreativeDeveloperCase.Application.Business
{
    public class ImageDownloadFinishEventArgs : EventArgs
    {
        public int Index { get; set; }
        public string ImageUrl { get; set; }

        public ImageDownloadFinishEventArgs(int index, string imageUrl)
        {
            Index = index;
            ImageUrl = imageUrl;
        }
    }
}