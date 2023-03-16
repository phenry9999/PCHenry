using System;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace CsMvvmToolkit_CP
{
    static class Mod_Public
    {
        public static string sAppPath { get; set; }

        public static void ErrHandler(string sErr)
        {
            File.AppendAllText(sAppPath + @"\Log.txt", string.Format("{0}{1}", Environment.NewLine, DateAndTime.Now.ToString() + "; " + sErr));
            var msgBoxService = Ioc.Default.GetService<IMsgBoxService>();
            msgBoxService.Show("Unexpected error:" + Constants.vbNewLine + Constants.vbNewLine + sErr, img: MessageBoxImage.Error);
        }

        public static string ReadTextLines(string FileName)
        {
            string ReadTextLinesRet = default;
            var fi = new FileInfo(FileName);
            ReadTextLinesRet = Constants.vbNullString;
            try
            {
                if (fi.Length > 0L)
                {
                    // Open the file using a stream reader.
                    using (var sr = new StreamReader(FileName))
                    {
                        string line;

                        // Read the stream to a string and write the string to the console.
                        line = sr.ReadToEnd();
                        line = Strings.Replace(line, " " + Constants.vbNewLine, Constants.vbNewLine);
                        line = Strings.Replace(line, " " + '\t', "");
                        line = Strings.Replace(line, Conversions.ToString('\n'), "");
                        line = Strings.Replace(line, Conversions.ToString('\r'), Constants.vbNewLine);
                        line = Strings.Replace(line, Constants.vbNewLine + Constants.vbNewLine + Constants.vbNewLine, Constants.vbNewLine);
                        line = Strings.Trim(line);

                        // http://forums.devx.com/showthread.php?146639-VB-NET-remove-extra-blank-lines-from-file
                        line = line.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

                        return line;
                    }
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                ErrHandler(ex.ToString());
            }

            return ReadTextLinesRet;
        }
    }
}