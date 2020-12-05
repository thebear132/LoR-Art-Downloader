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
        //The Card class is for JSON converting the website data to a list of these individual cards you can use
        //Just keep it collapsed
        public partial class Card
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("set")]
            public string Set { get; set; }

            [JsonProperty("region")]
            public string Region { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("flavorText")]
            public string FlavorText { get; set; }

            [JsonProperty("artistName")]
            public string ArtistName { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("cost")]
            public long Cost { get; set; }

            [JsonProperty("health")]
            public long Health { get; set; }

            [JsonProperty("attack")]
            public long Attack { get; set; }

            [JsonProperty("keywords")]
            public object[] Keywords { get; set; }

            [JsonProperty("tags")]
            public object[] Tags { get; set; }

            [JsonProperty("associatedCards")]
            public object[] AssociatedCards { get; set; }

            [JsonProperty("similarCards")]
            public string[] SimilarCards { get; set; }

            [JsonProperty("isCollectible")]
            public bool IsCollectible { get; set; }
        }


        static async Task Main(string[] args)
        {
            string mainWeb = "https://lor.mobalytics.gg/cards/";
            string downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); //Gets the local desktop location
            downloadPath += "\\LorArtDownloader";
            createMainDir(downloadPath);


            //Get the web content in the only working way
            //It doesnt execute javascript, so the output is different when in the browser
            string webResponse = await GetResponse(mainWeb).ConfigureAwait(false);
            File.WriteAllText(downloadPath + "\\RawResponse.html", webResponse); //Just debugging


            //Remover det ubrugelige i starten
            int temp = webResponse.IndexOf("\"cards\":");
            webResponse = webResponse.Remove(0, temp + 8); //+8 we dont want "cards": so we also filter out that
            //Remover det ubrugelige i enden
            temp = webResponse.IndexOf("</script><script type=");
            webResponse = webResponse.Remove(temp, webResponse.Length - temp);
            //Remover mere ubrugelige i enden
            temp = webResponse.IndexOf(",\"version\":\"en_us");
            webResponse = webResponse.Remove(temp, webResponse.Length - temp);

            webResponse = webResponse.Replace(",", ",\n"); //Laver nye linjer fordi ellers står det hele på en linje
            string htmlPath = downloadPath + "\\JsonDownloaded(Only for debugging).html";
            File.WriteAllText(htmlPath, webResponse); //Skriver det til desktop så man kan tjekke teksten nemmere




            Card[] cards = JsonConvert.DeserializeObject<Card[]>(webResponse); //Laver alt JSON om til en liste af "Card" objekter

            // string lorPictureArchive = "https://cdn-lor.mobalytics.gg/production/images/set2/en_us/img/card/game/02BW041-full.png";

            string entryUrl, entryPath, fixedTitle, tierIndicator;
            foreach (Card entryCard in cards)
            {
                tierIndicator = ""; //just dont change please
                fixedTitle = entryCard.Title.Replace(":", ""); //Kolon fucker navngivningen op åbenbart
                entryPath = downloadPath + "\\" + entryCard.Region + "\\";
                createMainDir(entryPath);

                if (entryCard.Id.Contains("T")) //Denne if tilføjer dens Tier hvis den har en
                {
                    Console.WriteLine(entryCard.Title + " Contains (T): " + entryCard.Id);

                    if ((entryCard.Id[entryCard.Id.Length - 3]).ToString().Equals("T"))
                    {
                        tierIndicator += " " + entryCard.Id.Substring(entryCard.Id.Length - 3, 3);
                    }
                    else if ((entryCard.Id[entryCard.Id.Length - 2]).ToString().Equals("T"))
                    {
                        tierIndicator += " " + entryCard.Id.Substring(entryCard.Id.Length - 2, 2);
                    }
                    Console.WriteLine("Tier indicater: " + tierIndicator);
                }

                if (entryCard.Type.Equals("Spell") || entryCard.Type.Equals("Ability"))
                {
                    entryPath += "Spells and Abilites\\";
                    createMainDir(entryPath);
                }


                entryPath += fixedTitle + tierIndicator + ".png";

                if (File.Exists(entryPath))
                {
                    Console.WriteLine("Already a file ||| " + entryPath);
                }
                else
                {
                    entryUrl = "https://cdn-lor.mobalytics.gg/production/images/set" + entryCard.Set + "/en_us/img/card/game/" + entryCard.Id + "-full.png";

                    if (downloadPic(entryUrl, entryPath))
                    {
                        Console.WriteLine("Downloaded card art: " + entryCard.Title);
                    }
                    else
                    {
                        Console.WriteLine("Error downloading: " + entryCard.Title);
                    }
                }

            }


            Console.WriteLine("All card art downloaded. See your desktop to see it ;D -TheBear");
            Console.ReadKey();
        }



        static bool createMainDir(string downloadPath)
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

        static bool downloadPic(string url, string path)
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
        private static readonly HttpClient _HttpClient = new HttpClient();
        private static async Task<string> GetResponse(string url)
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
