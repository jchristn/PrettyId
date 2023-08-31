using System;
using PrettyId;

namespace Test
{
    class Program
    { 
        static void Main(string[] args)
        {
            string val = "";

            Console.WriteLine("");
            Console.WriteLine("32-byte values, prepended by 'data_'");
            for (int i = 0; i < 5; i++)
            {
                val = IdGenerator.Generate("data_", 32);
                Console.WriteLine(val.Length + ": " + val);
            }

            Console.WriteLine("");
            Console.WriteLine("64-byte values");
            for (int i = 0; i < 5; i++)
            {
                val = IdGenerator.Generate(64);
                Console.WriteLine(val.Length + ": " + val);
            }

            Console.WriteLine("");
            Console.WriteLine("No length-specified, should be 32-bytes");
            for (int i = 0; i < 5; i++)
            {
                val = IdGenerator.Generate();
                Console.WriteLine(val.Length + ": " + val);
            }

            Console.WriteLine("");
            Console.WriteLine("32-byte values, prepended by 'key_', hex characters only");
            for (int i = 0; i < 5; i++)
            {
                val = IdGenerator.Generate("key_", 32, new char[] { 'a', 'b', 'c', 'd', 'e', 'f', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
                Console.WriteLine(val.Length + ": " + val);
            }

            Console.WriteLine("");
            Console.WriteLine("64-byte values, hex characters only");
            for (int i = 0; i < 5; i++)
            {
                val = IdGenerator.Generate(64, new char[] { 'a', 'b', 'c', 'd', 'e', 'f', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
                Console.WriteLine(val.Length + ": " + val);
            }

            Console.WriteLine("");
            Console.WriteLine("Base64 values, total length 48-bytes");
            for (int i = 0; i < 5; i++)
            {
                val = IdGenerator.GenerateBase64(null, 48);
                Console.WriteLine(val.Length + ": " + val);
            }

            Console.WriteLine("");
            Console.WriteLine("Base64 values, with header, total length 64-bytes");
            for (int i = 0; i < 5; i++)
            {
                val = IdGenerator.GenerateBase64("b64_", 64);
                Console.WriteLine(val.Length + ": " + val);
            }
        }
    }
}
