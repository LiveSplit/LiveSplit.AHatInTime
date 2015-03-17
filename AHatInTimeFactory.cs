using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveSplit.AHatInTime
{
    public class AHatInTimeFactory : IComponentFactory
    {
        public string ComponentName
        {
            get { return "A Hat In Time Auto Splitter"; }
        }

        public IComponent Create(LiveSplitState state)
        {
            return new AHatInTimeComponent(state);
        }

        public string UpdateName
        {
            get { return ComponentName; }
        }

        public string XMLURL
        {
#if RELEASE_CANDIDATE
#else
            get { return "http://livesplit.org/update/Components/update.LiveSplit.AHatInTime.xml"; }
#endif
        }

        public string UpdateURL
        {
#if RELEASE_CANDIDATE
#else
            get { return "http://livesplit.org/update/"; }
#endif
        }

        public Version Version
        {
            get { return Version.Parse("1.1.3"); }
        }

        public ComponentCategory Category
        {
            get { return ComponentCategory.Control; }
        }

        public string Description
        {
            get { return "Removes loading times and auto splits for A Hat in Time"; }
        }
    }
}
