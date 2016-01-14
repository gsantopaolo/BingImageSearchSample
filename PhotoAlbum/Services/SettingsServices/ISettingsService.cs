using System;
using Windows.UI.Xaml;

namespace PhotoAlbum.Services.SettingsServices
{
    public interface ISettingsService
    {
        bool UseShellBackButton { get; set; }
        bool UserInteractionMode { get; set; }
        ApplicationTheme AppTheme { get; set; }
        TimeSpan CacheMaxDuration { get; set; }
    }
}
