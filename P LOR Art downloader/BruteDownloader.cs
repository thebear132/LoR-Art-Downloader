using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P_LOR_Art_downloader
{
    class BruteDownloader
    {
        public static async Task Start()
        {
            string mainWeb = "https://lor.mobalytics.gg/cards/";

            //Get the web content in the only working way
            //It doesnt execute javascript, so the output is different when in the browser
            string webResponse = await Program.GetResponse(mainWeb).ConfigureAwait(false);
            File.WriteAllText(Program.downloadPath + "\\BruteRawResponse.html", webResponse); //Just debugging

            //https://cdn-lor.mobalytics.gg/production/images/set2/en_us/img/card/game/02BW041-full.png

            webResponse.Remove(1, webResponse.IndexOf("css-ov4b0x"));
            Console.WriteLine(webResponse);
            File.WriteAllText(Program.downloadPath + "\\BruteRawResponse2.html", webResponse); //Just debugging

        }
    }
}
