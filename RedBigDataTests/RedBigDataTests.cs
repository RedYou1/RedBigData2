namespace RedBigDataTests
{
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Newtonsoft.Json.Linq;
    using RedBigDataNamespace;

    [TestClass]
    public class RedBigDataTests
    {
        public const string TestPath = @"..\..\..\Databases";

        [TestMethod]
        public void _1_Creation()
        {
            if (Directory.Exists(TestPath))
                Directory.Delete(TestPath, true);

            Assert.IsFalse(Directory.Exists(TestPath));
            RedBigData redBigData = new(TestPath);
            Assert.IsTrue(Directory.Exists(TestPath));
            Assert.AreEqual(redBigData.Path, TestPath);
            Assert.AreEqual(redBigData.Version, RedBigData.CurrentVersion);
            Assert.AreEqual(redBigData.DatabasesName.Count, 0);

            Assert.IsNull(redBigData.CurrentDatabase);
            Assert.IsFalse(Directory.Exists($@"{TestPath}\bigDB"));
            redBigData.CreateDatabase("bigDB");
            Assert.IsTrue(Directory.Exists($@"{TestPath}\bigDB"));
            Assert.AreEqual(redBigData.DatabasesName.Count, 1);

            redBigData.SetCurrentDatabase("bigDB");
            Assert.IsNotNull(redBigData.CurrentDatabase);

            Assert.AreEqual(redBigData.CurrentDatabase.Path, $@"{TestPath}\bigDB");
            Assert.AreEqual(redBigData.CurrentDatabase.TablesName.Count, 0);

            Assert.ThrowsException<Exception>(() => redBigData.CurrentDatabase.GetTable("bigTable"));

            Assert.IsFalse(Directory.Exists($@"{redBigData.CurrentDatabase.Path}\bigTable"));
            redBigData.CurrentDatabase.CreateTable("bigTable");
            Assert.IsTrue(Directory.Exists($@"{redBigData.CurrentDatabase.Path}\bigTable"));

            Table table = redBigData.CurrentDatabase.GetTable("bigTable");
            Assert.AreEqual(table.Path, $@"{redBigData.CurrentDatabase.Path}\bigTable");
            Assert.AreEqual(table.Columns.Count, 0);
            Assert.AreEqual(table.Rows, 0);
        }

        [TestMethod]
        public void _2_Column()
        {
            RedBigData redBigData = new(TestPath);
            Assert.AreEqual(redBigData.DatabasesName.Count, 1);
            Assert.AreEqual(redBigData.DatabasesName[0], "bigDB");
            Assert.IsNull(redBigData.CurrentDatabase);
            redBigData.SetCurrentDatabase("bigDB");
            Assert.IsNotNull(redBigData.CurrentDatabase);
            Assert.AreEqual(redBigData.CurrentDatabase.TablesName.Count, 1);
            Assert.AreEqual(redBigData.CurrentDatabase.TablesName[0], "bigTable");
            Table table = redBigData.CurrentDatabase.GetTable("bigTable");
            Assert.AreEqual(table.Columns.Count, 0);
            Assert.AreEqual(table.Rows, 0);

            table.AddColumn("col1", TypeColumn.Int);
            Assert.AreEqual(table.Columns.Count, 1);
            table.AddColumn("name", TypeColumn.String);
            Assert.AreEqual(table.Columns.Count, 2);

            Assert.AreEqual(table.Columns[0].name, "col1");
            Assert.AreEqual(table.Columns[1].name, "name");
            Assert.AreEqual(table.Columns[0].type, TypeColumn.Int);
            Assert.AreEqual(table.Columns[1].type, TypeColumn.String);

            Assert.AreEqual(table.Rows, 0);
        }

        [TestMethod]
        public void _3_Add()
        {
            RedBigData redBigData = new(TestPath);
            redBigData.SetCurrentDatabase("bigDB");
            Table table = redBigData.CurrentDatabase!.GetTable("bigTable");

            table.AddRow(1, "allo");
            Assert.AreEqual(table.Rows, 1);

            object[][] data = table.GetRow(0, 1, "col1", "name").ToArray();
            Assert.AreEqual(data[0][0], 1);
            Assert.AreEqual(data[0][1], "allo");

            table = redBigData.CurrentDatabase!.GetTable("bigTable");

            table.AddRow(new object[] { 2, "wow" });
            Assert.AreEqual(table.Rows, 2);

            table = redBigData.CurrentDatabase!.GetTable("bigTable");

            data = table.GetRow(0, 2, "name", "col1").ToArray();
            Assert.AreEqual(data[0][1], 1);
            Assert.AreEqual(data[0][0], "allo");
            Assert.AreEqual(data[1][1], 2);
            Assert.AreEqual(data[1][0], "wow");

            table = redBigData.CurrentDatabase!.GetTable("bigTable");

            table.InsertRow(1, 3, "non");
            Assert.AreEqual(table.Rows, 3);

            table = redBigData.CurrentDatabase!.GetTable("bigTable");

            data = table.GetRow(1, 1, "col1", "name").ToArray();
            Assert.AreEqual(data[0][0], 3);
            Assert.AreEqual(data[0][1], "non");

            table = redBigData.CurrentDatabase!.GetTable("bigTable");

            table.RemoveRow(1, 1);
            Assert.AreEqual(table.Rows, 2);

            table = redBigData.CurrentDatabase!.GetTable("bigTable");

            table.RemoveRow(0, 2);
            Assert.AreEqual(table.Rows, 0);

            table = redBigData.CurrentDatabase!.GetTable("bigTable");
            Assert.AreEqual(table.Rows, 0);
        }
    }
}