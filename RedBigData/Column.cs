using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedBigDataNamespace
{
    public abstract class Column
    {
        public abstract string Name { get; }

        internal static Column FromPath(string path)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Column<T> : Column
    {
        public abstract void Add(params T[] element);

        public abstract void Insert(int index, params T[] element);

        public abstract void RemoveFirst(int count);

        public abstract void RemoveLast(int count);

        public abstract void Remove(int index, int count);

        public abstract ReadOnlyCollection<T> Elements { get; }
    }
}
