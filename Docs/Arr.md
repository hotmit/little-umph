# Array Helpers
```cs
string[]()() arr = new string[]()() { "It", "was", "the", "best", "of", 
        "times,", "it", "was", "the", "worst", "of", "times" };

// Split the array into to 3 peices of 5,5 and 2
List<string[]()> chunks = Arr.Chunk(arr, size: 5);

// "It, was, the, best, of, times,, it, was, the, worst, of, times"
string implode = Arr.Implode(", ", arr);

// The bigArr will be two arr array combine together
string[]() bigArr = Arr.Merge(arr, arr);
```