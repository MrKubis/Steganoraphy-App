// See https://aka.ms/new-console-template for more information
using System.Collections;
using System.Text;
using System.Linq;
using test;
using System.ComponentModel;

//MESSAGE PART

string message = "dupa dupa 123";
string input = "test.png";
string output = "result.png";

Console.WriteLine("Encrypting: " + message + " in: " + input + "...");
LSB.EncryptPNGImage(input, output,message);

Console.WriteLine();

Console.WriteLine("Decrypting " + output + "...");
BitArray bits = LSB.DecryptPNGImage(output);
byte[] bytes = LSB.ToByteArray(bits);
string text = Encoding.UTF8.GetString(bytes);
Console.WriteLine(text);
