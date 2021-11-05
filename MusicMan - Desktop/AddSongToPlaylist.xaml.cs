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
    /// Interaction logic for AddSongToPlaylist.xaml
    /// </summary>
    public partial class AddSongToPlaylist : Window
    {
        public Music SelectedSong;

        public AddSongToPlaylist(Music song)
        {
            InitializeComponent();
            InitializeListView();
            SelectedSong = song;
        }

        public void InitializeListView()
        {
            List<Playlist> playlists = GetAllPlaylists();

            foreach (Playlist playlist in playlists)
            {
                ListViewItem lvItem = new ListViewItem();
                lvItem.Content = playlist.Name;

                PlaylistsLv.Items.Add(lvItem);
            }
        }

        private List<Playlist> GetAllPlaylists()
        {
            XDocument doc = XDocument.Load(Properties.Settings.Default.MusicPath + "/Playlists.xml");
            List<Playlist> playlists = new List<Playlist>();

            foreach (XElement element in doc.Root.Elements())
            {
                Playlist playlist = new Playlist();
                List<Music> songs = new List<Music>();

                playlist.Name = element.Name.ToString();

                foreach (XElement songElement in element.Elements())
                {
                    Music song = new Music();

                    //XName name = "Artist";
                    //song.Artist = songElement.Attribute(name).Value;
                    //name = "FilePath";
                    //song.FilePath = songElement.Attribute(name).Value;
                    //name = "ImageUrl";
                    //song.ImageUrl = songElement.Attribute(name).Value;
                    //name = "Duration";
                    //song.Duration = songElement.Attribute(name).Value;
                    //name = "Title";
                    //song.Title = songElement.Attribute(name).Value;

                    songs.Add(song);
                }

                playlist.AssignList(songs);

                playlists.Add(playlist);
            }

            return playlists;
        }

        private void AbrotBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SelectBtn_Click(object sender, RoutedEventArgs e)
        {
            ListViewItem selectedPlaylist = (ListViewItem)PlaylistsLv.SelectedItem;

            XDocument doc = XDocument.Load(Properties.Settings.Default.MusicPath + "/Playlists.xml");

            XElement selectedPlaylistElement = doc.Root.Element(selectedPlaylist.Content.ToString());

            selectedPlaylistElement.Add(new XAttribute("Artist", SelectedSong.Artist));
            selectedPlaylistElement.Add(new XAttribute("Title", SelectedSong.Title));
            selectedPlaylistElement.Add(new XAttribute("Duration", SelectedSong.Duration));
            selectedPlaylistElement.Add(new XAttribute("FilePath", SelectedSong.FilePath));
            selectedPlaylistElement.Add(new XAttribute("ImageUrl", SelectedSong.ImageUrl));

            doc.Save(Properties.Settings.Default.MusicPath + "/Playlist.xml");

            this.Close();
        }
    }
}
