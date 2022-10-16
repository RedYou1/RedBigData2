namespace RedBigDataTests
{
    using RedBigDataNamespace;

    [TestClass]
    public class RedBigDataTests
    {
        public const string TestPath = @"..\..\..\Databases";

        [TestMethod]
        public void RedBigDataTest()
        {
            if (Directory.Exists(TestPath))
                Directory.Delete(TestPath, true);
            {
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
            }
        }
    }
}