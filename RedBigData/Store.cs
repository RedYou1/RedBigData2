using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RedBigData
{
    internal static class Store
    {
        //https://gist.github.com/13xforever/2835844
        internal static T FromByte<T>(this byte[] data) where T : struct
        {
            var pData = GCHandle.Alloc(data, GCHandleType.Pinned);
            var result = (T)Marshal.PtrToStructure(pData.AddrOfPinnedObject(), typeof(T))!;
            pData.Free();
            return result;
        }

        //https://gist.github.com/13xforever/2835844
        internal static byte[] ToBytes<T>(this T data) where T : struct
        {
            var result = new byte[Marshal.SizeOf(typeof(T))];
            var pResult = GCHandle.Alloc(result, GCHandleType.Pinned);
            Marshal.StructureToPtr(data, pResult.AddrOfPinnedObject(), true);
            pResult.Free();
            return result;
        }

        internal static void SaveArrayString(StreamWriter stream, ref string[] strings)
        {
            stream.WriteLine(strings.Length);
            foreach (string s in strings)
            {
                stream.WriteLine(s.Replace('\n', (char)0x1));
            }
        }

        internal static string[] LoadArrayString(StreamReader stream)
        {
            int length = int.Parse(stream.ReadLine()!);
            string[] strings = new string[length];
            for (int i = 0; i < length; i++)
            {
                strings[i] = stream.ReadLine()!.Replace((char)0x1, '\n');
            }
            return strings;
        }
    }
}
