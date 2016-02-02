using System;
using Windows.UI.Xaml;
using System.Threading.Tasks;
using BingImageSearchSample.Services.SettingsServices;
using Windows.ApplicationModel.Activation;


namespace BingImageSearchSample
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki

    sealed partial class App : Template10.Common.BootStrapper
    {

        public App()
        {

            InitializeComponent();
            SplashFactory = (e) => new Views.Splash(e);

            #region App settings

            var _settings = SettingsService.Instance;
            RequestedTheme = _settings.AppTheme;
            CacheMaxDuration = _settings.CacheMaxDuration;
            ShowShellBackButton = _settings.UseShellBackButton;

            #endregion
        }

        //// runs even if restored from state
        //public override async Task OnInitializeAsync(IActivatedEventArgs args)
        //{

        //    // content may already be shell when resuming
        //    if ((Window.Current.Content as Views.Shell) == null)
        //    {
        //        // setup hamburger shell
        //        Template10.Services.NavigationService.NavigationService nav = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);
        //        if (SettingsService.Instance.UserInteractionMode == false)
        //            Window.Current.Content = new Views.Shell(nav);
        //        else
        //            Window.Current.Content = new Views.MainPageTeam();
        //    }
        //    await Task.Yield();
        //}

        // runs even if restored from state
        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            // setup hamburger shell
            var nav = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);
            Window.Current.Content = new Views.Shell((Template10.Services.NavigationService.NavigationService)nav);


            await Task.Yield();
        }

        // runs only when not restored from state
        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // perform long-running load
            await Task.Delay(0);

            // navigate to first page
            if (SettingsService.Instance.UserInteractionMode == false)
                NavigationService.Navigate(typeof(Views.MainPageHandleld));
            else
                NavigationService.Navigate(typeof(Views.MainPageTeam));
        }
    }
}

