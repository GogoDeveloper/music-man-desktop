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
using System.Windows.Navigation;
using System.Windows.Shapes;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using System.IO;

namespace MusicMan___Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string videoTitle { get; set; }
        string videoAuthor { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            RelaodSongs();

        }

        private async void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            string videoUrl = UrlLbl.Text;

            if (string.IsNullOrEmpty(videoUrl))
            {
                MessageBox.Show("Input cannot be empty!", "No Url detected.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            if (!(new Uri(videoUrl).Host == "www.youtube.com"))
            {
                MessageBox.Show("The link you've entered is invalid!", "Invalid Url detected", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            string videoId =  RetrieveVideoId(videoUrl);

            if (string.IsNullOrEmpty(videoId))
            {
                MessageBox.Show("The link you've entered is invalid!", "Invalid Url detected", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


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
                await DonwloadAudio(youtubeClient, audioStream);
                var downloadPath = Properties.Settings.Default.MusicPath + @$"\{videoTitle}.mp3";
                var file = TagLib.File.Create(downloadPath);
                file.Tag.AlbumArtists = new string[] { $"{videoAuthor}" };

            }
            catch (Exception ex)
            {
                return;
            }

            UrlLbl.Text = "";
        }

        private string RetrieveVideoId(string videoUrl)
        {
            string[] videoParts = videoUrl.Split("/watch?v=");
            string videoId;

            try
            {
                videoId = videoParts[1];
            }
            catch
            {
                return null;
            }

            return videoId;
        }

        private async Task<StreamManifest> RetrieveStreamManifest(string videoId, YoutubeClient client)
        {
            var video = await client.Videos.GetAsync(videoId);
            videoTitle = video.Title;
            videoAuthor = video.Author.Title;
            return await client.Videos.Streams.GetManifestAsync(videoId);

        }

        private async Task DonwloadAudio(YoutubeClient client, IStreamInfo streamInfo)
        {

            string downloadPath = Properties.Settings.Default.MusicPath + @$"\{videoTitle}.mp3";
            await client.Videos.Streams.DownloadAsync(streamInfo, downloadPath);
            
            
            

        }
        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            RelaodSongs();
        }
        private void RelaodSongs()
        {
            List<Music> musicList = new List<Music>();
            List<string> songs = Directory.GetFiles(Properties.Settings.Default.MusicPath, "*.mp3").ToList();
            if (songs.Any())
            {
                foreach (var song in songs)
                {
                    var file = TagLib.File.Create(song);
                    
                   
                    Music currentSong = new Music
                    {
                        FilePath = song,
                        Title = System.IO.Path.GetFileName(song).Replace(".mp3", ""),
                        Author = file.Tag.AlbumArtists!= null ? file.Tag.AlbumArtists.FirstOrDefault() : ""
                    };



                    musicList.Add(currentSong);
                }
            }
            lvSongs.ItemsSource = musicList;
        }


    }
}
