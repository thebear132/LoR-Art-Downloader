using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Net;


namespace P_LOR_Art_downloader //DEN GEMMER IKKE ANDRE KORT MED SAMME NAVN SÅ F.EKS. CHAMPIONKORTENES LEVELED UP VERSIONER. Det skal fikses
{
    class Program
    {
        public static string downloadPath;
        static async Task Main(string[] args)
        {
            downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            downloadPath += "\\LorArtDownloader";
            Program.createMainDir(downloadPath);


            Console.WriteLine("Choose\n 1|Scrape\n 2|Brute");
            int input = 0;

            try
            {
                input = Convert.ToInt32(Console.ReadLine());
            }
            catch
            {
                Environment.Exit(1);
            }


            if (input == 1)
            {
                Console.WriteLine("Scrape starting");

                await Task.Run(() => ScrapeDownloader.Start());
                //await Scrape();
            }
            else if (input == 2)
            {
                Console.WriteLine("Brute starting");

                await Task.Run(() => BruteDownloader.Start());
                //await BruteDownloader.Start();
            }

        }

        public static bool createMainDir(string downloadPath)
        {
            //Console.WriteLine("The download path is " + downloadPath);
            try
            {
                if (Directory.Exists(downloadPath))
                {
                    return true;
                }

                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(downloadPath);
                Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(downloadPath));
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
                return false;
            }
        }

        public static bool downloadPic(string url, string path)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadFile(new Uri(url), path);
                    // OR 
                    //client.DownloadFileAsync(new Uri(url), @"c:\temp\image35.png");
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR ENCOUNTERED");
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Url was: " + url + " ||| Path was: " + path);
                    return false;
                }
            }
        }



        //METHOD TO SEND GET REQUEST DISGUISED AS BROWSER (Mozilla). Else it wont fucking work i dont fucking now why
        public static readonly HttpClient _HttpClient = new HttpClient();
        public static async Task<string> GetResponse(string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url)))
            {
                request.Headers.TryAddWithoutValidation("Accept", "text/html, application/xhtml+xml, application/xml");
                request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                request.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

                using (var response = await _HttpClient.SendAsync(request).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                    using (var streamReader = new StreamReader(decompressedStream))
                    {
                        return await streamReader.ReadToEndAsync().ConfigureAwait(false);
                    }
                }
            }
        }


    }
}
