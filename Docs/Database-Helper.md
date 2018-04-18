# Database Helpers
```cs
// You do not need to open or close the connection
// the library will do that for you
// The only time you need to close something is 
// when you receive a SqlDataReader
QuickDb db = new QuickDb("Data Source=local;Initial Catalog=SampleDb;Integrated Security=SSPI;");
                       
// Using datareader
using (SqlDataReader dr = db.Query("SELECT * FROM Users"))
{
    // Use the dr here                
}
            
// Get single value from a row using column index
int count = db.ScalarQuery("SELECT COUNT(Name) FROM Users", 
                  columnIndex: 0, valueOnError: -1);
int sameCount = db.ScalarQuery("SELECT COUNT(Name) FROM Users", 0, -1);
            
// You can cast your result using generic query (specifying "<DateTime>" is optional)
DateTime lastUpdated = db.ScalarQuery("SELECT LastUpdated FROM Users WHERE id = 10", 
            columnName: "LastUpdated", valueOnError: DateTime.MinValue);

// Scalar query using sqlcomand, sometime using string query is unsafe
SqlCommand cmdPhone = new SqlCommand("SELECT Phone FROM Users WHERE id = @id");
cmdPhone.Parameters.AddWithValue("@id", 75);
string phone = db.ScalarQuery(cmdPhone, "Phone", "No Phone");
            
// Update using sql query
int result = db.NonQuery("UPDATE Users SET Name='John Stevenson' WHERE id=10");
// Error
if (result == QuickDb.INT_NonQueryError)
{   // Error message
    string error = db.LastExeception.Message;
}


// These are the most interesting functions and I use them most often
// DynQuery, DynNonQuery, DynScalarQuery. The library dynamicly create
// SqlCommand and assign the value to them 
// (the value params[]() is the same order as parameter appearances)
// You get the security of SqlCommand without 
// the hassle of building your own paramenters
using (SqlDataReader dr = db.DynQuery("SELECT * FROM Users WHERE Age > @age AND Birthday > @bday", 18, new DateTime(2000, 1, 1)))
{
    while (dr.Read())
    {
        Console.WriteLine(dr["Name"](_Name_).ToString());
    }
}

// DynScalar: get the first column, return -10 if an error occur
int age = db.DynScalarQuery<int>(columnIndex: 0, valueOnError: -10, 
            sql: "SELECT Age FROM Users WHERE Id = @id", parameters: 75);
int ageShorterLessLegible  = db.DynScalarQuery(0, -10, 
            "SELECT Age FROM Users WHERE Id = @id", 75);

// You can specify date as DateTime or a string literal, 
// in this case it a formatted sqldate string style 121
db.DynNonQuery("UPDATE Users SET Name=@name, Birthday=@bday WHERE Id=@id", 
        "Elvis Presley", "1935-01-08", 135);
           
// Trucate db log 
db.AsyncTruncateLog(sizeInMb: 10);

// Get sql server utc datetime
DateTime dbUtcTime = db.GetDbDateTimeUtc();
```