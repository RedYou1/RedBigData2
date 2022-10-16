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
                using (StreamWriter sw = new StreamWriter(Path))
                {
                    Save.Invoke(sw, data);
                }
            }
        }

        public Action<StreamWriter, T> Save { get; }
        public Func<StreamReader, T> Load { get; }

        public SyncFile(string path, T defaultData, Action<StreamWriter, T> save, Func<StreamReader, T> load)
        {
            Path = path;
            Save = save;
            Load = load;
            if (File.Exists(path))
            {
                using (StreamReader sw = new StreamReader(Path, Encoding.UTF8))
                {
                    data = Load.Invoke(sw);
                }
            }
            else
            {
                data = defaultData;
                using (StreamWriter sw = new StreamWriter(Path))
                {
                    Save.Invoke(sw, data);
                }
            }
        }
    }
}
