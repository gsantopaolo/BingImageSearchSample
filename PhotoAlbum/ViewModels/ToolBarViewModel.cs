using PhotoAlbum.Events;
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
    public class ToolBarViewModel : PhotoAlbum.Mvvm.ViewModelBase, IVewModel
    {
        public ToolBarViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                Query = "Designtime value";
        }

        private string _query = "HoloLensDevs";

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
                        EventAggregator.Istance<SearchEvent>().Publish(Query);

                }, null /*() => string.IsNullOrEmpty(Query) == false*/);
                return this._queryCommand;
            }
        }

    }
}

