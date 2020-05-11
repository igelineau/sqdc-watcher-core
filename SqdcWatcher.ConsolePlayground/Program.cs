﻿using System;

namespace SqdcWatcher.ConsolePlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                Console.WriteLine();
                Console.WriteLine(
                    $"Key {keyInfo.Key}, KeyChar {(int)keyInfo.KeyChar}, modifiers = {keyInfo.Modifiers}");
            }
        }
    }
}