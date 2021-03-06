﻿using BingImageSearchSample.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace BingImageSearchSample.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPageTeam : Page
    {
        public MainPageTeam()
        {
            this.InitializeComponent();
            var h = Shell.HamburgerMenu;
            h.HamburgerButtonVisibility = Visibility.Collapsed;
            h.DisplayMode = SplitViewDisplayMode.Overlay;
            h.VisualStateNarrowMinWidth = -1;
            h.VisualStateNormalMinWidth = -1;
            h.VisualStateWideMinWidth = -1;
            h.IsOpen = false;
            
        }

        // strongly-typed view models enable x:bind
        public MainPageTeamViewModel ViewModel => this.DataContext as MainPageTeamViewModel;
    }
}
