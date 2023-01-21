using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenamingMp3Songs
{
    public class Program
    {
        public static void Main()
        {
            //string songPath = @"c:\temp\";
            //string songPath = @"D:\";
            //string songPath = @"D:\Music\80s90s";
            //string songPath = @"C:\Users\phenry\Music\EuroDance\New.202112";
            string songPath = @"C:\Users\phenry\Music\EuroDance";
            ProcessSongPath(songPath);
            Console.ReadLine();
        }

        private static void ProcessSongPath(string songPath)
        {
            if (string.IsNullOrEmpty(songPath) || !Directory.Exists(songPath))
                throw new DirectoryNotFoundException(songPath);

            Console.WriteLine($"Processing path {songPath}");

            List<string> allSongs = Directory.EnumerateFiles(songPath, "*.mp3", SearchOption.AllDirectories).ToList();
            allSongs.AddRange(Directory.EnumerateFiles(songPath, "*.wma", SearchOption.AllDirectories));
            allSongs.Sort();

            //specially names song for special sorting in Windows Media Player
            allSongs.RemoveAll(x => x.EndsWith("0_empty.wma"));


            Console.WriteLine($"Processing {allSongs.Count} songs");

            foreach (var songFilename in allSongs)
            {
                ProcessSongAsync(songFilename);
            }

            Console.WriteLine($"Processed {allSongs.Count} songs");
        }

        private static void ProcessSongAsync(string songFilename)
        {
            Console.WriteLine(songFilename);
            string newSongFilename = string.Empty;

            using (FileStream fs = File.OpenRead(songFilename))
            {
                SimpleFileAbstraction fu = new SimpleFileAbstraction(new SimpleFile(songFilename, fs));

                TagLib.File tagFile = TagLib.File.Create(fu);
                string artist = tagFile.Tag.FirstAlbumArtist;
                string album = tagFile.Tag.Album;
                string title = tagFile.Tag.Title;
                string contributingArtist = tagFile.Tag.FirstPerformer;
                newSongFilename = GetNewSongFilename(songFilename, contributingArtist, title);
            }

            if (songFilename != newSongFilename)
            {
                if (!File.Exists(newSongFilename))
                {
                    File.Move(songFilename, newSongFilename);
                    Console.WriteLine($"\t\t{newSongFilename}");
                }
                else
                {
                    Console.WriteLine($"*************** Skipping {songFilename} since after renaming, there is a name conflict, figure out which song you want first.");
                    Console.ReadLine();
                }
            }
        }

        private static string GetNewSongFilename(string songFilename, string artist, string title)
        {
            string newSongFilename = songFilename;

            FileInfo songFileInfo = new FileInfo(songFilename);
            title = CleanString(title);
            artist = CleanString(artist);

            if (!string.IsNullOrEmpty(title))
            {
                newSongFilename = Path.Combine(songFileInfo.DirectoryName, $"{title} -BY- {artist}{songFileInfo.Extension}");
            }
            else
            {
                newSongFilename = CleanString(songFileInfo.Name.Replace(songFileInfo.Extension, ""));
                newSongFilename = Path.Combine(songFileInfo.DirectoryName, $"{newSongFilename}{songFileInfo.Extension}");
            }

            return newSongFilename;
        }

        private static string CleanString(string stringToClean)
        {
            string cleanedString = string.Empty;

            if (!string.IsNullOrEmpty(stringToClean))
            {
                cleanedString = stringToClean;

                cleanedString = cleanedString.Replace("\\", "").Replace("/", "").Replace("*", "").Replace("?", "_").Replace(":", "_").Replace("\"", "");

                cleanedString = cleanedString.Replace("(Fitness Version)", "");
                cleanedString = cleanedString.Replace("(Radio Mix)", "");
                cleanedString = cleanedString.Replace("(Orginal Edit)", "");
                cleanedString = cleanedString.Replace("[Clip Officiel]", "");
                cleanedString = cleanedString.Replace("(Clip Officiel)", "");
                cleanedString = cleanedString.Replace("(Clip officiel)", "");
                cleanedString = cleanedString.Replace("[Official Clip]", "");
                cleanedString = cleanedString.Replace(" - Clip Officiel", "");
                cleanedString = cleanedString.Replace(" - Official Clip", "");
                cleanedString = cleanedString.Replace("(Official Music Video)", "");
                cleanedString = cleanedString.Replace("(HQ)", "");
                cleanedString = cleanedString.Replace("(Vidéoclip officiel)", "");
                cleanedString = cleanedString.Replace("(EXECUTIVE PRODUCER)", "");
                cleanedString = cleanedString.Replace("(vidéo officiel)", "");
                cleanedString = cleanedString.Replace("(official video)", "");
                cleanedString = cleanedString.Replace("(Audio Officiel)", "");
                cleanedString = cleanedString.Replace("(Official Audio)", "");
                cleanedString = cleanedString.Replace(" (clip 1988)", "");
                cleanedString = cleanedString.Replace("( 1998)", "");
                cleanedString = cleanedString.Replace("( 1998 )", "");
                cleanedString = cleanedString.Replace("(Secret Mix)", "");
                cleanedString = cleanedString.Replace("(Long Versions)", "");
                cleanedString = cleanedString.Replace("[Edit]", "");
                cleanedString = cleanedString.Replace("(Remix)", "");
                cleanedString = cleanedString.Replace("(New Version)", "");
                cleanedString = cleanedString.Replace("[12\" Version]", "");
                cleanedString = cleanedString.Replace(" (12\" Mix)", "");
                cleanedString = cleanedString.Replace("(extended Mix)", "");
                cleanedString = cleanedString.Replace("(Extended Version)", "");
                cleanedString = cleanedString.Replace("(Jack White Mix)", "");
                cleanedString = cleanedString.Replace("(Remastered 2008)", "");
                cleanedString = cleanedString.Replace("(2002 Remaster)", "");
                cleanedString = cleanedString.Replace("( 2002 Remaster)", "");
                cleanedString = cleanedString.Replace("(2003 Remaster)", "");
                cleanedString = cleanedString.Replace("(2005 Remaster)", "");
                cleanedString = cleanedString.Replace("(2011 Remaster)", "");
                cleanedString = cleanedString.Replace("(2012 Remaster)", "");
                cleanedString = cleanedString.Replace("(2018 Remaster)", "");
                cleanedString = cleanedString.Replace("(2015 Remastered)", "");
                cleanedString = cleanedString.Replace("(Remastered)", "");
                cleanedString = cleanedString.Replace("(Single Edit)", "");
                cleanedString = cleanedString.Replace("(Single Version)", "");
                cleanedString = cleanedString.Replace(" - Single Version", "");
                cleanedString = cleanedString.Replace("(Us Remix)", "");
                cleanedString = cleanedString.Replace("(The Twang Mix)", "");
                cleanedString = cleanedString.Replace("(New Voice Mix)", "");
                cleanedString = cleanedString.Replace("[Mono]", "");
                cleanedString = cleanedString.Replace("(Eiffel65 Remix)", "");
                cleanedString = cleanedString.Replace("[second wind]", "");
                //cleanedString = cleanedString.Replace("", "");
                //cleanedString = cleanedString.Replace("", "");
                //cleanedString = cleanedString.Replace("", "");
                //cleanedString = cleanedString.Replace("", "");
                //cleanedString = cleanedString.Replace("", "");

                cleanedString = cleanedString.Trim();
            }

            return cleanedString;
        }
    }
}

