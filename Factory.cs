using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveSplit.AHatInTime
{
    public class Factory : IComponentFactory
    {
        public string ComponentName
        {
            get { return "A Hat In Time - Time Without Loads"; }
        }

        public IComponent Create(LiveSplitState state)
        {
            return new AHatInTimeComponent(state);
        }

        public string UpdateName
        {
            get { return ""; }
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
            get { return "Removes loading times for A Hat in Time"; }
        }
    }
}
