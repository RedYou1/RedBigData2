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
    public class ColumnStruct<T> : Column<T> where T : struct
    {
        public Table Table;
        public override string Name { get; }
        public string Path => $@"{Table.Path}\{Name}";

        private T[] _elements;

        public ColumnStruct(Table table, string name)
        {
            Table = table;
            Name = name;

            if (!File.Exists(Path))
            {
                File.Create(Path).Close();
                _elements = new T[0];
            }
            else
            {
                _elements = new T[Table.Rows];
                if (Table.Rows > 0)
                {
                    byte[] bytes = File.ReadAllBytes(Path);
                    int size = Marshal.SizeOf(typeof(T));
                    for (int i = 0; i < Table.Rows; i++)
                    {
                        _elements[i] = Store.FromByte<T>(bytes.Take(size).ToArray());
                        bytes = bytes.Skip(size).ToArray();
                    }
                }
            }
        }

        public override void Add(params T[] element)
        {
            _elements = _elements.Concat(element).ToArray();
            using (FileStream sw = new FileStream(Path, FileMode.Append, FileAccess.Write, FileShare.None))
            {
                sw.Write(element.SelectMany(e => Store.ToBytes(e)).ToArray());
            }
        }

        public override void Insert(int index, params T[] element)
        {
            byte[] temp;
            using (FileStream sr = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.None))
            using (BinaryReader br = new BinaryReader(sr))
            {
                sr.Seek(Marshal.SizeOf(typeof(T)) * index, SeekOrigin.Begin);
                temp = br.ReadBytes(Marshal.SizeOf(typeof(T)) * (_elements.Length - index));
            }

            _elements = _elements.Take(index)
                        .Concat(element)
                        .Concat(_elements.Skip(index)).ToArray();

            using (FileStream sw = new FileStream(Path, FileMode.Open, FileAccess.Write, FileShare.None))
            using (BinaryWriter bw = new BinaryWriter(sw))
            {
                bw.Seek(Marshal.SizeOf(typeof(T)) * index, SeekOrigin.Begin);
                bw.Write(element.SelectMany(e => Store.ToBytes(e)).ToArray());
                bw.Write(temp);
            }
        }

        public override void Remove(int index, int count)
        {
            byte[] temp;
            using (FileStream sr = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.None))
            using (BinaryReader br = new BinaryReader(sr))
            {
                br.ReadBytes(Marshal.SizeOf(typeof(T)) * (index + count));
                temp = br.ReadBytes(Marshal.SizeOf(typeof(T)) * (_elements.Length - index - count));
            }

            _elements = _elements.Take(index)
                        .Concat(_elements.Skip(index + count)).ToArray();

            using (FileStream sw = new FileStream(Path, FileMode.Open, FileAccess.Write, FileShare.None))
            using (BinaryWriter bw = new BinaryWriter(sw))
            {
                bw.Seek(Marshal.SizeOf(typeof(T)) * index, SeekOrigin.Begin);
                bw.Write(temp);
                sw.SetLength(sw.Position);
            }
        }

        public override ReadOnlyCollection<T> TypedElements
            => Array.AsReadOnly(_elements);
    }
}
