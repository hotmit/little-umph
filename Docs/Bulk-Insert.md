# Bulk Insert
> **This is the quickest way I found to insert data into MSSQL server. The speed is vary depend on your data and your server.  I performed a test below, inserted 1 million records into the database. It took less than 40 seconds, under half a second per 10,000 records.**

## Data Structure
```sql
CREATE TABLE [dbo](dbo).[MillionsRows](MillionsRows)(
    [Id](Id) [bigint](bigint) IDENTITY(1,1) NOT NULL,
    [Name](Name) [nvarchar](nvarchar)(128) NULL,
    [Height](Height) [float](float) NULL,
    [Birthday](Birthday) [datetime](datetime) NULL,
    CONSTRAINT [PK_MillionRows](PK_MillionRows) PRIMARY KEY CLUSTERED 
(
    [Id](Id) ASC
)) ON [PRIMARY](PRIMARY)
```

## Query Syntax
```cs
QuickDb db = new QuickDb(@"Data Source=.\SQLEXPRESS;Initial Catalog=Test;Integrated Security=SSPI;");

List<object> dataList = new List<object>();
for (int j = 0; j < 10000; j++)
{
    string name = "Name";
    int heightInInches = Num.Random(50, 100);
    DateTime birthday = DateTime.Now;
    dataList.Add(new object[]() { name, heightInInches, birthday });
}

// tableName: the name of the database table you want to insert data into
// columnList: specify the column name, the outter brackets are mandatory
// data: the list of data for each row, List<object[]()>  
//       in this case object[]() has a length of three. First for the name, second for height, third for b-day
// result: number of rows inserted
int result = db.BulkInsert(tableName: "MillionsRows", 
                    columnList: "(Name, Height, Birthday)", data: dataList);
```

## One Million Records Sample
```cs
string nameData = "A charm invests a face" +
    "a poem by Emily Dickinson" + 

    "A charm invests a face" + 
    "Imperfectly beheld." + 
    "The lady dare not lift her veil" + 
    "For fear it be dispelled." + 

    "But peers beyond her mesh," + 
    "And wishes, and denies," + 
    "Lest interview annul a want" + 
    "That image satisfies. ";


QuickDb db = new QuickDb(@"Data Source=.\SQLEXPRESS;Initial Catalog=Test;Integrated Security=SSPI;");

db.NonQuery("TRUNCATE TABLE MillionsRows");

Stopwatch totalTime = new Stopwatch();
totalTime.Start();

for (int i = 0; i < 100; i++)
{
    Stopwatch batchTime = new Stopwatch();
    batchTime.Start();

    List<object> dataList = new List<object>();
    for (int j = 0; j < 10000; j++)
    {
        string name = nameData.Substring(Num.Random(0, 50), Num.Random(5, 10));
        int heightInInches = Num.Random(50, 100);
        DateTime birthday = DateTime.Now;
        dataList.Add(new object[]() { name, heightInInches, birthday });
    }

    int result = db.BulkInsert(tableName: "MillionsRows", 
                           columnList: "(Name, Height, Birthday)", data: dataList);

    batchTime.Stop();
    Debug.WriteLine("{0}th Batch Time elapsed: {1}", i, batchTime.Elapsed);
}

totalTime.Stop();
Debug.WriteLine("Total Time elapsed: {0}", totalTime.Elapsed);

long rowCount = db.ScalarQuery<long>("SELECT COUNT(*) FROM MillionsRows", 0, -1);
Debug.WriteLine("Total Rows Count: {0}", rowCount);

// Result of 1million records inserted
// Total Time elapsed: 00:00:39.5264486 
```

## SQL Bulk Insert Syntax
> For your reference here is a list of ways you can insert multiple records at once in SQL. 
```sql
-- Combine multiple statements
INSERT MyTable (colA, colB) VALUES(1, 'a');
INSERT MyTable (colA, colB) VALUES(2, 'b');


-- Union method
INSERT MyTable (colA, colB)
     SELECT 1, 'a'
     UNION ALL
     SELECT 2, 'b';
-- As of SQL2008 The Union method can be express as:
INSERT MyTable (colA, colB)
VALUES (1, ‘a’),
(2, ‘b’)


-- Execute a string of SQL (Fastest)
INSERT MyTable (colA, colB)
     EXEC ('SELECT 1, ''a'';,
            SELECT 2, ''b'';'); 
```