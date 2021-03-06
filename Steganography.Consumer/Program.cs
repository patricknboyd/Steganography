﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boyd.Steganography.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string imagePath = @"C:\Users\Patrick\Pictures\cats.png";
                string outputPath = @"C:\rubbish\encoded image.png";

                using (Image image = new Image())
                {
                    image.LoadImage(imagePath);

                    image.CharacterEncoding = Encoding.UTF32;

                    Console.WriteLine("Pixels: {0}", image.ImagePixels);
                    Console.WriteLine("Bits to encode: {0}", image.EncodedBits);
                    Console.WriteLine("Character Encoding: {0}", image.CharacterEncoding.EncodingName);
                    Console.WriteLine("Available bytes: {0}", image.AvailableMessageBytes);
                    Console.WriteLine("Available characters: {0}", image.AvailableMessageCharacters);

                    Console.WriteLine();

                    using (System.IO.FileStream outStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    {
                        image.EncodeMessage("Hello, world!", outStream);
                    }
                }

                System.Diagnostics.Process.Start(outputPath);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine();
            Console.WriteLine("Press <Enter> to continue...");
            Console.ReadLine();

        }
    }
}
