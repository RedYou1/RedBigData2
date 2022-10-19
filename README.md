# RedBigData2

My litle database manager made in c#.

## How to use it

Instanciate a RedBigData object with the root path of the database (it will create the folder automaticly if not already).
You will need to create a database and use it after.

```
redBigData.CreateDatabase("database");
redBigData.SetCurrentDatabase("database");
```

You can now create your tables.

```
Table table = redBigData.CurrentDatabase.CreateTable("table")
```

And do querries with there instances.

```
Table table = redBigData.CurrentDatabase.GetTable("table");

//get
object[][] data = table.GetRow(rowIndex, rowCount, listOfColumns).ToArray(); //GetRow return an IEnumerable<object[]>
object importantInfo = data[columnIndexOfTheListPassedInThePreviousFunction][rowIndex];

//add
table.AddRow(listOfData);
table.InsertRow(rowIndex, listOfData);

//remove
table.RemoveRow(rowIndex, rowCount);
```

## How it work

### Folder hearchy
root folder
 - manager info
 - database folders
   - database info
   - table folders
     - table info
     - Column static sized data files
     - Column dynamicly sized data folders
       - data files
