using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Media;
using AngleSharp.Common;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Windows.Media.Imaging;

namespace MusicMan___Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isPaused;
        private bool isPlaylist;



        public MainWindow()
        {
            InitializeComponent();

            Camunda c = new Camunda();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!isPlaylist)
            {
                _ = ReloadSongsAsync(Directory.GetFiles(Properties.Settings.Default.MusicPath, "*.mp3").ToList());
            }

            if (!PlaylistsXmlExisting())
                CreatePlaylistsXml();
           
            
        }
        private void LvSongs_OnLoaded(object sender, RoutedEventArgs e)
        {
            var contextMenu = LvSongs.Resources["ItemContextMenu"] as ContextMenu ?? throw new NullReferenceException("No ContextMenu found!");

            contextMenu.Items.Clear();

            MenuItem addToPlaylistMenuItem = new()
            {
                Header = "Add To Playlist"
            };
            foreach (var playlist in GetAllPlaylists())
            {
                MenuItem subMenuItem = new MenuItem();
                subMenuItem.Header = playlist.Name;
                subMenuItem.Click += SubMenuItemOnClick;
                addToPlaylistMenuItem.Items.Add(subMenuItem);
            }
            MenuItem deleteSongMenuItem = new()
            {
                Header = "Delete"
            };

            deleteSongMenuItem.Click += DeleteSongMenuItem_Click;

            contextMenu.Items.Add(addToPlaylistMenuItem);
            contextMenu.Items.Add(deleteSongMenuItem);
            if (isPlaylist)
            {
                MenuItem removeFromPlaylistMenuItem = new()
                {
                    Header = "Remove song from this Playlist"
                };

                removeFromPlaylistMenuItem.Click += RemoveFromPlaylistMenuItem_Click;

                contextMenu.Items.Add(removeFromPlaylistMenuItem);
                
            }
        }
        private void OnMusicTabClick(object sender, MouseButtonEventArgs e)
        {

            if (!isPlaylist)
            {
                _ = ReloadSongsAsync(Directory.GetFiles(Properties.Settings.Default.MusicPath, "*.mp3").ToList());
            }
            else
            {
                BackToSongsBtn.Visibility = Visibility.Visible;
                var playlist = GetAllPlaylists().FirstOrDefault(x => x.Name.Equals(SongsLbl.Content));
                _ = ReloadSongsAsync(LoadSongsFromPlaylist(playlist.Name));

            }
            
        }

        private void RemoveFromPlaylistMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var song = (Music)LvSongs.SelectedItem;
            var playlist = GetAllPlaylists().FirstOrDefault(x => x.Name.Equals(SongsLbl.Content));
            RemoveSongFromPlaylist(song, playlist);
            _ = ReloadSongsAsync(LoadSongsFromPlaylist(playlist.Name));

        }

        private void SubMenuItemOnClick(object sender, RoutedEventArgs e)
        {
            var plName = (MenuItem)sender;
            var currentPlaylist = GetAllPlaylists().FirstOrDefault(x => x.Name == plName.Header.ToString());
            AddSongToPlaylist((Music)LvSongs.SelectedItem, currentPlaylist);
        }

        private void DeleteSongMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var song = (Music)LvSongs.SelectedItem;
            File.Delete(song.FilePath);
            _ = ReloadSongsAsync(Directory.GetFiles(Properties.Settings.Default.MusicPath, "*.mp3").ToList());
        }

        private void CreatePlaylistsXml()
        {
            XDocument doc = new XDocument();
            XElement rootElement = new XElement("Playlists");

            doc.Add(rootElement);

            doc.Save(Properties.Settings.Default.MusicPath + "/Playlists.xml");
        }

        private bool PlaylistsXmlExisting()
        {
            if (File.Exists(Properties.Settings.Default.MusicPath + "/Playlists.xml"))
                return true;

            return false;
        }

        private async void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            string videoId = UrlLbl.Text.Contains("https://www.youtube.com/watch?v=") ? UrlLbl.Text.Substring(UrlLbl.Text.LastIndexOf("=", StringComparison.OrdinalIgnoreCase) + 1) : throw new NullReferenceException("Invalid Url!");

            YoutubeClient youtubeClient = new YoutubeClient();
            StreamManifest manifest;


            try
            {
                manifest = await RetrieveStreamManifest(videoId, youtubeClient);
            }
            catch
            {
                return;
            }

            var audioStream = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();


            try
            {
                var video = await youtubeClient.Videos.GetAsync(videoId);
                var downloadPath = Properties.Settings.Default.MusicPath + @$"\{video.Title}.mp3";
                await DownloadAudio(youtubeClient, audioStream, downloadPath);

            }
            catch (Exception)
            {
                return;
            }

            UrlLbl.Text = "";
        }

        private async Task<StreamManifest> RetrieveStreamManifest(string videoId, YoutubeClient client)
        {
            return await client.Videos.Streams.GetManifestAsync(videoId);
        }

        private async Task DownloadAudio(YoutubeClient client, IStreamInfo streamInfo, string downloadPath)
        {
            await client.Videos.Streams.DownloadAsync(streamInfo, downloadPath);
        }
        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isPlaylist)
            {
                _ = ReloadSongsAsync(Directory.GetFiles(Properties.Settings.Default.MusicPath, "*.mp3").ToList());
            }
            else
            {
                var playlist = GetAllPlaylists().FirstOrDefault(x => x.Name.Equals(SongsLbl.Content));
                _ = ReloadSongsAsync(LoadSongsFromPlaylist(playlist.Name));
            }

        }
        private async Task ReloadSongsAsync(List<string> songs)
        {
            YoutubeClient youtubeClient = new YoutubeClient();

            List<Music> musicList = new List<Music>();
            if (songs.Any())
            {
                foreach (var song in songs)
                {
                    var songTitle = Path.GetFileName(song).Replace(".mp3", "");
                    await foreach (var result in youtubeClient.Search.GetVideosAsync(songTitle))
                    {
                        if (result.Title.Trim() == songTitle.Trim())
                        {
                            Music currentSong = new Music
                            {
                                Index = songs.IndexOf(song),
                                FilePath = song,
                                Title = GetSongName(songTitle),
                                Artist = result.Author.Title ?? "",
                                ImageUrl = result.Thumbnails.FirstOrDefault()?.Url,
                                Duration = result.Duration.ToString()
                            };
                            musicList.Add(currentSong);
                            break;
                        }
                    }
                }
            }
            LvSongs.ItemsSource = musicList;

        }

        //private void SongItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    ContextMenu cm = FindResource("songContextMenu") as ContextMenu;
        //    cm.PlacementTarget = sender as ListViewItem;
        //    cm.IsOpen = true;
        //}

        private void DoubleClickPlay(object sender, RoutedEventArgs e)
        {
            PlaySong(0);
        }

        private void PlaylistTab_Focus(object sender, RoutedEventArgs e)
        {
            PlaylistsLv.Items.Clear();

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

                    XName name = "Artist";
                    song.Artist = songElement.Attribute(name).Value;
                    name = "FilePath";
                    song.FilePath = songElement.Attribute(name).Value;
                    name = "ImageUrl";
                    song.ImageUrl = songElement.Attribute(name).Value;
                    name = "Duration";
                    song.Duration = songElement.Attribute(name).Value;
                    name = "Title";
                    song.Title = songElement.Attribute(name).Value;

                    songs.Add(song);
                }

                playlist.AssignList(songs);

                playlists.Add(playlist);
            }

            return playlists;
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            PlaySong(-1);
        }

        private void NewPlaylist_Btn(object sender, RoutedEventArgs e)
        {
            AddPlaylist addPlaylistDialog = new AddPlaylist();

            addPlaylistDialog.ShowDialog();
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (isPaused)
            {
                if (MePlayer.IsLoaded)
                {
                    MePlayer.LoadedBehavior = MediaState.Manual;
                    MePlayer.Play();
                    isPaused = false;
                }
            }
            else
            {
                PlaySong(0);
            }

        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            MePlayer.Pause();
            isPaused = true;
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            PlaySong(+1);
        }

        private void DelPlaylist_Btn(object sender, RoutedEventArgs e)
        {
            if (PlaylistsLv.SelectedItem == null)
            {
                return;
            }

            ListViewItem selectedPlaylist = (ListViewItem)PlaylistsLv.SelectedItem;

            RemovePlaylistFromXml(selectedPlaylist.Content.ToString());

            PlaylistsLv.Items.Remove(selectedPlaylist);
        }

        private void RemovePlaylistFromXml(string playlistName)
        {
            XDocument doc = XDocument.Load(Properties.Settings.Default.MusicPath + "/Playlists.xml");

            doc.Root.Element(playlistName).RemoveNodes();

            var q = from node in doc.Descendants(playlistName)
                    select node;
            q.ToList().ForEach(x => x.Remove());

            doc.Save(Properties.Settings.Default.MusicPath + "/Playlists.xml");
        }


        private void PlaySong(int prevNext)
        {
            Music song;
            if (prevNext != 0)
            {
                var index = LvSongs.SelectedIndex + prevNext;
                if (index != -1 && index < LvSongs.Items.Count)
                {
                    song = (Music)LvSongs.Items.GetItemAt(index);
                    LvSongs.SelectedItem = song;
                }
                else
                {
                    song = null;
                }

            }
            else
            {
                song = (Music)LvSongs.SelectedItem;
            }


            if (song != null)
            {
                LbCurrentSong.Content = song.Title;
                ImgThumbnail.Source = new BitmapImage(new Uri(song.ImageUrl));
                MePlayer.Source = new Uri(song.FilePath);
                if (MePlayer.IsLoaded)
                {
                    MePlayer.LoadedBehavior = MediaState.Manual;
                    MePlayer.Play();
                }
            }

        }

        private string GetSongName(string songTitle)
        {

            Regex regex = new Regex("^.*-.*\\(.*\\)$");
            if (regex.IsMatch(songTitle))
            {
                var hyphen = songTitle.IndexOf("-", StringComparison.OrdinalIgnoreCase);
                songTitle = songTitle.Substring(hyphen + 1, songTitle.IndexOf("(", StringComparison.OrdinalIgnoreCase) - hyphen - 2).Trim();
            }


            return songTitle;
        }
        private void AddSongToPlaylist(Music song, Playlist playlist)
        {
            playlist.Songs.Add(song);

            XDocument doc = XDocument.Load(Properties.Settings.Default.MusicPath + "/Playlists.xml");

            XElement playlistElement = doc.Root.Element(playlist.Name);

            XElement songElement = new XElement(song.Title.Replace(" ", ""));

            songElement.Add(new XAttribute("Artist", song.Artist));
            songElement.Add(new XAttribute("Title", song.Title));
            songElement.Add(new XAttribute("Duration", song.Duration));
            songElement.Add(new XAttribute("FilePath", song.FilePath));
            songElement.Add(new XAttribute("ImageUrl", song.ImageUrl));


            if (playlistElement.Element(songElement.Name) != null)
            {
                var meBoxResult = MessageBox.Show($"Are you sure you want to add {song.Title} to this Playlist again?",
                    "Duplicate Song in Playlist", MessageBoxButton.YesNo);
                if (meBoxResult == MessageBoxResult.Yes)
                {
                    playlistElement.Add(songElement);
                }
            }
            else
            {
                playlistElement.Add(songElement);
            }


            doc.Save(Properties.Settings.Default.MusicPath + "/Playlists.xml");
        }
        public void RemoveSongFromPlaylist(Music song, Playlist playlist)
        {
            playlist.Songs.Remove(song);

            XDocument doc = XDocument.Load(Properties.Settings.Default.MusicPath + "/Playlists.xml");

            XElement playlistElement = doc.Root.Element(playlist.Name);

            playlistElement.Element(song.Title.Replace(" ", "")).Remove();


            doc.Save(Properties.Settings.Default.MusicPath + "/Playlists.xml");
        }

        private void DoubleClickPlaylist(object sender, MouseButtonEventArgs e)
        {
            ListViewItem lvItem = (ListViewItem)sender;

            _ = ReloadSongsAsync(LoadSongsFromPlaylist(lvItem.Content.ToString()));
            isPlaylist = true;
            SongsLbl.Content = lvItem.Content;
        }

        private List<string> LoadSongsFromPlaylist(string playlistName)
        {
            XDocument doc = XDocument.Load(Properties.Settings.Default.MusicPath + "/Playlists.xml");
            XElement playlistElement = doc.Root.Element(playlistName);
            var dirList = Directory.GetFiles(Properties.Settings.Default.MusicPath, "*.mp3").ToList();
            List<string> songs = new List<string>();
            foreach (var xElement in playlistElement.Elements())
            {
                if (playlistElement.Elements().Any())
                {
                    if (dirList.Contains((string)xElement.Attribute(XName.Get(nameof(Music.FilePath)))))
                    {
                        songs.Add((string)xElement.Attribute(XName.Get(nameof(Music.FilePath))));
                    }
                }

            }

            return songs;
        }


        private void BackToSongsBtn_Click(object sender, RoutedEventArgs e)
        {
            isPlaylist = false;
            _ = ReloadSongsAsync(Directory.GetFiles(Properties.Settings.Default.MusicPath, "*.mp3").ToList());
            BackToSongsBtn.Visibility = Visibility.Hidden;
            SongsLbl.Content = "All Songs";
        }


        
    }


}
//private string RetrieveVideoId(string videoUrl)
//{
//    string[] videoParts = videoUrl.Split("/watch?v=");
//    string videoId;

//    try
//    {
//        videoId = videoParts[1];
//    }
//    catch
//    {
//        return null;
//    }

//    return videoId;
//}

//if (string.IsNullOrEmpty(videoUrl))
//{
//    MessageBox.Show("Input cannot be empty!", "No Url detected.", MessageBoxButton.OK, MessageBoxImage.Error);
//    return;
//}


//if (new Uri(videoUrl).Host != "www.youtube.com")
//{
//    MessageBox.Show("The link you've entered is invalid!", "Invalid Url detected", MessageBoxButton.OK, MessageBoxImage.Error);
//    return;
//}


//string videoId =  RetrieveVideoId(videoUrl);

//if (string.IsNullOrEmpty(videoId))
//{
//    MessageBox.Show("The link you've entered is invalid!", "Invalid Url detected", MessageBoxButton.OK, MessageBoxImage.Error);
//    return;
//}
//private void MenuItemAddToPlaylist_Click(object sender, RoutedEventArgs e)
//{
//    MenuItem mi = sender as MenuItem;
//    ContextMenu cm = mi.Parent as ContextMenu;
//    ListViewItem songItem = cm.PlacementTarget as ListViewItem;
//    Music selectedSong = songItem.Content as Music;

//    AddSongToPlaylist addSongDialog = new AddSongToPlaylist(selectedSong);
//    addSongDialog.ShowDialog();
//}
