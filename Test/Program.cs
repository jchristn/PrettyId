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
        }
    }
}
