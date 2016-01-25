using PhotoAlbum.Events;
using PhotoAlbum.Services.BingService;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Template10.Controls;
using Windows.UI.Xaml.Navigation;

namespace PhotoAlbum.ViewModels
{
    public interface IVewModel { }

    public class MainPageTeamViewModel : Mvvm.ViewModelBase
    {
        SubscriptionToken _token1 = null;
        SubscriptionToken _token2 = null;
        public MainPageTeamViewModel()
        {
            //if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            //    Value = "Design time value";

            _token1 = EventAggregator.Istance<SearchEvent>().Subscribe((query) => Search(query));
            _token2 = EventAggregator.Istance<CloseTeamItemEvent>().Subscribe((item) => PresentationItemCollection.Remove(item));
            PresentationItemCollection.Add(new ToolBarViewModel());
        }

        private async void Search(string query)
        {
            ObservableItemCollection<BingImage> images = await BingService.SearchImagesAsync(query, 10);

            //BingImageSearchService serv = new BingImageSearchService();
            //ObservableCollection<BingImage> images = await serv.SearchImagesAsync(query);

            foreach (BingImage image in images)
                PresentationItemCollection.Add(new TeamItemViewModel() { BingImage = image });
        }

        private ObservableCollection<IVewModel> _presentationItemCollection;

        public ObservableCollection<IVewModel> PresentationItemCollection
        {
            get
            {
                return _presentationItemCollection = this._presentationItemCollection ?? new ObservableCollection<IVewModel>();
            }

            set
            {
                Set(ref _presentationItemCollection, value); ;
            }
        }

    }
}

