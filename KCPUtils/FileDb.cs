using System;
using System.IO;
using System.Text;
using SQLite;

namespace KCPUtils.FileDb
{
    public class FileDb
    {
        public string Name;
        private readonly SQLiteAsyncConnection DbConnection;
        public Env EnvironmentInstance;

        public FileDb(string fileDbPathname, string fullpath)
        {
            this.Name = fileDbPathname;
            this.DbConnection = new SQLiteAsyncConnection(fileDbPathname);
            this.DbConnection.CreateTable<File>();
            this.EnvironmentInstance = new Env(fullpath);
        }

        public static FileDb NewFile(string databasePath)
        {
            var fullpath = Path.Combine(Environment.CurrentDirectory, databasePath);
            return new FileDb(databasePath, fullpath);
        }

        public static FileDb NewFile(string databasePath, string parentDirectory)
        {
            var fullpath = Path.Combine(parentDirectory, databasePath);
            return new FileDb(databasePath, fullpath);
        }

        public class Env
        {
            public string Fullpath;
            public Env(string fullpath)
            {
                this.Fullpath = fullpath;
            }
        }

        public class File
        {
            [PrimaryKey]
            public string Filename { get; set; }
            public FileType FileType { get; set; }
            public Byte[] Content { get; set; }
        }

        public FileDb CollectFile(string filename, FileType type)
        {
            var _file = new File()
            {
                Filename = filename,
                FileType = type,

                // using filename and type to get content
                Content = System.IO.File.ReadAllBytes(filename)
            };

            this.DbConnection.Insert(_file);
            return this;
        }

        public string Pathname => this.EnvironmentInstance.Fullpath;

        public void Print()
        {
            var _tblQuery = this.DbConnection.Table<File>();

            foreach (var entry in _tblQuery.ToList())
            {
                Console.WriteLine(entry.Filename);
                if (entry.FileType != FileType.Text)
                {
                    Console.WriteLine(Encoding.UTF8.GetString(entry.Content));
                    continue;
                }
            }
        }

        public void Build() => this.DbConnection.Close();
    }

    public enum FileType
    {
        Text
    }
}