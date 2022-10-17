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

            string? command;
            while (!string.IsNullOrWhiteSpace(command = Console.ReadLine()))
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
                                    if (table.ColumnsName.Count == 0)
                                    {
                                        Console.WriteLine("No Columns");
                                    }
                                    else
                                    {
                                        foreach (string column in table.ColumnsName)
                                        {
                                            Console.WriteLine($"  -{column}");
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
                    default:
                        Console.WriteLine("Unknown Command");
                        break;
                }
            }
        }
    }
}
