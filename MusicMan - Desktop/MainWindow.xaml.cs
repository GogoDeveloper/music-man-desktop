using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using System.IO;
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
        private List<Music> musicList;
        public MainWindow()
        {
            InitializeComponent();
            ReloadSongsAsync();
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
            ReloadSongsAsync();
        }
        private async Task ReloadSongsAsync()
        {
            YoutubeClient youtubeClient = new YoutubeClient();

            musicList = new List<Music>();
            List<string> songs = Directory.GetFiles(Properties.Settings.Default.MusicPath, "*.mp3").ToList();
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
                                FilePath = song,
                                Title = songTitle,
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


        private void DoubleClickPlay(object sender, RoutedEventArgs e)
        {
            
            
            
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

                playlist.Songs = songs;

                playlists.Add(playlist);
            }

            return playlists;
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NewPlaylist_Btn(object sender, RoutedEventArgs e)
        {
            AddPlaylist addPlaylistDialog = new AddPlaylist();

            addPlaylistDialog.ShowDialog();
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (MePlayer.IsLoaded)
            {
                MePlayer.LoadedBehavior = MediaState.Manual;
                MePlayer.Play();
            }
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            MePlayer.Pause();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Mouse_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ListViewItem lvItem = (ListViewItem)sender;

                Music song = (Music)lvItem.DataContext;
                LbCurrentSong.Content = song.Title;
                ImgThumbnail.Source = new BitmapImage(new Uri(song.ImageUrl));

                //mePlayer.Source = new Uri(song.FilePath);
                if (MePlayer.IsLoaded)
                {
                    MePlayer.LoadedBehavior = MediaState.Manual;
                    MePlayer.Play();
                }
            }
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