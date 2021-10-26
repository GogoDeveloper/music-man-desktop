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
using NAudio.Wave;

namespace MusicMan___Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Music> musicList = new List<Music>();

        public MainWindow()
        {
            InitializeComponent();
            lvSongs.ItemsSource = musicList;
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
            

            string videoId = RetrieveVideoId(videoUrl);

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
            }
            catch
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
            return await client.Videos.Streams.GetManifestAsync(videoId);
        }

        private async Task DonwloadAudio(YoutubeClient client, IStreamInfo streamInfo)
        {
            string donwloadPath = Properties.Settings.Default.MusicPath + $"/temp.mp3";
            await client.Videos.Streams.DownloadAsync(streamInfo, donwloadPath);
        }

        private void MusicTab_Focus(object sender, RoutedEventArgs e)
        {
            string[] songs = Directory.GetFiles(Properties.Settings.Default.MusicPath);

            foreach (var song in songs)
            {
                Music currentSong = null;
                //Mp3FileReader reader = new Mp3FileReader(Properties.Settings.Default.MusicPath + song);

                //Music currentSong = new Music
                //{
                //    FilePath = Properties.Settings.Default.MusicPath + song,
                //    Length = reader.TotalTime,
                //    Title = song.Replace(".mp3", "")
                //};
                //musicList.Add(currentSong);



                currentSong.FilePath = Properties.Settings.Default.MusicPath + song;

                using (Mp3FileReader reader = new Mp3FileReader(Properties.Settings.Default.MusicPath + song))
                {
                    currentSong.Length = reader.TotalTime;
                }

                currentSong.Title = song.Replace(".mp3", "");
            }
        }
    }
}
