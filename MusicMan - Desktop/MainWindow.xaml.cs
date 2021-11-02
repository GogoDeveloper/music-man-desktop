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

            List<Music> musicList = new List<Music>();
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
                                Duration = (TimeSpan)result.Duration
                            };
                            musicList.Add(currentSong);
                            break;
                        }
                    }
                }
            }
            LvSongs.ItemsSource = musicList;
        }


        private void DoubleClickPlay(object sender, MouseButtonEventArgs e)
        {
            
            
            ListViewItem lvItem = (ListViewItem)sender;

            Music song = (Music)lvItem.DataContext;

            MediaPlayer mediaPlayer = new MediaPlayer();
            mePlayer.Source = new Uri(song.FilePath);
            mediaPlayer.Open(new Uri(song.FilePath));
            mediaPlayer.Play();
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