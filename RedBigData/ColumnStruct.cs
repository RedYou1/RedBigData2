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
    public class ColumnStruct<T> : Column<T> where T : struct
    {
        public Table Table;
        private string name;
        public override string Name => name;
        public string Path { get; }

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
            Path = $@"{Table.Path}\{name}.txt";
            this.name = name;
            data = new Data() { elements = new T[0] };
            Save();
        }

        internal ColumnStruct(Table table, string name, T[] data)
        {
            Table = table;
            Path = $@"{Table.Path}\{name}.txt";
            this.name = name;
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
            Save();
        }

        public override void Insert(int index, params T[] element)
        {
            data = new Data()
            {
                elements =
                    data.elements.Take(index)
                        .Concat(element)
                        .Concat(data.elements.Skip(0)).ToArray()
            };
            Save();
        }

        public override void RemoveFirst(int count)
        {
            data = new Data()
            {
                elements =
                    data.elements.Skip(count).ToArray()
            };
            Save();
        }

        public override void RemoveLast(int count)
        {
            data = new Data()
            {
                elements =
                    data.elements.SkipLast(count).ToArray()
            };
            Save();
        }

        public override void Remove(int index, int count)
        {
            data = new Data()
            {
                elements =
                    data.elements.Take(index)
                        .Concat(data.elements.Skip(index + count)).ToArray()
            };
            Save();
        }

        private void Save()
        {
            File.WriteAllBytes(Path, Store.ToBytes(data));
        }

        public override ReadOnlyCollection<T> Elements
            => Array.AsReadOnly(data.elements);
    }
}
