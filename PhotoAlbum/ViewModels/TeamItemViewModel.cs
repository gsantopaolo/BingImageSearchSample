using PhotoAlbum.Services.BingService;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace PhotoAlbum.ViewModels
{
    public class TeamItemViewModel : PhotoAlbum.Mvvm.ViewModelBase, IVewModel
    {
        public TeamItemViewModel()
        {
            //if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            //    Value = "Designtime value";
        }


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

        private BingImage _bingImage = null;
        public BingImage BingImage
        {
            get { return _bingImage; }
            set
            {
                Set(ref _bingImage, value);
                if (_bingImage.MediaFilePath != null)
                {
                    IsLoading = true;
                    Task.Run(async () =>
                    {
                        await LoadImage(_bingImage.MediaFilePath, Image);

                        IsLoading = false;
                    });
                }
            }
        }

        private async Task LoadImage(string file, BitmapImage img)
        {
            try
            {


                Windows.Storage.StorageFile storageFile = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(_bingImage.MediaFilePath);

                using (FileRandomAccessStream stream = (FileRandomAccessStream)await storageFile.OpenAsync(FileAccessMode.Read))
                {
                    
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        img.SetSource(stream);
                    });
                }

                //return retVal;
            }
            catch(Exception e)
            {
                //return null;
            }
        }

        private BitmapImage _image = new BitmapImage();
        public BitmapImage Image
        {
            get { return _image; }
            set { Set(ref _image, value); }
        }
    }
}

