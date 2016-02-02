using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Template10.Controls;

namespace BingImageSearchSample.Services.BingService
{
    public static class BingService
    {
        private const string rootUrl = "https://api.datamarket.azure.com/Bing/Search/v1/";

        public static async Task<ObservableItemCollection<BingImage>> SearchImagesAsync(string query, int top = 100)
        {
            ObservableItemCollection<BingImage> retVal = null;
            var search = string.Format("Image?Query=%27{0}%27&$top={1}&$format=json", WebUtility.UrlEncode(query), top);
            WebRequest request = WebRequest.Create(new Uri(string.Format("{0}{1}", rootUrl, search)));
            string content = null;
            WebResponse response = null;

            try
            {
                request.Credentials = new NetworkCredential(API_Keys.Bing_Images_Search.SECURE_ACCOUNT_ID, API_Keys.Bing_Images_Search.SECURE_ACCOUNT_ID);
                response = await request.GetResponseAsync();
                Stream resStream = response.GetResponseStream();

                using (StreamReader read = new StreamReader(resStream))
                {
                    int count = (int)response.ContentLength;
                    int offset = 0;
                    Byte[] buf = new byte[count];
                    do
                    {
                        int n = resStream.Read(buf, offset, count);
                        if (n == 0) break;
                        count -= n;
                        offset += n;
                        content = Encoding.ASCII.GetString(buf, 0, buf.Length);
                    } while (count > 0);

                    read.Dispose();
                }

                var d = JToken.Parse(content);
                var results = d["d"]["results"];
                retVal = JsonConvert.DeserializeObject<ObservableItemCollection<BingImage>>(results.ToString());
            }
            catch(Exception ex)
            {

            }
            finally
            {
                if (response != null)
                    response.Dispose();
            }
            
            return retVal;
        }
    }
}