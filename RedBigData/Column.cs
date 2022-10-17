using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedBigDataNamespace
{
    public interface Column
    {
        public string Name { get; }

        public void Add(params object[] element);

        public void Insert(int index, params object[] element);

        public void Remove(int index, int count);

        public ReadOnlyCollection<object> Elements { get; }
    }

    public abstract class Column<T> : Column
    {

        public abstract void Add(params T[] element);

        public abstract void Insert(int index, params T[] element);

        public abstract void Remove(int index, int count);

        public abstract ReadOnlyCollection<T> TypedElements { get; }

        public abstract string Name { get; }



        public void Add(params object[] element)
        {
            Add(element.Cast<T>().ToArray());
        }

        public void Insert(int index, params object[] element)
        {
            Insert(index, element.Cast<T>().ToArray());
        }

        public ReadOnlyCollection<object> Elements => Array.AsReadOnly(TypedElements.Cast<object>().ToArray());
    }
}
