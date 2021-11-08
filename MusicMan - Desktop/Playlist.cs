using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MusicMan___Desktop
{
    public class Playlist
    {
        public string Name { get; set; }
        //Add handled adding / removing (adding and removing from xml not only from list)
        public List<Music> Songs { get; private set; }





        public void AssignList(List<Music> music)
        {
            Songs = music;
        }
    }
}
