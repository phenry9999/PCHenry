using System.IO;
using TagLib;

namespace RenamingMp3Songs
{
    public class SimpleFileAbstraction : TagLib.File.IFileAbstraction
    {
        readonly private SimpleFile file;

        public SimpleFileAbstraction(SimpleFile file)
        {
            this.file = file;
        }

        public string Name
        {
            get { return file.Name; }
        }

        public System.IO.Stream ReadStream
        {
            get { return file.Stream; }
        }

        public System.IO.Stream WriteStream
        {
            get { return file.Stream; }
        }

        public void CloseStream(System.IO.Stream stream)
        {
            stream.Position = 0;
        }
    }
    public class SimpleFile
    {
        public SimpleFile(string Name, Stream Stream)
        {
            this.Name = Name;
            this.Stream = Stream;
        }
        public string Name { get; set; }
        public Stream Stream { get; set; }
    }
}
