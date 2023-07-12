using NeoSmart.AsyncLock;
using System.Diagnostics;
using AdCreativeDeveloperCase.Application.Models;

namespace AdCreativeDeveloperCase.Application.Business
{
    public class ConsoleApplication
    {
        private readonly IHttpClientFactory _httpFactory;
        private SemaphoreSlim _semaphoreSlim;
        private readonly AsyncLock _lock = new AsyncLock();
        private int _counter = 0;
        private DownloadSettings _downloadSettings;
        public ConsoleApplication(IHttpClientFactory httpFactory, DownloadSettings downloadSettings)
        {
            _httpFactory = httpFactory;
            _downloadSettings = downloadSettings;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            #region ReadSettings
            Console.WriteLine("Do you want to read settings from file? (true or false)");
            if (!bool.TryParse(Console.ReadLine(), out var useSettings) || !useSettings)
            {
                _downloadSettings = new DownloadSettings();
                Console.WriteLine("Enter the number of images to download:");

                if (!int.TryParse(Console.ReadLine(), out var downloadCount))
                {
                    Console.WriteLine("You entered wrong data!");
                    Environment.Exit(0);
                }
                _downloadSettings.Count = downloadCount;
                Console.WriteLine($"DownloadCount:{_downloadSettings.Count}");
                Console.WriteLine();

                Console.WriteLine("Enter the maximum parallel download limit:");

                if (!int.TryParse(Console.ReadLine(), out var parallelism))
                {
                    Console.WriteLine("You entered wrong data!");
                    Environment.Exit(0);
                }
                _downloadSettings.Parallelism = parallelism;
                Console.WriteLine($"Parallelism:{parallelism}");
                Console.WriteLine();

                Console.WriteLine("Enter the save path (default: \\outputs");

                _downloadSettings.SavePath = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(_downloadSettings.SavePath))
                {
                    _downloadSettings.SavePath = "\\outputs";
                }

                Console.WriteLine($"SavePath:{_downloadSettings.SavePath}");
                Console.WriteLine();
            }
            #endregion

            _semaphoreSlim = new SemaphoreSlim(_downloadSettings.Parallelism);

            string path = Path.GetDirectoryName(Directory.GetCurrentDirectory()) + _downloadSettings.SavePath;
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            else
            {
                Directory.CreateDirectory(path);
            }

            var tasks = Enumerable.Range(0, _downloadSettings.Count)
                .Select(x => ExecTask(x, path, cancellationToken))
                .ToArray();

            var sw = Stopwatch.StartNew();
            await Task.WhenAll(tasks);
            sw.Stop();

            Console.WriteLine($"ElapsedMilliseconds: {sw.ElapsedMilliseconds}");
        }

        async Task<int> ExecTask(int index, string savePath, CancellationToken cancellationToken = default)
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            try
            {
                var downloader = new ImageDownloader(_httpFactory, index, savePath);
                downloader.Started += async delegate (object? sender, ImageDownloadStartEventArgs args)
                {

                };
                downloader.Finished += async delegate (object? sender, ImageDownloadFinishEventArgs args)
                {
                    using (await _lock.LockAsync())
                    {
                        using (await _lock.LockAsync())
                        {
                            _counter++;
                            Console.WriteLine($"_counter {_counter}/{_downloadSettings.Count}, task ok: {index}");
                            //Console.SetCursorPosition(Console.CursorLeft, 0);
                        }
                    }
                };
                downloader.Canceled += async delegate (object? sender, ImageDownloadCancelEventArgs args)
                {
                    using (await _lock.LockAsync())
                    {
                        using (await _lock.LockAsync())
                        {
                            _counter++;
                            Console.WriteLine($"_counter {_counter}/{_downloadSettings.Count}, task cancel: {index}");
                            //Console.SetCursorPosition(Console.CursorLeft, 0);
                        }
                    }
                };
                await downloader.DownloadImageAsync(cancellationToken);
                return index;
            }
            catch (Exception exception)
            {
                using (await _lock.LockAsync())
                {
                    using (await _lock.LockAsync())
                    {
                        _counter++;
                        Console.WriteLine($"_counter {_counter}/{_downloadSettings.Count}, task error: {index}, message: {exception.Message}");
                        //Console.SetCursorPosition(Console.CursorLeft, 0);
                    }
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
            return index;
        }
    }
}