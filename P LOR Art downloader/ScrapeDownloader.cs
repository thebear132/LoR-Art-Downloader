using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace P_LOR_Art_downloader
{
    class ScrapeDownloader
    {
        //The Card class is for JSON converting the website data to a list of these individual cards you can use
        //Just keep it collapsed
        partial class Card
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





        public static async Task Start()
        {
            string mainWeb = "https://lor.mobalytics.gg/cards/";
            Program.createMainDir(Program.downloadPath);


            //Get the web content in the only working way
            //It doesnt execute javascript, so the output is different when in the browser
            string webResponse = await Program.GetResponse(mainWeb).ConfigureAwait(false);
            File.WriteAllText(Program.downloadPath + "\\RawResponse.html", webResponse); //Just debugging


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
            string htmlPath = Program.downloadPath + "\\JsonDownloaded(Only for debugging).html";
            File.WriteAllText(htmlPath, webResponse); //Skriver det til desktop så man kan tjekke teksten nemmere




            Card[] cards = JsonConvert.DeserializeObject<Card[]>(webResponse); //Laver alt JSON om til en liste af "Card" objekter

            // string lorPictureArchive = "https://cdn-lor.mobalytics.gg/production/images/set2/en_us/img/card/game/02BW041-full.png";

            string entryUrl, entryPath, fixedTitle, tierIndicator;
            foreach (Card entryCard in cards)
            {
                tierIndicator = ""; //just dont change please
                fixedTitle = entryCard.Title.Replace(":", ""); //Kolon fucker navngivningen op åbenbart
                entryPath = Program.downloadPath + "\\" + entryCard.Region + "\\";
                Program.createMainDir(entryPath);

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
                    Program.createMainDir(entryPath);
                }


                entryPath += fixedTitle + tierIndicator + ".png";

                if (File.Exists(entryPath))
                {
                    Console.WriteLine("Already a file ||| " + entryPath);
                }
                else
                {
                    entryUrl = "https://cdn-lor.mobalytics.gg/production/images/set" + entryCard.Set + "/en_us/img/card/game/" + entryCard.Id + "-full.png";

                    if (Program.downloadPic(entryUrl, entryPath))
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
    }
}
