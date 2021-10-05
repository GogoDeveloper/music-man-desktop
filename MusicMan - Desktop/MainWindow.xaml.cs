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

namespace MusicMan___Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            string videoUrl = UrlLbl.Text;

            if (string.IsNullOrEmpty(videoUrl))
                return;

            if (!(new Uri(videoUrl).Host == "www.youtube.com"))
                return;

            string videoId = RetrieveVideoId(videoUrl);

            if (string.IsNullOrEmpty(videoId))
                return;

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
            string tempPath = "temp.mp3";
            await client.Videos.Streams.DownloadAsync(streamInfo, tempPath);
        }
    }
}
