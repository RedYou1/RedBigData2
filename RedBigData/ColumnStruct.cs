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

        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        private struct Data
        {
            public T[] elements;
        }

        private Data data;

        public ColumnStruct(Table table, string name)
        {
            Table = table;
            Name = name;

            if (!File.Exists(Path))
            {
                File.Create(Path);
                data = new Data() { elements = new T[0] };
            }
            else
            {
                T[] values = new T[Table.Rows];
                byte[] bytes = File.ReadAllBytes(Path);
                int size = Marshal.SizeOf(typeof(T));
                for (int i = 0; i < Table.Rows; i++)
                {
                    values[i] = Store.FromByte<T>(bytes.Take(size).ToArray());
                    bytes = bytes.Skip(size).ToArray();
                }
                data = new Data() { elements = values };
            }
        }

        internal ColumnStruct(Table table, string name, T[] data)
        {
            Table = table;
            Name = name;
            this.data = new Data() { elements = data };
        }

        public override void Add(params T[] element)
        {
            data = new Data()
            {
                elements =
                    data.elements
                        .Concat(element).ToArray()
            };
            using (StreamWriter sw = new StreamWriter(Path))
            {
                BinaryWriter bw = new BinaryWriter(sw.BaseStream);
                bw.Seek(0, SeekOrigin.End);
                bw.Write(element.SelectMany(e => Store.ToBytes(e)).ToArray());
            }
        }

        public override void Insert(int index, params T[] element)
        {
            byte[] temp;
            using (StreamReader sr = new StreamReader(Path))
            {
                BinaryReader br = new BinaryReader(sr.BaseStream);
                br.ReadBytes(Marshal.SizeOf(typeof(T)) * index);
                temp = br.ReadBytes(Marshal.SizeOf(typeof(T)) * (data.elements.Length - index));
            }

            data = new Data()
            {
                elements =
                    data.elements.Take(index)
                        .Concat(element)
                        .Concat(data.elements.Skip(index)).ToArray()
            };

            using (StreamWriter sw = new StreamWriter(Path, true))
            {
                BinaryWriter bw = new BinaryWriter(sw.BaseStream);
                bw.Seek(Marshal.SizeOf(typeof(T)) * index, SeekOrigin.Begin);
                bw.Write(element.SelectMany(e => Store.ToBytes(e)).ToArray());
                bw.Write(temp);
            }
        }

        public override void Remove(int index, int count)
        {
            byte[] temp;
            using (StreamReader sr = new StreamReader(Path))
            {
                BinaryReader br = new BinaryReader(sr.BaseStream);
                br.ReadBytes(Marshal.SizeOf(typeof(T)) * (index + count));
                temp = br.ReadBytes(Marshal.SizeOf(typeof(T)) * (data.elements.Length - index - count));
            }

            data = new Data()
            {
                elements =
                    data.elements.Take(index)
                        .Concat(data.elements.Skip(index + count)).ToArray()
            };

            using (StreamWriter sw = new StreamWriter(Path, true))
            {
                BinaryWriter bw = new BinaryWriter(sw.BaseStream);
                bw.Seek(Marshal.SizeOf(typeof(T)) * index, SeekOrigin.Begin);
                bw.Write(temp);
            }
        }

        public override ReadOnlyCollection<T> TypedElements
            => Array.AsReadOnly(data.elements);
    }
}
