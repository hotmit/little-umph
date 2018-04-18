# Simple Encryption
> Sometime you need to conceal a peice of information, but you don't want to go through the hassle of generate/store key and vector pair. Once you have the result you have to deal with byte[](), which make it more cumbersome to handle.   
> Instead of running into the risk of loosing your remaining hairs, you can use SimpleEncryption instead. It uses AES encryption in the back end so it is crytically strong and all the data is in string format so you can store and retrieve it without conversion. 

```cs
const string secretPassPhrase = "Yo!";

// encrypted = "R4qn9MZwIUlrAPmww3Or2dnx5ZBLFWNSEI8g+geh7z8="
string encrypted = SimpleEncryption.Encrypt("This is my secret ...", secretPassPhrase);

// plaintext = "This is my secret ..."
string plaintext = SimpleEncryption.Decrypt(encrypted, secretPassPhrase);
```

> By no mean I'm telling you to replace AES with these functions.  By generate the key and vector from a passphrase, it probably weaken the encryption.  However for most purposes it is more than strong enough.