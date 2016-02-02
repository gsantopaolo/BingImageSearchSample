using BingImageSearchSample.Events;
using BingImageSearchSample.Models;
using BingImageSearchSample.Services.BingService;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Template10.Controls;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;

namespace BingImageSearchSample.ViewModels
{
    public interface IVewModel { }


    public class MainPageTeamViewModel : Mvvm.ViewModelBase
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
            //// Get the app's installation folder. 
            //var appFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

            //// Get the subfolders in the current folder. 
            //var subfoldersPromise = appFolder.GetFoldersAsync();
            //subfoldersPromise.(Function getFoldersSuccess(subfolders) {

            //    // Iterate over the results and print the list of folders 
            //    // and files to the Visual Studio Output window. 
            //    subfolders.forEach(function forEachSubfolder(folder) {
            //        console.log(folder.name);

            //        // To iterate over the files in each folder, 
            //        // uncomment the following lines. 
            //        // var getFilesPromise = folder.getFilesAsync(); 
            //        // getFilesPromise.done(function getFilesSuccess(files) { 
            //        //     console.log(folder.name); 
            //        //     files.forEach(function forEachFile(file) { 
            //        //         console.log(".", file.name); 
            //        //     }); 
            //        // }); 
            //    });
            //});

            //ObservableItemCollection<Album> list = new ObservableItemCollection<Album>();
            //IReadOnlyList<StorageFolder> var = Windows.Storage.ApplicationData.Current.LocalFolder.GetFoldersAsync()

            IsLoading = true;
            ObservableItemCollection<BingImage> images = await BingService.SearchImagesAsync(query, 10);

            //BingImageSearchService serv = new BingImageSearchService();
            //ObservableCollection<BingImage> images = await serv.SearchImagesAsync(query);
            if (images != null)
            {
                foreach (BingImage image in images)
                    PresentationItemCollection.Add(new TeamItemViewModel() { BingImage = image });
            }
            IsLoading = false;
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

