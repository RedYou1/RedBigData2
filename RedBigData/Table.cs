using Microsoft.VisualBasic;
using RedBigData;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RedBigDataNamespace
{
    public class Table
    {
        public Database Database { get; }
        public string Name { get; }
        public string Path => $@"{Database.Path}\{Name}";
        public string InfoPath => @$"{Path}\info";

        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        private struct Data
        {
            public int rows;
            public ColumnInfo[] columns;
        }

        public struct ColumnInfo
        {
            public string name { get; init; }
            public TypeColumn type { get; init; }
        }

        private SyncFile<Data> file;
        private Data data { get => file.Data; set => file.Data = value; }

        internal Table(Database db, string name)
        {
            Database = db;
            Name = name;

            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            file = new(InfoPath,
                new Data()
                {
                    rows = 0,
                    columns = new ColumnInfo[0]
                }, Save, Load);

            columns = new Column[data.columns.Length];
            for (int i = 0; i < data.columns.Length; i++)
            {
                columns[i] = NewColumn(data.columns[i].name, data.columns[i].type.ID);
            }
        }

        public void AddColumn(string name, TypeColumn type)
        {
            if (data.rows > 0)
            {
                throw new NotImplementedException();
            }
            if (data.columns.Any(c => c.name == name))
            {
                throw new Exception("already contains this name");
            }
            data = new Data()
            {
                rows = data.rows,
                columns = data.columns.Append(new ColumnInfo { name = name, type = type }).ToArray()
            };
            columns = columns.Append(NewColumn(name, type.ID)).ToArray();
        }

        public void AddRow(params object[] row)
        {
            if (data.columns.Length != row.Length)
            {
                throw new Exception("not right length");
            }
            for (int i = 0; i < data.columns.Length; i++)
            {
                columns[i].Add(row[i]);
            }
            data = new Data()
            {
                rows = data.rows + 1,
                columns = data.columns
            };
        }

        public void InsertRow(int index, params object[] row)
        {
            if (data.columns.Length != row.Length)
            {
                throw new Exception("not right length");
            }
            for (int i = 0; i < data.columns.Length; i++)
            {
                columns[i].Insert(index, row[i]);
            }
            data = new Data()
            {
                rows = data.rows + 1,
                columns = data.columns
            };
        }

        public void RemoveRow(int index, int count)
        {
            for (int i = 0; i < data.columns.Length; i++)
            {
                columns[i].Remove(index, count);
            }
            data = new Data()
            {
                rows = data.rows - count,
                columns = data.columns
            };
        }

        public IEnumerable<object[]> GetRow(int from, int count, params string[] columns)
        {
            if (from + count > Rows)
                throw new IndexOutOfRangeException();

            ReadOnlyCollection<object>[] data = new ReadOnlyCollection<object>[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                IEnumerable<Column> col = this.columns.Where(c => c.Name == columns[i]);
                if (col.Count() != 1)
                    throw new Exception($"column {columns[i]} dont exists or have multiple");
                data[i] = col.First().Elements;
            }

            for (int i = from; i < from + count; i++)
            {
                object[] o = new object[columns.Length];
                for (int c = 0; c < columns.Length; c++)
                {
                    o[c] = data[c][i];
                }
                yield return o;
            }
        }

        private static void Save(FileStream stream, Data data)
        {
            using (BinaryWriter bw = new BinaryWriter(stream))
            using (StreamWriter sw = new StreamWriter(stream))
            {
                bw.Write(Store.ToBytes<int>(data.rows));
                bw.Write(Store.ToBytes(data.columns.Length));
                bw.Write(data.columns.Select(t => t.type.IDByte).ToArray());
                bw.Flush();
                foreach (string s in data.columns.Select(c => c.name))
                {
                    sw.WriteLine(s.Replace('\n', (char)0x1));
                }
            }
        }

        private static Data Load(FileStream stream)
        {
            using (BinaryReader br = new BinaryReader(stream))
            using (StreamReader sr = new StreamReader(stream))
            {
                int rows = Store.FromByte<int>(br.ReadBytes(sizeof(int)));

                int length = Store.FromByte<int>(br.ReadBytes(sizeof(int)));
                TypeColumn[] typeColumns =
                    br.ReadBytes(length)
                        .Select(t => TypeColumn.FromID(t)).ToArray();

                string[] columns = new string[length];
                for (int i = 0; i < length; i++)
                {
                    columns[i] = sr.ReadLine()!.Replace((char)0x1, '\n');
                }

                return new Data()
                {
                    rows = rows,
                    columns = Enumerable.Range(0, length)
                        .Select(i => new ColumnInfo { name = columns[i], type = typeColumns[i] }).ToArray()
                };
            }
        }

        public int Rows => data.rows;

        private Column[] columns;
        public ReadOnlyCollection<ColumnInfo> Columns => Array.AsReadOnly(data.columns);

        private Column NewColumn(string name, TypeColumnID typeColumm)
        {
            switch (typeColumm)
            {
                case TypeColumnID.String:
                    return new ColumnDym<string>(this, name, (stream, e) => stream.Write(e), stream => stream.ReadToEnd());
                case TypeColumnID.Byte:
                    return new ColumnStruct<byte>(this, name);
                case TypeColumnID.Short:
                    return new ColumnStruct<short>(this, name);
                case TypeColumnID.Int:
                    return new ColumnStruct<int>(this, name);
                case TypeColumnID.Long:
                    return new ColumnStruct<long>(this, name);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
