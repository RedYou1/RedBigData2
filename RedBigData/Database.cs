using RedBigData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RedBigDataNamespace
{
    public class Database
    {
        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        private struct Data
        {
            public string[] tables;
        }

        public string Path { get; }
        public string InfoPath => @$"{Path}\info.txt";

        private SyncFile<Data> file;
        private Data data { get => file.Data; set => file.Data = value; }

        public Database(RedBigData rbd, string name)
        {
            Path = $@"{rbd.Path}\{name}";
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            file = new(InfoPath,
                new Data() { tables = new string[0] },
                Save, Load);
        }

        private static void Save(StreamWriter stream, Data data)
        {
            Store.SaveArrayString(stream, ref data.tables);
        }

        private static Data Load(StreamReader stream)
        {
            return new Data()
            {
                tables = Store.LoadArrayString(stream)
            };
        }

        public Table CreateTable(string name)
        {
            data = new Data()
            {
                tables = data.tables.Append(name).ToArray()
            };
            return new Table(this, name);
        }

        public ReadOnlyCollection<string> TablesName
            => Array.AsReadOnly(data.tables);

        public IEnumerable<Table> Tables()
        {
            foreach (string name in data.tables)
            {
                yield return new Table(this, name);
            }
        }

        public Table GetTable(string name)
        {
            if (data.tables.Contains(name))
            {
                return new Table(this, name);
            }
            throw new Exception();
        }
    }
}
