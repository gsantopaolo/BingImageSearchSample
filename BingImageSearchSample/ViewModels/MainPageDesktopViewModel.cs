using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace BingImageSearchSample.ViewModels
{
    public class MainPageDesktopViewModel : Mvvm.ViewModelBase
    {
        public MainPageDesktopViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                Value = "Designtime value";
        }

        string _Value = string.Empty;
        public string Value { get { return _Value; } set { Set(ref _Value, value); } }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (state.ContainsKey(nameof(Value)))
            {
                Value = state[nameof(Value)]?.ToString();
                state.Clear();
            }
            else
            {
                Value = parameter?.ToString();
            }
            return Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            if (suspending)
                state[nameof(Value)] = Value;
            await Task.Yield();
        }

        public void GotoDetailsPage()
        {
            NavigationService.Navigate(typeof(Views.DetailPage), Value);
        }

        public void GotoPrivacy()
        {
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);
        }

        public void GotoAbout()
        {
            NavigationService.Navigate(typeof(Views.SettingsPage), 2);
        }

    }
}

