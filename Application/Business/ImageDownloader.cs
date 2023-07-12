namespace AdCreativeDeveloperCase.Application.Business
{
    public class ImageDownloader
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly string _randomImagePath;
        private readonly int _index;
        private readonly string _savePath;
        public event EventHandler<ImageDownloadStartEventArgs>? Started;
        public event EventHandler<ImageDownloadFinishEventArgs>? Finished;
        public event EventHandler<ImageDownloadCancelEventArgs>? Canceled;
        public ImageDownloader(IHttpClientFactory httpFactory, int index, string savePath)
        {
            _httpFactory = httpFactory;
            _index = index;
            _savePath = savePath;
            _randomImagePath = $"https://loremflickr.com/240/240?id={_index}";
        }
        public async Task DownloadImageAsync(CancellationToken cancellationToken = default)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                OnStarted(new ImageDownloadStartEventArgs(_index, _randomImagePath));

                using var client = _httpFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Get, _randomImagePath);
                var response = await client.SendAsync(request, cancellationToken);

                var filePath = $@"{_savePath}\{_index}.jpg";
                await using var fs = new FileStream(filePath, FileMode.CreateNew);
                await response.Content.CopyToAsync(fs, cancellationToken);

                OnFinished(new ImageDownloadFinishEventArgs(_index, _randomImagePath));
            }
            else
            {
                OnCanceled(new ImageDownloadCancelEventArgs(_index, _randomImagePath));
            }
        }
        protected virtual void OnStarted(ImageDownloadStartEventArgs e)
        {
            Started?.Invoke(this, e);
        }
        protected virtual void OnFinished(ImageDownloadFinishEventArgs e)
        {
            Finished?.Invoke(this, e);
        }
        protected virtual void OnCanceled(ImageDownloadCancelEventArgs e)
        {
            Canceled?.Invoke(this, e);
        }
    }
}