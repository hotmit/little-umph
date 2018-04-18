# Datatype Conversions
> I created these classes to help easily convert between different types of data. From personal experience, everytime I have to convert these things I have to dig through my notes to get the code to make it works. 

```cs
byte[]() bytBin = Bin.ToBytes("10110110");
string hexBin = Bin.ToHex("10110110");
int decBin = Bin.ToInt("10110110", valueOnError: -1);

string binDec = Dec.ToBin(127);
string hexDec = Dec.ToHex(127, totalLength: 4);
byte[]() bytDec = Dec.ToBytes(127, totalLength: 4);

string binHex = Hex.ToBin("FAE2 A7B2");
byte[]() bytHex = Hex.ToBytes("FAE2 A7B2");
int decHex = Hex.ToInt("FAE2 A7B2", valueOnError: -1);

byte[]()() bytArr = new byte[]()() { 10, 7, 11, 34 };
string binByt = ByteArr.ToBin(bytArr);
string hexByt = ByteArr.ToHex(bytArr);
int decByt = ByteArr.ToInt(bytArr, valueOnError: -1);
string strByt = ByteArr.ToString(bytArr);
```