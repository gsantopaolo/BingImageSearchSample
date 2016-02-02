using System;
using Windows.UI.Xaml;

namespace BingImageSearchSample.Services.SettingsServices
{
    public interface ISettingsService
    {
        bool UseShellBackButton { get; set; }
        bool UserInteractionMode { get; set; }
        ApplicationTheme AppTheme { get; set; }
        TimeSpan CacheMaxDuration { get; set; }
    }
}
