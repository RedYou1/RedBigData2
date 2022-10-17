using RedBigDataNamespace;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RedBigData
{
    public class ColumnDym<T> : Column<T>
    {
        public override string Name { get; }

        public string Path => @$"{Path}\{Name}";

        private Table Table { get; }

        private T[] elements;

        public Action<StreamWriter, T> Save { get; }
        public Func<StreamReader, T> Load { get; }

        public ColumnDym(Table table, string name, Action<StreamWriter, T> save, Func<StreamReader, T> load)
        {
            Table = table;
            Name = name;
            Save = save;
            Load = load;
            elements = new T[Table.Rows];
            for (int i = 0; i < Table.Rows; i++)
            {
                using (StreamReader sr = new StreamReader($@"{Path}\{i}"))
                {
                    elements[i] = Load.Invoke(sr);
                }
            }
        }

        public override void Add(params T[] elements)
        {
            int i = elements.Length;
            foreach (T element in elements)
            {
                using (StreamWriter sw = new StreamWriter($@"{Path}\{i}"))
                {
                    Save.Invoke(sw, element);
                }
                i++;
            }
            this.elements = this.elements
                        .Concat(elements).ToArray();
        }

        public override void Insert(int index, params T[] elements)
        {
            for (int i = this.elements.Length; i >= index; i--)
            {
                using (StreamWriter sw = new StreamWriter($@"{Path}\{i + elements.Length}"))
                {
                    Save.Invoke(sw, this.elements[i]);
                }
            }
            int i2 = index;
            foreach (T element in elements)
            {
                using (StreamWriter sw = new StreamWriter($@"{Path}\{i2}"))
                {
                    Save.Invoke(sw, element);
                }
                i2++;
            }
            this.elements = this.elements.Take(index)
                        .Concat(elements)
                        .Concat(this.elements.Skip(index)).ToArray();
        }

        public override void Remove(int index, int count)
        {
            for (int i = index; i <= index + count; i++)
            {
                using (StreamWriter sw = new StreamWriter($@"{Path}\{i + count}"))
                {
                    Save.Invoke(sw, elements[i + count]);
                }
            }
            elements = elements.Take(index)
                        .Concat(elements.Skip(index + count)).ToArray();
        }

        public override ReadOnlyCollection<T> TypedElements => Array.AsReadOnly(elements);
    }
}
