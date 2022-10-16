using RedBigData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RedBigDataNamespace
{
    public class Table
    {
        public Database Database { get; }
        private string name;
        public string Name => name;
        public string Path { get; }
        public string InfoPath => @$"{Path}\info.txt";
        private Column[] columns => data.columns
               .Select(c => Column.FromPath($@"{Path}\{c}")).ToArray();

        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        private struct Data
        {
            public int rows;
            public string[] columns;
        }

        private SyncFile<Data> file;
        private Data data { get => file.Data; set => file.Data = value; }

        internal Table(Database db, string name)
        {
            Database = db;
            Path = $@"{Database.Path}\{name}";
            this.name = name;

            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            file = new(InfoPath,
                new Data()
                {
                    rows = 0,
                    columns = new string[0]
                }, Save, Load);
        }

        private static void Save(StreamWriter stream, Data data)
        {
            new BinaryWriter(stream.BaseStream).Write(Store.ToBytes<int>(data.rows));
            Store.SaveArrayString(stream, ref data.columns);
        }

        private static Data Load(StreamReader stream)
        {
            return new Data()
            {
                rows = Store.FromByte<int>(new BinaryReader(stream.BaseStream).ReadBytes(sizeof(int))),
                columns = Store.LoadArrayString(stream)
            };
        }

        public int Rows => data.rows;
        public ReadOnlyCollection<Column> Columns => Array.AsReadOnly(columns);
    }
}
