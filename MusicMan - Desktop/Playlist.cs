using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MusicMan___Desktop
{
    class Playlist
    {
        public string Name { get; set; }
        //Add handled adding / removing (adding and removing from xml not only from list)
        public List<Music> Songs { get; private set; }

        public void AddSong(Music song)
        {
            Songs.Add(song);

            XDocument doc = XDocument.Load(Properties.Settings.Default.MusicPath + "/Playlists.xml");

            XElement playlistElement = doc.Root.Element(Name);
            XElement songElement = new XElement(song.Title.Replace(" ", ""));

            songElement.Add(new XAttribute("Artist", song.Artist));
            songElement.Add(new XAttribute("Title", song.Title));
            songElement.Add(new XAttribute("Duration", song.Duration));
            songElement.Add(new XAttribute("FilePath", song.FilePath));
            songElement.Add(new XAttribute("ImageUrl", song.ImageUrl));

            playlistElement.Add(songElement);

            doc.Save(Properties.Settings.Default.MusicPath + "/Playlists.xml");
        }

        public void RemoveSong(Music song)
        {
            Songs.Remove(song);

            XDocument doc = XDocument.Load(Properties.Settings.Default.MusicPath + "/Playlists.xml");

            XElement playlistElement = doc.Root.Element(Name);

            playlistElement.Element(song.Title.Replace(" ", "")).RemoveAll();

            doc.Save(Properties.Settings.Default.MusicPath + "/Playlists.xml");
        }

        public void AssignList(List<Music> music)
        {
            Songs = music;
        }
    }
}
