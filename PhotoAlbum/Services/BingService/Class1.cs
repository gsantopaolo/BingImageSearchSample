using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Template10.Mvvm;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Syndication;

namespace PhotoAlbum.Services.BingService
{
    public class BingImage : BindableBase
    {
        private bool _isLoading;

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }

            set
            {
                Set(ref _isLoading, value); ;
            }
        }

        private Guid _id;

        public Guid Id
        {
            get { return _id; }
            set
            {
                _id = value;
                RaisePropertyChanged("Id");
            }
        }

        private string _title;

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged("Title");
            }
        }

        private string _mediaUrl;

        public string MediaUrl
        {
            get { return _mediaUrl; }
            set
            {
                _mediaUrl = value;
                RaisePropertyChanged("MediaUrl");
            }
        }

        private string _mediaFilePath;

        public string MediaFilePath
        {
            get { return _mediaFilePath; }
            set
            {
                _mediaFilePath = value;
                RaisePropertyChanged("MediaFilePath");
            }
        }

        private string _sourceUrl;

        public string SourceUrl
        {
            get { return _sourceUrl; }
            set
            {
                _sourceUrl = value;
                RaisePropertyChanged("SourceUrl");
            }
        }

        private string _displayUrl;

        public string DisplayUrl
        {
            get { return _displayUrl; }
            set
            {
                _displayUrl = value;
                RaisePropertyChanged("DisplayUrl");
            }
        }

        private BitmapImage _image = new BitmapImage();
        public BitmapImage Image
        {
            get { return _image; }
            set { Set(ref _image, value); }
        }

        private int _width;

        public int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                RaisePropertyChanged("Width");
            }
        }

        private int _height;

        public int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
        }

        private double _fileSize;

        public double FileSize
        {
            get { return _fileSize; }
            set
            {
                _fileSize = value;
                RaisePropertyChanged("FileSize");
            }
        }

        private string _contentType;

        public string ContentType
        {
            get { return _contentType; }
            set
            {
                _contentType = value;
                RaisePropertyChanged("ContentType");
            }
        }


        private BingThumbnail _thub;

        public BingThumbnail Thumb
        {
            get { return _thub; }
            set
            {
                _thub = value;
                _thub.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_thub_PropertyChanged);
            }
        }

        void _thub_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("Thumb");
        }
    }

    public class BingThumbnail : BindableBase
    {
        private string _mediaUrl;

        public string MediaUrl
        {
            get { return _mediaUrl; }
            set
            {
                _mediaUrl = value;
                RaisePropertyChanged("MediaUrl");
            }
        }

        private int _width;

        public int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                RaisePropertyChanged("Width");
            }
        }

        private int _height;

        public int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
        }

        private double _fileSize;

        public double FileSize
        {
            get { return _fileSize; }
            set
            {
                _fileSize = value;
                RaisePropertyChanged("FileSize");
            }
        }

        private string _contentType;

        public string ContentType
        {
            get { return _contentType; }
            set
            {
                _contentType = value;
                RaisePropertyChanged("ContentType");
            }
        }
    }

    internal static class Helpers
    {
        //internal static async Task DisplayTextResultAsync(
        //    HttpResponseMessage response,
        //    string output,
        //    CancellationToken token)
        //{
        //    string responseBodyAsText;
        //    output += SerializeHeaders(response);
        //    responseBodyAsText = await response.Content.ReadAsStringAsync();

        //    token.ThrowIfCancellationRequested();

        //    // Insert new lines.
        //    responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine);

        //    output += responseBodyAsText;
        //}

        //internal static string SerializeHeaders(HttpResponseMessage response)
        //{
        //    StringBuilder output = new StringBuilder();

        //    // We cast the StatusCode to an int so we display the numeric value (e.g., "200") rather than the
        //    // name of the enum (e.g., "OK") which would often be redundant with the ReasonPhrase.
        //    output.Append(((int)response.StatusCode) + " " + response.ReasonPhrase + "\r\n");

        //    SerializeHeaderCollection(response.Headers, output);
        //    SerializeHeaderCollection(response.Content.Headers, output);
        //    output.Append("\r\n");
        //    return output.ToString();
        //}

        internal static void SerializeHeaderCollection(
            IEnumerable<KeyValuePair<string, string>> headers,
            StringBuilder output)
        {
            foreach (var header in headers)
            {
                output.Append(header.Key + ": " + header.Value + "\r\n");
            }
        }

        public static async Task<byte[]> GetHttpAsBytesAsync(string url)
        {
            //build request
            var request = WebRequest.CreateHttp(url);
            request.UseDefaultCredentials = true;
            byte[] bytes;

            //get response
            var response = await request.GetResponseAsync();
            using (var br = new BinaryReader(response.GetResponseStream()))
            {
                using (var ms = new MemoryStream())
                {
                    var lineBuffer = br.ReadBytes(1024);

                    while (lineBuffer.Length > 0)
                    {
                        ms.Write(lineBuffer, 0, lineBuffer.Length);
                        lineBuffer = br.ReadBytes(1024);
                    }

                    bytes = new byte[(int)ms.Length];
                    ms.Position = 0;
                    ms.Read(bytes, 0, bytes.Length);
                }
            }

            return bytes;
        }

        public static async Task SaveBytesToFileAsync(StorageFolder folder, string filename, byte[] bytes)
        {
            var file = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(file, bytes);
        }

        //internal static void CreateHttpClient(ref HttpClient httpClient)
        //{
        //    if (httpClient != null)
        //    {
        //        httpClient.Dispose();
        //    }

        //    // HttpClient functionality can be extended by plugging multiple filters together and providing
        //    // HttpClient with the configured filter pipeline.
        //    IHttpFilter filter = new HttpBaseProtocolFilter();
        //    filter = new PlugInFilter(filter); // Adds a custom header to every request and response message.
        //    httpClient = new HttpClient(filter);

        //    // The following line sets a "User-Agent" request header as a default header on the HttpClient instance.
        //    // Default headers will be sent with every request sent from this HttpClient instance.
        //    httpClient.DefaultRequestHeaders.UserAgent.Add(new HttpProductInfoHeaderValue("Sample", "v8"));
        //}

        //internal static void ScenarioStarted(Button startButton, Button cancelButton, TextBox outputField)
        //{
        //    startButton.IsEnabled = false;
        //    cancelButton.IsEnabled = true;
        //    if (outputField != null)
        //    {
        //        outputField.Text = String.Empty;
        //    }
        //}

        //internal static void ScenarioCompleted(Button startButton, Button cancelButton)
        //{
        //    startButton.IsEnabled = true;
        //    cancelButton.IsEnabled = false;
        //}

        //internal static void ReplaceQueryString(TextBox addressField, string newQueryString)
        //{
        //    string resourceAddress = addressField.Text;

        //    // Remove previous query string.
        //    int questionMarkIndex = resourceAddress.IndexOf("?", StringComparison.Ordinal);
        //    if (questionMarkIndex != -1)
        //    {
        //        resourceAddress = resourceAddress.Substring(0, questionMarkIndex);
        //    }

        //    addressField.Text = resourceAddress + newQueryString;
        //}

        internal static bool TryGetUri(string uriString, out Uri uri)
        {
            // Note that this app has both "Internet (Client)" and "Home and Work Networking" capabilities set,
            // since the user may provide URIs for servers located on the internet or intranet. If apps only
            // communicate with servers on the internet, only the "Internet (Client)" capability should be set.
            // Similarly if an app is only intended to communicate on the intranet, only the "Home and Work
            // Networking" capability should be set.
            if (!Uri.TryCreate(uriString.Trim(), UriKind.Absolute, out uri))
            {
                return false;
            }

            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                return false;
            }

            return true;
        }
    }

    public class BingImageSearchService//: INotifyBingImageDownloadCompleted
    {
        private async void Start_Click(string url)
        {
            Uri resourceAddress;

            // The value of 'AddressField' is set by the user and is therefore untrusted input. If we can't create a
            // valid, absolute URI, we'll notify the user about the incorrect input.
            if (!Helpers.TryGetUri(url, out resourceAddress))
            {
                //rootPage.NotifyUser("Invalid URI.", NotifyType.ErrorMessage);
                return;
            }



            //try
            //{
            //    HttpClient 
            //    HttpResponseMessage response = await httpClient.GetAsync(resourceAddress).AsTask(cts.Token);

            //    await Helpers.DisplayTextResultAsync(response, OutputField, cts.Token);
            //    response.EnsureSuccessStatusCode();

            //    XElement element = XElement.Parse(await response.Content.ReadAsStringAsync());
            //    OutputList.ItemsSource = (
            //        from c in element.Elements("item")
            //        select c.Attribute("name").Value);

            //    //rootPage.NotifyUser("Completed", NotifyType.StatusMessage);
            //}
            //catch (TaskCanceledException)
            //{
            //    //rootPage.NotifyUser("Request canceled.", NotifyType.ErrorMessage);
            //}
            //catch (Exception ex)
            //{
            //    //rootPage.NotifyUser("Error: " + ex.Message, NotifyType.ErrorMessage);
            //}
            //finally
            //{
            //    //Helpers.ScenarioCompleted(StartButton, CancelButton);
            //}
        }

        //private void Cancel_Click(object sender, RoutedEventArgs e)
        //{
        //    cts.Cancel();
        //    cts.Dispose();

        //    // Re-create the CancellationTokenSource.
        //    cts = new CancellationTokenSource();
        //}

        //public void Dispose()
        //{
        //    if (httpClient != null)
        //    {
        //        httpClient.Dispose();
        //        httpClient = null;
        //    }

        //    if (cts != null)
        //    {e
        //        cts.Dispose();
        //        cts = null;
        //    }
        //}


        private const string USER_ID = "g_santopaolo@hotmail.com"; // yourLiveId
        private const string SECURE_ACCOUNT_ID = "1b5R4r91DSdW+4vjmkJN3vJoBJ564V4y57em3YyDOEg=";  // yourMarketplaceAccountKey not your Live password
        private const string SERVICE_URI = "https://api.datamarket.azure.com/Data.ashx/Bing/Search/Image?Query=%27{0}%27&Market=%27en-US%27&Adult=%27Strict%27&ImageFilters=%27Aspect%3aWide%27&$top=50&$format=Atom";
        //                                  https://api.datamarket.azure.com/Data.ashx/Bing/Search/Image?Query=%27valentino%20rossi%27&Market=%27en-US%27&Adult=%27Strict%27&ImageFilters=%27Aspect%3aWide%27&$top=50&$format=Atom
        //                                  https://api.datamarket.azure.com/Data.ashx/Bing/Search/Image?Query=%27valentino%2brossi%27&Market=%27en-US%27&Adult=%27Strict%27&ImageFilters=%27Aspect%3aWide%27&$top=50&$format=Atom"
        //https://api.datamarket.azure.com/Data.ashx/Bing/Search/Image?Query=%27star%20trek%27&Market=%27en-US%27&Adult=%27Strict%27&ImageFilters=%27Aspect%3aWide%27&$top=50&$format=Atom"//"https://api.datamarket.azure.com/Data.ashx/Bing/Search";//"https://api.datamarket.azure.com/Bing/Search/";
        private const string METADATA = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata";
        private const string DATA_SERVICES = "http://schemas.microsoft.com/ado/2007/08/dataservices";
        private static readonly string BING_IMAGES = @"C:\1";

        //static SearchImages()
        //{
        //    //Task.Factory.StartNew(() => DeleteFiles());
        //}


        //private static void DeleteFiles()
        //{
        //    try
        //    {
        //        //System.IO.DirectoryInfo downloadedMessageInfo = new DirectoryInfo(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BING_IMAGES));
        //        System.IO.DirectoryInfo downloadedMessageInfo = new DirectoryInfo(BING_IMAGES);

        //        foreach (FileInfo file in downloadedMessageInfo.GetFiles())
        //        {
        //            file.Delete();
        //        }

        //    }
        //    catch { }
        //}

        //private async Task<IrisMessage> ReencodeAndAnalyze(IRandomAccessStream stream)


        public async Task<ObservableCollection<BingImage>> SearchImagesAsync(string keyWord)
        {

            return await DownloadImagesAsyncInternal(keyWord);
        }

        private async Task<ObservableCollection<BingImage>> DownloadImagesAsyncInternal(string keyWord)
        {
            ObservableCollection<BingImage> imageList = new ObservableCollection<BingImage>();

            try
            {
                #region webrequest to get bing image results
                WebRequest request = (WebRequest)WebRequest.Create(string.Format(SERVICE_URI, WebUtility.UrlEncode(keyWord
                    .Replace(@"""", "").Replace("$", "").Replace("-", "").Replace("+", "").Replace("!", "").Replace("*", "")
                    .Replace("'", "").Replace("(", "").Replace(")", "").Replace(",", ""))
                    ));

                // Request mutual authentication.
                //request.AuthenticationLevel = AuthenticationLevel.MutualAuthRequested;
                // Supply client credentials.
                request.Credentials = new NetworkCredential(USER_ID, SECURE_ACCOUNT_ID);
                string tmpString = null;
                HttpWebResponse response = null;
                try
                {
                    response = (HttpWebResponse)await request.GetResponseAsync();
                    Stream resStream = response.GetResponseStream();

                    StringBuilder sb = new StringBuilder();

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
                            tmpString = Encoding.ASCII.GetString(buf, 0, buf.Length);
                            sb.Append(tmpString);
                        } while (count > 0);

                        read.Dispose();
                    }

                }
                catch
                {

                }
                finally
                {
                    if (response != null)
                        response.Dispose();
                }

                //XDocument xdoc = XDocument.Parse(tmpString);
                SyndicationFeed feed = new SyndicationFeed();
                feed.Load(tmpString);
                #endregion

                #region feed creation

                if (feed != null)
                {
                    foreach (var item in feed.Items)
                    {

                        try
                        {
                            BingImage image = new BingImage();

                            ////item.Content.WriteTo(tempWriter, "Content", "");
                            //tempWriter.Flush();

                            // Get the content as XML. 
                            //var contentXml = Encoding.UTF8.GetString(tempStream.ToArray());
                            var contentDocument = XDocument.Parse(item.Content.Text);// XDocument.Parse(item.InnerText);// XDocument.Parse(contentXml);

                            // Find the properties element. 
                            var propertiesName = XName.Get("properties", METADATA);
                            var propertiesElement = contentDocument.Descendants(propertiesName).FirstOrDefault();

                            // guid
                            var ID = XName.Get("ID", DATA_SERVICES);
                            var IDElement = propertiesElement.Descendants(ID).FirstOrDefault();
                            image.Id = new Guid(IDElement.Value);

                            // string
                            var title = XName.Get("Title", DATA_SERVICES);
                            var titleElement = propertiesElement.Descendants(title).FirstOrDefault();
                            image.Title = titleElement.Value;

                            // string
                            var mediaUrl = XName.Get("MediaUrl", DATA_SERVICES);
                            var mediaUrlElement = propertiesElement.Descendants(mediaUrl).FirstOrDefault();
                            image.MediaUrl = mediaUrlElement.Value;

                            // string
                            var sourceUrl = XName.Get("SourceUrl", DATA_SERVICES);
                            var sourceUrlElement = propertiesElement.Descendants(sourceUrl).FirstOrDefault();
                            image.SourceUrl = sourceUrlElement.Value;

                            // string
                            var displayUrl = XName.Get("DisplayUrl", DATA_SERVICES);
                            var displayUrlElement = propertiesElement.Descendants(displayUrl).FirstOrDefault();
                            image.DisplayUrl = displayUrlElement.Value;

                            // int32
                            var width = XName.Get("Width", DATA_SERVICES);
                            var widthElement = propertiesElement.Descendants(width).FirstOrDefault();
                            int w = 0;
                            Int32.TryParse(widthElement.Value, out w);
                            image.Width = w;

                            // int32
                            var height = XName.Get("Height", DATA_SERVICES);
                            var heightElement = propertiesElement.Descendants(height).FirstOrDefault();
                            int h = 0;
                            Int32.TryParse(heightElement.Value, out h);
                            image.Height = h;

                            // int64
                            var fileSize = XName.Get("FileSize", DATA_SERVICES);
                            var fileSizeElement = propertiesElement.Descendants(fileSize).FirstOrDefault();
                            double f = 0;
                            double.TryParse(fileSizeElement.Value, out f);
                            image.FileSize = f;

                            // string
                            var contentType = XName.Get("ContentType", DATA_SERVICES);
                            var contentTypeElement = propertiesElement.Descendants(contentType).FirstOrDefault();
                            image.ContentType = contentTypeElement.Value;

                            // Find the properties element. 
                            var thumbnailName = XName.Get("Thumbnail", DATA_SERVICES);
                            var thumbnaiElement = contentDocument.Descendants(thumbnailName).FirstOrDefault();
                            image.Thumb = new BingThumbnail();

                            // string
                            var mediaThumbUrl = XName.Get("MediaUrl", DATA_SERVICES);
                            var mediaThumbUrlElement = propertiesElement.Descendants(mediaThumbUrl).FirstOrDefault();
                            image.Thumb.MediaUrl = mediaThumbUrlElement.Value;

                            // string
                            var contentTypeThumbUrl = XName.Get("ContentType", DATA_SERVICES);
                            var contentTypeThumbUrlElement = propertiesElement.Descendants(contentTypeThumbUrl).FirstOrDefault();
                            image.Thumb.ContentType = contentTypeThumbUrlElement.Value;

                            // int32
                            var widthThumb = XName.Get("Width", DATA_SERVICES);
                            var widthThumbElement = propertiesElement.Descendants(widthThumb).FirstOrDefault();
                            int wt = 0;
                            Int32.TryParse(widthThumbElement.Value, out wt);
                            image.Width = wt;

                            // int32
                            var heightThumb = XName.Get("Height", DATA_SERVICES);
                            var heightThumbElement = propertiesElement.Descendants(heightThumb).FirstOrDefault();
                            int ht = 0;
                            Int32.TryParse(heightThumbElement.Value, out ht);
                            image.Height = ht;

                            // int64
                            var fileSizeThumb = XName.Get("FileSize", DATA_SERVICES);
                            var fileSizeThumbElement = propertiesElement.Descendants(fileSizeThumb).FirstOrDefault();
                            double ft = 0;
                            double.TryParse(fileSizeThumbElement.Value, out ft);
                            image.FileSize = ft;

                            imageList.Add(image);
                        }
                        catch (Exception ex)
                        {
                            //Trace.WriteLine(ex.Message, "ERROR");
                        }


                    }

                }
                #endregion

                #region webrequest for image download
                var imList = imageList.OrderByDescending(c => c.Width);//.ThenByDescending(n => n.Height).ToList();//.FirstOrDefault();

                if (imList != null)
                {
                    // provo a scarica una immagine fichè non ci riesco
                    foreach (BingImage im in imList)
                    {
                        try
                        {
                            if (im != null)
                            {

                                string fileName = string.Format("{0}.{1}", Guid.NewGuid(), Path.GetExtension(im.MediaUrl).ToLower());

                                Windows.Storage.StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
                                Windows.Storage.StorageFile file = await folder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);

                                await Helpers.SaveBytesToFileAsync(folder, fileName, await Helpers.GetHttpAsBytesAsync(im.MediaUrl));


                                im.MediaFilePath = fileName;
                                if (im.MediaFilePath == null)
                                {

                                }

                                //imageToFill.Id = im.Id;
                                //imageToFill.ContentType = im.ContentType;
                                //imageToFill.DisplayUrl = im.DisplayUrl;
                                //imageToFill.FileSize = im.FileSize;
                                //imageToFill.Height = im.Height;
                                //imageToFill.MediaFilePath = im.MediaFilePath;
                                //imageToFill.MediaUrl = im.MediaUrl;
                                //imageToFill.SourceUrl = im.SourceUrl;
                                //if (imageToFill.Thumb == null)
                                //    imageToFill.Thumb = new BingThumbnail();
                                //imageToFill.Thumb.ContentType = im.Thumb.ContentType;
                                //imageToFill.Thumb.FileSize = im.Thumb.FileSize;
                                //imageToFill.Thumb.Height = im.Thumb.Height;
                                //imageToFill.Thumb.MediaUrl = im.Thumb.MediaUrl;
                                //imageToFill.Thumb.Width = im.Thumb.Width;
                                //imageToFill.Title = im.Title;
                                //imageToFill.Width = im.Width;

                                //RaiseDownloadCompleted(imageToFill, downloadEventId);
                            }
                        }
                        catch (Exception ex)
                        {
                            //Trace.WriteLine(ex.Message, "ERROR");
                        }
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                //Trace.WriteLine(e.Message, "ERROR");
            }



            return imageList;
        }

    }
}
