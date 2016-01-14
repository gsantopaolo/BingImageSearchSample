using PhotoAlbum.ViewModels;
using Windows.UI.Xaml.Controls;

namespace PhotoAlbum.Views
{
    public sealed partial class MainPageHandleld : Page
    {
        public MainPageHandleld()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Disabled;
        }

        // strongly-typed view models enable x:bind
        public MainPageHandheldViewModel ViewModel => this.DataContext as MainPageHandheldViewModel;

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
    }
}
