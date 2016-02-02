using BingImageSearchSample.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingImageSearchSample.Events
{
    public class SearchEvent : PubSubEvent<String> { }
    public class CloseTeamItemEvent : PubSubEvent<TeamItemViewModel> { }
}
