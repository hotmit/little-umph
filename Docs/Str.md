# String Helpers
```cs
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
```