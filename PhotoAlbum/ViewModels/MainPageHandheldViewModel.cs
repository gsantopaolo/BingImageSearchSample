using PhotoAlbum.Events;
using PhotoAlbum.Services.BingService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Template10.Controls;
using Template10.Mvvm;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace PhotoAlbum.ViewModels
{
    public class MainPageHandheldViewModel : Mvvm.ViewModelBase
    {
        public MainPageHandheldViewModel()
        {
            //if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)

        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            //if (state.ContainsKey(nameof(Value)))
            //{
            //    Value = state[nameof(Value)]?.ToString();
            //    state.Clear();
            //}
            //else
            //{
            //    Value = parameter?.ToString();
            //}
            return Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            //if (suspending)
            //    state[nameof(Value)] = Value;
            await Task.Yield();
        }

        DelegateCommand<BingImage> _openCommand;
        public DelegateCommand<BingImage> OpenCommand
        {
            get
            {
                this._openCommand = this._openCommand ?? new DelegateCommand<BingImage>( image =>
                {
                    GotoDetailsPage(image);
                });
                return this._openCommand;
            }
        }

        public void GotoDetailsPage(BingImage image)
        {
            NavigationService.Navigate(typeof(Views.DetailPage), image);
        }

        public void GotoPrivacy()
        {
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);
        }

        public void GotoAbout()
        {
            NavigationService.Navigate(typeof(Views.SettingsPage), 2);
        }

        private async void Search(string query)
        {

            //foreach(BingImage image in Images)
            //{
            //    image.IsLoading = true;
            //        await LoadImage(image.MediaFilePath, image.Image);
            //    image.IsLoading = false;
            //}

            Images.Clear();
            IsLoading = true;
            
            Images = await BingService.SearchImagesAsync(query, 10);

            IsLoading = false;
        }

        private string _query = "";

        private DelegateCommand _queryCommand;

        public string Query
        {
            get
            {
                return this._query;
            }
            set
            {
                this.Set(ref this._query, value);
                //this.QueryCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand QueryCommand
        {
            get
            {
                this._queryCommand = this._queryCommand ?? new DelegateCommand(() =>
                {
                    //this._repositoriesSource.Query = this.Query;
                    //Repositories.Refresh();

                    if (string.IsNullOrEmpty(Query) == false)
                    {
                        
                        Search(Query);
                    }

                }, null /*() => string.IsNullOrEmpty(Query) == false*/);
                return this._queryCommand;
            }
        }

        private ObservableItemCollection<BingImage> _images;

        public ObservableItemCollection<BingImage> Images
        {
            get
            {
                return _images = this._images ?? new ObservableItemCollection<BingImage>();
            }

            set
            {
                Set(ref _images, value); ;
            }
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

        private async Task LoadImage(string file, BitmapImage img)
        {
            try
            {


                Windows.Storage.StorageFile storageFile = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(file);

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
            catch (Exception e)
            {
                //return null;
            }
        }
    }
}

