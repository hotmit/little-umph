## Project Description
> It's a little library to help you to alleviate some of the mundane stuff during your development. It has some nifty stuff like a neat database wrapper, conversion utilities, string functions and vast of other mini helpers to improve the efficiency and consistency of your code.

## Database Helper
```cs
// Get single value from a row using column index
int count = db.ScalarQuery("SELECT COUNT(Name) FROM Users", 
                  columnIndex: 0, valueOnError: -1);
             
// You can cast your result using generic query (specifying "<DateTime>" is optional)
DateTime lastUpdated = db.ScalarQuery("SELECT LastUpdated FROM Users WHERE id = 10", 
            columnName: "LastUpdated", valueOnError: DateTime.MinValue);
            
// Update using sql query
int result = db.NonQuery("UPDATE Users SET Name='John Stevenson' WHERE id=10");
 
// The library dynamicly create SqlCommand and assign the value to them 
// (the value params[] is the same order as @parameter appearances)
// You get the security of SqlCommand without 
// the hassle of building your own paramenters
SqlDataReader dr = db.DynQuery("SELECT * FROM Users WHERE Age > @age AND Birthday > @bday", 18, 
         new DateTime(2000, 1, 1)));

// DynScalar: get the first column, return -10 if an error occur
int age = db.DynScalarQuery<int>(columnIndex: 0, valueOnError: -10, 
            sql: "SELECT Age FROM Users WHERE Id = @id", parameters: 75);
           
// Trucate db log 
db.AsyncTruncateLog(sizeInMb: 10);
// Get sql server utc datetime
DateTime dbUtcTime = db.GetDbDateTimeUtc();
```
For more indept examples on this you can goto [Database Helper](Docs/Database-Helper) under documentation.


## Bulk Insert
*Inserted one million rows in less than 40 seconds.*
```cs
List<object> dataList = new List<object>();
for (int j = 0; j < 10000; j++)
{
    string name = "Name";
    int heightInInches = Num.Random(50, 100);
    DateTime birthday = DateTime.Now;
    dataList.Add(new object[] { name, heightInInches, birthday });
}
// This will insert 10,000 records at once, support sql2005 and higher
int result = db.BulkInsert(tableName: "MillionsRows", 
                     columnList: "(Name, Height, Birthday)", data: dataList);
```
Documentation: [Bulk Insert](Docs/Bulk-Insert)


## Simple Encryption
> Encrypt using AES encryption but all the passphrase and encrypted data are in string format, which make it easier to handle.

```cs
const string secretPassPhrase = "Yo!";

// encrypted = "R4qn9MZwIUlrAPmww3Or2dnx5ZBLFWNSEI8g+geh7z8="
string encrypted = SimpleEncryption.Encrypt("This is my secret ...", secretPassPhrase);

// plaintext = "This is my secret ..."
string plaintext = SimpleEncryption.Decrypt(encrypted, secretPassPhrase);
```
Documentation: [Simple Encryption](Docs/Simple-Encryption)


## Data Conversion
```
byte[] bytBin = Bin.ToBytes("10110110");
string hexBin = Bin.ToHex("10110110");
int decBin = Bin.ToInt("10110110", valueOnError: -1);

string binDec = Dec.ToBin(127);
string hexDec = Dec.ToHex(127, totalLength: 4);
byte[] bytDec = Dec.ToBytes(127, totalLength: 4);

string binHex = Hex.ToBin("FAE2 A7B2");
byte[] bytHex = Hex.ToBytes("FAE2 A7B2");
int decHex = Hex.ToInt("FAE2 A7B2", valueOnError: -1);

byte[] bytArr = new byte[] { 10, 7, 11, 34 };
string binByt = ByteArr.ToBin(bytArr);
string hexByt = ByteArr.ToHex(bytArr);
int decByt = ByteArr.ToInt(bytArr, valueOnError: -1);
string strByt = ByteArr.ToString(bytArr);
```
Documentation: [Conversions](Docs/Conversions)


## Word Of Caution
> This library is primary is using by me for my personal projects. Hence there are a few of experimental stuff inside the library. If you see a file marked as Alpha please use caution when using it. Other than that everything else I will make sure the interface remain the same even when the code changes.


# Document Index
* DB
	* [Database Helper](Docs/Database-Helper)
	* [Bulk Insert](Docs/Bulk-Insert)
* Tools
	* [Simple Encryption](Docs/Simple-Encryption)
* Utils
	* [Conversions](Docs/Conversions)
	* [Arr](Docs/Arr)
	* [Str](Docs/Str)


---
# Version Marker
* NET20 - Framework 2.0
* NET35
  * NET35; NET35_OR_GREATER;
  ```c#
  #if NET35_OR_GREATER
  using System.Linq;
  #endif
  ```

* NET40
  * NET35_OR_GREATER; NET40; NET40_OR_GREATER;
* NET45
  * NET35_OR_GREATER; NET40_OR_GREATER; NET45; NET45_OR_GREATER;
