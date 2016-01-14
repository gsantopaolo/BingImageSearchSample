using PhotoAlbum.Services.BingService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;

namespace PhotoAlbum.ViewModels
{
    public class DetailPageViewModel : PhotoAlbum.Mvvm.ViewModelBase
    {
        public DetailPageViewModel()
        {
            //if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            //    Value = "Designtime value";
        }

        private BingImage _image;
        public BingImage Image { get { return _image; } set { Set(ref _image, value); } }

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (state.ContainsKey(nameof(Image)))
            {
                Image = state[nameof(Image)] as BingImage;
                state.Clear();
            }
            else
            {
                Image = parameter as BingImage;
            }
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            if (suspending)
                state[nameof(Image)] = Image;
            await Task.Yield();
        }

        public override void OnNavigatingFrom(NavigatingEventArgs args)
        {
            args.Cancel = false;
        }
    }
}

