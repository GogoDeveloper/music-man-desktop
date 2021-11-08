using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace MusicMan___Desktop
{
    /// <summary>
    /// Interaction logic for AddPlaylist.xaml
    /// </summary>
    public partial class AddPlaylist : Window
    {
        public AddPlaylist()
        {
            InitializeComponent();
        }

        private void Create_Btn(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PlaylistNameTb.Text))
            {
                return;
            }

            XDocument doc = XDocument.Load(Properties.Settings.Default.MusicPath + "/Playlists.xml");

            XElement newPlaylist = new XElement(PlaylistNameTb.Text.Replace(" ", ""));

            doc.Root.Add(newPlaylist);
            doc.Save(Properties.Settings.Default.MusicPath + "/Playlists.xml");

            this.Close();
        }

        private void Abort_Btn(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
