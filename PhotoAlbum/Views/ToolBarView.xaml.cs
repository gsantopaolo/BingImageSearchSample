using PhotoAlbum.ViewModels;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PhotoAlbum.Views
{
    public sealed partial class ToolBarView : UserControl
    {
        public ToolBarView()
        {
            this.InitializeComponent();
            this.Loaded += ToolBarView_Loaded;
        }

        private void ToolBarView_Loaded(object sender, RoutedEventArgs e)
        {
            object o = this.DataContext;
        }

        // strongly-typed view models enable x:bind
        public ToolBarViewModel ViewModel => this.DataContext as ToolBarViewModel;
    }
}
