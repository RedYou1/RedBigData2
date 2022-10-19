using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedBigData
{
    internal class SyncFile<T>
    {
        public string Path { get; }

        private T data;
        public T Data
        {
            get => data;
            set
            {
                data = value;
                using (FileStream sw = new FileStream(Path, FileMode.Truncate, FileAccess.Write, FileShare.None))
                {
                    Save.Invoke(sw, data);
                }
            }
        }

        public Action<FileStream, T> Save { get; }
        public Func<FileStream, T> Load { get; }

        public SyncFile(string path, T defaultData, Action<FileStream, T> save, Func<FileStream, T> load)
        {
            Path = path;
            Save = save;
            Load = load;
            if (File.Exists(path))
            {
                using (FileStream sw = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    data = Load.Invoke(sw);
                }
            }
            else
            {
                data = defaultData;
                using (FileStream sw = new FileStream(Path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    Save.Invoke(sw, data);
                }
            }
        }
    }
}
