using RedBigData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RedBigDataNamespace
{
    public class RedBigData
    {
        public static Version CurrentVersion = new(1, 0, 0);

        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        private struct Data
        {
            public Version version;
            public string[] databases;
        }

        public string Path { get; }
        public string InfoPath => @$"{Path}\info.txt";

        private SyncFile<Data> file;
        private Data data { get => file.Data; set => file.Data = value; }

        public Version Version => data.version;

        public RedBigData(string path)
        {
            Path = path;
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            file = new(InfoPath,
                new Data()
                {
                    version = CurrentVersion,
                    databases = new string[0]
                }, Save, Load);
        }

        private static void Save(StreamWriter stream, Data data)
        {
            stream.WriteLine(data.version);
            Store.SaveArrayString(stream, ref data.databases);
        }

        private static Data Load(StreamReader stream)
        {
            return new Data()
            {
                version = Version.Parse(stream.ReadLine()!),
                databases = Store.LoadArrayString(stream)
            };
        }

        public Database? CurrentDatabase { get; private set; }
        public void SetCurrentDatabase(string? name)
        {
            if (name is null)
            {
                CurrentDatabase = null;
            }
            else if (data.databases.Contains(name))
            {
                CurrentDatabase = new Database(this, name);
            }
            else
            {
                throw new Exception("this Database doesn't exists");
            }
        }

        public ReadOnlyCollection<string> DatabasesName
            => Array.AsReadOnly(data.databases);

        public IEnumerable<Database> Databases()
        {
            foreach (string name in data.databases)
            {
                yield return new Database(this, name);
            }
        }

        public Database CreateDatabase(string name)
        {
            data = new Data()
            {
                version = data.version,
                databases = data.databases.Append(name).ToArray()
            };
            return new Database(this, name);
        }
    }
}
