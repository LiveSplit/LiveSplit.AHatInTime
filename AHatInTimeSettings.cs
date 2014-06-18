using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LiveSplit.AHatInTime
{
    public partial class AHatInTimeSettings : UserControl
    {
        public String Path { get; set; }

        public AHatInTimeSettings()
        {
            InitializeComponent();
            txtPath.DataBindings.Add("Text", this, "Path", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void btnPath_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Filter = "A Hat In Time Executable (HatinTimeGame.exe)|HatinTimeGame.exe|All Files (*.*)|*.*",
                FileName = Path
            };
            var result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                Path = ofd.FileName;
                txtPath.Text = Path;
            }
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            var settingsNode = document.CreateElement("Settings");

            var pathNode = document.CreateElement("Path");
            pathNode.InnerText = Path;
            settingsNode.AppendChild(pathNode);

            return settingsNode;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            Path = settings["Path"].InnerText;
        }
    }
}
