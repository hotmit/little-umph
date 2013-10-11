using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

using LittleUmph;

namespace LittleUmpSandBox
{
    class Samples
    {
        public Samples()
        {
            
        }

        public void DataConversionSample()
        {
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
        }

        public void UtilsSample()
        {
            string[] arr = new string[] { "It", "was", "the", "best", "of", 
                    "times,", "it", "was", "the", "worst", "of", "times" };

            // Split the array into to 3 peices of 5,5 and 2
            List<string[]> chunks = Arr.Chunk(arr, size: 5);

            // "It, was, the, best, of, times,, it, was, the, worst, of, times"
            string implode = Arr.Implode(", ", arr);

            // The bigArr will be two arr array combine together
            string[] bigArr = Arr.Merge(arr, arr);




            // "firstName" => "First Name"
            string camel = Str.BreakCamelCase("firstName");

            // Result: "It-was-the-best"
            string concat = Str.Concat("-", "It", "was", "the", "best");

            // Result: true
            bool contains = Str.Contains("It was the best of times", 
                                "Time", ignoreCase: true);

            // Result: true
            Str.ContainsAll("It was the best of times", true,
                    "was", "BEST");

            // Result: true
            Str.ContainsAny("It was the best of times", "was", "anything");

            // Result: "It was", CutRight works the same way
            Str.CutLeft("It was the best of times", 6);

            // Result: 3.5
            Str.DoubleVal("$3.50", -1);

            // Result: 37
            Str.IntVal("37", -1);

            // Result: "This i", 
            Str.MaxLength("It was the best of times", 6);
            // Result: "It was the best of times"
            // Useful for trimming buffer 
            Str.MaxLength("It was the best of times", 100);

            // Result: 5
            Str.SubCount("It was the best of times", " ");

            // Result: "times"
            Str.SubString("It was the best of times", -5);

        }


        public void BulkInsert()
        {
            /*
                CREATE TABLE [dbo].[MillionsRows](
                    [Id] [bigint] IDENTITY(1,1) NOT NULL,
                    [Name] [nvarchar](128) NULL,
                    [Height] [float] NULL,
                    [Birthday] [datetime] NULL,
                    CONSTRAINT [PK_MillionRows] PRIMARY KEY CLUSTERED 
                (
                    [Id] ASC
                )) ON [PRIMARY]
                */

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
                    dataList.Add(new object[] { name, heightInInches, DateTime.Now });
                }

                int result = db.BulkInsert(tableName: "MillionsRows", columnList: "(Name, Height, Birthday)", data: dataList);

                batchTime.Stop();
                Debug.WriteLine("{0}th Batch Time elapsed: {1}", i, batchTime.Elapsed);
            }

            totalTime.Stop();
            Debug.WriteLine("Total Time elapsed: {0}", totalTime.Elapsed);

            long rowCount = db.ScalarQuery<long>("SELECT COUNT(*) FROM MillionsRows", 0, -1);
            Debug.WriteLine("Total Rows Count: {0}", rowCount);

            // Result of 1million records inserted
            // Total Time elapsed: 00:00:39.5264486            
        }

        public void SimpleBulkInsert()
        {
            QuickDb db = new QuickDb(@"Data Source=.\SQLEXPRESS;Initial Catalog=Test;Integrated Security=SSPI;");
            db.NonQuery("TRUNCATE TABLE SimpleMillions");

            Stopwatch totalTime = new Stopwatch();
            totalTime.Start();

            int totalRecords = 1000000;
            int batchSize = 12000;

            for (int i = 0; i < totalRecords/batchSize; i++)
            {
                Stopwatch batchTime = new Stopwatch();
                batchTime.Start();

                List<object> dataList = new List<object>();
                for (int j = 0; j < batchSize; j++)
                {
                    dataList.Add(new object[] { 0, "Hello " + i.ToString() });
                }
                int result = db.BulkInsert(tableName: "SimpleMillions", columnList: "(id, text)", data: dataList);

                batchTime.Stop();
                Debug.WriteLine("{0}th Batch Time elapsed: {1}", i, batchTime.Elapsed);
            }

            totalTime.Stop();
            Debug.WriteLine("Total Time elapsed: {0}", totalTime.Elapsed);

            long rowCount = db.ScalarQuery<long>("SELECT COUNT(*) FROM SimpleMillions", 0, -1);
            Debug.WriteLine("Total Rows Count: {0:0,000}", rowCount);
            Debug.WriteLine("Rows Per Second: {0:0,000}", rowCount / totalTime.Elapsed.TotalSeconds);    
        }

        public void BulkInsertSyntax()
        {
            /*
            CREATE TABLE [dbo].[MillionsRows](
                [Id] [bigint] IDENTITY(1,1) NOT NULL,
                [Name] [nvarchar](128) NULL,
                [Height] [float] NULL,
                [Birthday] [datetime] NULL,
                CONSTRAINT [PK_MillionRows] PRIMARY KEY CLUSTERED 
            (
                [Id] ASC
            )) ON [PRIMARY]
            */

            QuickDb db = new QuickDb(@"Data Source=.\SQLEXPRESS;Initial Catalog=Test;Integrated Security=SSPI;");

            List<object> dataList = new List<object>();
            for (int j = 0; j < 10000; j++)
            {
                string name = "Name";
                int heightInInches = Num.Random(50, 100);
                DateTime birthday = DateTime.Now;
                dataList.Add(new object[] { name, heightInInches, birthday });
            }

            // tableName: the name of the database table you want to insert data into
            // columnList: specify the column name, the outter brackets are mandatory
            // data: the list of data for each row, List<object[]>  
            //       in this case object[] has a length of three. First for the name, second for height, third for b-day
            // result: number of rows inserted
            int result = db.BulkInsert(tableName: "MillionsRows", columnList: "(Name, Height, Birthday)", data: dataList);


        }

        public void InsertBinary()
        {
            QuickDb db = new QuickDb(@"Data Source=.\SQLEXPRESS;Initial Catalog=Test;Integrated Security=SSPI;");

            db.NonQuery("insert into bintest (id, bin) values (1, 0x10)");
        }

        public void SimpleEncryptionMethods()
        {
            const string secretPassPhrase = "Yo!";

            // encrypted = "R4qn9MZwIUlrAPmww3Or2dnx5ZBLFWNSEI8g+geh7z8="
            string encrypted = SimpleEncryption.Encrypt("This is my secret ...", secretPassPhrase);

            // plaintext = "This is my secret ..."
            string plaintext = SimpleEncryption.Decrypt(encrypted, secretPassPhrase);
        }

    }
}
