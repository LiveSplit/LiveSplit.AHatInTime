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

        public string UpdateURL
        {
            get { return "http://livesplit.org/update/"; }
        }

        public Version Version
        {
            get { return new Version(); }
        }

        public string XMLURL
        {
            get { return "http://livesplit.org/update/Components/noupdates.xml"; }
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
