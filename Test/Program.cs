using System;
using PrettyId;

namespace Test
{
    class Program
    { 
        static void Main(string[] args)
        {
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine(IdGenerator.Generate("data_", 32));
            }

            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine(IdGenerator.Generate(64));
            }

            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine(IdGenerator.Generate());
            }

            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine(IdGenerator.Generate("key_", 32, new char[] { 'a', 'b', 'c', 'd', 'e', 'f', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }));
            }
        }
    }
}
