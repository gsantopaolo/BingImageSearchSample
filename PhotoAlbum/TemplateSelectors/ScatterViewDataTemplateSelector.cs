using PhotoAlbum.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PhotoAlbum.TemplateSelectors
{
    public class ScatterViewDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ToolBarTemplate { get; set; }
        public DataTemplate TeamItemTemplate { get; set; }



        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            //return base.SelectTemplateCore(item, container);
            DataTemplate retVal = base.SelectTemplateCore(item, container);
            //DataTemplate retVal = null;

            if (item is ToolBarViewModel)
                retVal = ToolBarTemplate;
            else if (item is TeamItemViewModel)
                retVal = TeamItemTemplate;
            return retVal;
        }
    }

    public enum ScatterViewTypes
    {
        ToolBar,
        Item
    }
}
