using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedBigData
{
    public enum TypeColumnID : byte
    {
        String,
        Byte,
        Short,
        Int,
        Long
    }

    public class TypeColumn
    {
        public static TypeColumn String => new(TypeColumnID.String, "String", (s) => s);
        public static TypeColumn Byte => new(TypeColumnID.Byte, "Byte",
            (s) =>
            {
                byte r;
                bool r2 = byte.TryParse(s, out r);
                return r2 ? r : null;
            });
        public static TypeColumn Short => new(TypeColumnID.Short, "Short",
            (s) =>
            {
                short r;
                bool r2 = short.TryParse(s, out r);
                return r2 ? r : null;
            });
        public static TypeColumn Int => new(TypeColumnID.Int, "Int",
            (s) =>
            {
                int r;
                bool r2 = int.TryParse(s, out r);
                return r2 ? r : null;
            });
        public static TypeColumn Long => new(TypeColumnID.Long, "Long",
            (s) =>
            {
                long r;
                bool r2 = long.TryParse(s, out r);
                return r2 ? r : null;
            });

        public TypeColumnID ID { get; }
        public byte IDByte => (byte)ID;

        public string Name { get; }
        public Func<string, object?> FromString { get; }

        private TypeColumn(TypeColumnID id, string name, Func<string, object?> fromString)
        {
            ID = id;
            Name = name;
            FromString = fromString;
        }

        public static TypeColumn FromID(byte id)
        {
            switch (id)
            {
                case 0:
                    return String;
                case 1:
                    return Byte;
                case 2:
                    return Short;
                case 3:
                    return Int;
                case 4:
                    return Long;
                default:
                    throw new Exception($"Not valide typeColumn id {id}");
            }
        }

        public override string ToString() => Name;

        public override bool Equals(object? obj)
            => obj is not null && obj is TypeColumn col && col.ID == ID;

        public override int GetHashCode()
            => IDByte.GetHashCode();
    }
}
