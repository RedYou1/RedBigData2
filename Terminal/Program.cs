namespace Terminal
{
    using RedBigDataNamespace;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal class Program
    {
        public static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            RedBigData redBigData = new(@"..\..\..\Databases");

            Console.WriteLine("Welcome to RedBigData");

            {
                IEnumerable<string> db = redBigData.DatabasesName;
                if (db.Count() == 1)
                {
                    redBigData.SetCurrentDatabase(db.First());
                    Console.WriteLine($"Only one database were created so you were automatically connected to {db.First()}");
                }
                else
                {
                    Console.WriteLine("List of databases:");
                    if (!db.Any())
                        Console.WriteLine("No Databases");
                    else
                        foreach (string name in db)
                        {
                            Console.WriteLine($"-{name}");
                        }
                }
            }

            string? command;
            while (!string.IsNullOrWhiteSpace(command = Console.ReadLine()))
            {
                try
                {
                    string[] args = command.Split(" ");
                    switch (args[0])
                    {
                        case "show":
                            switch (args[1])
                            {
                                case "database":
                                    Console.WriteLine(redBigData.CurrentDatabase);
                                    break;
                                case "databases":
                                    IEnumerable<string> databases = redBigData.DatabasesName;
                                    if (databases.Any())
                                    {
                                        foreach (string database in databases)
                                        {
                                            Console.WriteLine("  -" + Path.GetFileName(database));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("No Databases");
                                    }
                                    break;
                                case "table":
                                    {
                                        if (redBigData.CurrentDatabase is null)
                                        {
                                            Console.WriteLine("No Database Selected");
                                            break;
                                        }
                                        Table table = redBigData.CurrentDatabase.GetTable(args[2]);
                                        Console.WriteLine($"Table: {args[2]} - {table.Rows} rows");
                                        if (table.Columns.Count == 0)
                                        {
                                            Console.WriteLine("No Columns");
                                        }
                                        else
                                        {
                                            foreach (Table.ColumnInfo column in table.Columns)
                                            {
                                                Console.WriteLine($"  -{column.name} - {column.type}");
                                            }
                                        }
                                    }
                                    break;
                                case "tables":
                                    if (redBigData.CurrentDatabase is null)
                                    {
                                        Console.WriteLine("No Database Selected");
                                        break;
                                    }
                                    IEnumerable<string> tables = redBigData.CurrentDatabase.TablesName;
                                    if (tables.Any())
                                    {
                                        foreach (string table in tables)
                                        {
                                            Console.WriteLine("  -" + Path.GetFileName(table));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("No Tables");
                                    }
                                    break;
                            }
                            break;
                        case "use":
                            redBigData.SetCurrentDatabase(args[1]);
                            break;
                        case "create":
                            switch (args[1])
                            {
                                case "database":
                                    redBigData.CreateDatabase(args[2]);
                                    break;
                                case "table":
                                    if (redBigData.CurrentDatabase is null)
                                    {
                                        Console.WriteLine("No Database Selected");
                                        break;
                                    }
                                    redBigData.CurrentDatabase.CreateTable(args[2]);
                                    break;
                            }
                            break;
                        case "add":
                            {
                                if (redBigData.CurrentDatabase is null)
                                {
                                    Console.WriteLine("No Database Selected");
                                    break;
                                }

                                Table table = redBigData.CurrentDatabase.GetTable(args[1]);

                                object[] data = args.Skip(2).ToArray<object>();
                                for (int i = 0; i < table.Columns.Count; i++)
                                {
                                    switch (table.Columns[i].type)
                                    {
                                        case TypeColumn.String:
                                            break;
                                        case TypeColumn.Byte:
                                            {
                                                byte temp;
                                                if (byte.TryParse((string)data[i], out temp))
                                                {
                                                    data[i] = temp;
                                                }
                                                else
                                                {
                                                    throw new Exception($"data[{i}] is not correctly formated\n{data[i]} is not a {table.Columns[i].type}");
                                                }
                                            }
                                            break;
                                        case TypeColumn.Short:
                                            {
                                                short temp;
                                                if (short.TryParse((string)data[i], out temp))
                                                {
                                                    data[i] = temp;
                                                }
                                                else
                                                {
                                                    throw new Exception($"data[{i}] is not correctly formated\n{data[i]} is not a {table.Columns[i].type}");
                                                }
                                            }
                                            break;
                                        case TypeColumn.Int:
                                            {
                                                int temp;
                                                if (int.TryParse((string)data[i], out temp))
                                                {
                                                    data[i] = temp;
                                                }
                                                else
                                                {
                                                    throw new Exception($"data[{i}] is not correctly formated\n{data[i]} is not a {table.Columns[i].type}");
                                                }
                                            }
                                            break;
                                        case TypeColumn.Long:
                                            {
                                                long temp;
                                                if (long.TryParse((string)data[i], out temp))
                                                {
                                                    data[i] = temp;
                                                }
                                                else
                                                {
                                                    throw new Exception($"data[{i}] is not correctly formated\n{data[i]} is not a {table.Columns[i].type}");
                                                }
                                            }
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }

                                table.AddRow(data);

                                break;
                            }
                        case "select":
                            {
                                if (redBigData.CurrentDatabase is null)
                                {
                                    Console.WriteLine("No Database Selected");
                                    break;
                                }

                                Table table = redBigData.CurrentDatabase.GetTable(args[2]);

                                string[] col;
                                if (args[1] == "*")
                                {
                                    col = table.Columns.Select(c => c.name).ToArray();
                                }
                                else
                                {
                                    col = args[1].Split(',');
                                }

                                IEnumerable<object[]> rows = table.GetRow(0, table.Rows, col);

                                if (!rows.Any())
                                {
                                    Console.WriteLine("no rows");
                                }

                                foreach (object[] row in rows)
                                {
                                    Console.WriteLine(string.Join(',', row.Select(d => d.ToString())));
                                }

                                break;
                            }
                        default:
                            Console.WriteLine("Unknown Command");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message}{(e.StackTrace is not null ? '\n' + e.StackTrace : "")}");
                }
            }
        }
    }
}
