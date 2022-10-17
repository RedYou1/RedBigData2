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
    public enum TypeColumn : byte
    {
        String = 0,
        Byte = 1,
        Short = 2,
        Int = 3,
        Long = 4,
    }

    public class Table
    {
        public Database Database { get; }
        public string Name { get; }
        public string Path => $@"{Database.Path}\{Name}";
        public string InfoPath => @$"{Path}\info";
        private Column[] columns => data.columns
               .Select((c, i) =>
                   NewColumn(c, data.typeColumms[i])).ToArray();

        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        private struct Data
        {
            public int rows;
            public string[] columns;
            public TypeColumn[] typeColumms;
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
                    columns = new string[0],
                    typeColumms = new TypeColumn[0]
                }, Save, Load);
        }

        public void AddColumn(string name, TypeColumn type)
        {
            if (data.rows > 0)
            {
                throw new NotImplementedException();
            }
            if (data.columns.Contains(name))
            {
                throw new Exception("already contains this name");
            }
            data = new Data()
            {
                rows = data.rows,
                columns = data.columns.Append(name).ToArray(),
                typeColumms = data.typeColumms.Append(type).ToArray()
            };
            NewColumn(name, type);
        }

        public void AddRow(object[] row)
        {
            if (data.columns.Length != row.Length)
            {
                throw new Exception("not right length");
            }
            for (int i = 0; i < data.columns.Length; i++)
            {
                NewColumn(data.columns[i], data.typeColumms[i]).Add(row[i]);
            }
            data = new Data()
            {
                rows = data.rows + 1,
                columns = data.columns,
                typeColumms = data.typeColumms
            };
        }

        public void InsertRow(int index, object[] row)
        {
            if (data.columns.Length != row.Length)
            {
                throw new Exception("not right length");
            }
            for (int i = 0; i < data.columns.Length; i++)
            {
                NewColumn(data.columns[i], data.typeColumms[i]).Insert(index, row[i]);
            }
            data = new Data()
            {
                rows = data.rows + 1,
                columns = data.columns,
                typeColumms = data.typeColumms
            };
        }

        public void RemoveRow(int index, int count)
        {
            for (int i = 0; i < data.columns.Length; i++)
            {
                NewColumn(data.columns[i], data.typeColumms[i]).Remove(index, count);
            }
            data = new Data()
            {
                rows = data.rows - count,
                columns = data.columns,
                typeColumms = data.typeColumms
            };
        }

        private static void Save(StreamWriter stream, Data data)
        {
            new BinaryWriter(stream.BaseStream).Write(Store.ToBytes<int>(data.rows));
            Store.SaveArrayString(stream, ref data.columns);
            new BinaryWriter(stream.BaseStream)
                .Write(data.typeColumms.Select(t => (byte)t).ToArray());
        }

        private static Data Load(StreamReader stream)
        {
            int rows = Store.FromByte<int>(new BinaryReader(stream.BaseStream)
                .ReadBytes(sizeof(int)));
            return new Data()
            {
                rows = rows,
                columns = Store.LoadArrayString(stream),
                typeColumms = new BinaryReader(stream.BaseStream)
                    .ReadBytes(rows * sizeof(byte))
                    .Select(t => (TypeColumn)t).ToArray()
            };
        }

        public int Rows => data.rows;
        public ReadOnlyCollection<string> ColumnsName => Array.AsReadOnly(data.columns);
        public ReadOnlyCollection<TypeColumn> ColumnsType => Array.AsReadOnly(data.typeColumms);

        private Column NewColumn(string name, TypeColumn typeColumm)
        {
            switch (typeColumm)
            {
                case TypeColumn.String:
                    return new ColumnDym<string>(this, name, (stream, e) => stream.Write(e), stream => stream.ReadToEnd());
                case TypeColumn.Byte:
                    return new ColumnStruct<byte>(this, name);
                case TypeColumn.Short:
                    return new ColumnStruct<short>(this, name);
                case TypeColumn.Int:
                    return new ColumnStruct<int>(this, name);
                case TypeColumn.Long:
                    return new ColumnStruct<long>(this, name);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
