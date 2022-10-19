using Microsoft.VisualBasic;
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

        public RedBigData RedBigData { get; }
        public string Name { get; }
        public string Path => $@"{RedBigData.Path}\{Name}";
        public string InfoPath => @$"{Path}\info";

        private SyncFile<Data> file;
        private Data data { get => file.Data; set => file.Data = value; }

        public Database(RedBigData rbd, string name)
        {
            RedBigData = rbd;
            Name = name;
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            file = new(InfoPath,
                new Data() { tables = new string[0] },
                Save, Load);
        }

        private static void Save(FileStream stream, Data data)
        {
            using (BinaryWriter bw = new BinaryWriter(stream))
            using (StreamWriter sw = new StreamWriter(stream))
            {
                bw.Write(Store.ToBytes(data.tables.Length));
                bw.Flush();
                foreach (string s in data.tables)
                {
                    sw.WriteLine(s.Replace('\n', (char)0x1));
                }
            }
        }

        private static Data Load(FileStream stream)
        {
            using (StreamReader sr = new StreamReader(stream))
            using (BinaryReader br = new BinaryReader(stream))
            {
                int length = Store.FromByte<int>(br.ReadBytes(sizeof(int)));
                string[] strings = new string[length];
                for (int i = 0; i < length; i++)
                {
                    strings[i] = sr.ReadLine()!.Replace((char)0x1, '\n');
                }
                return new Data()
                {
                    tables = strings
                };
            }
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
