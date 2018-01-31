using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileFormatConversion
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Need 2 arguments InputFile OutputFile");
            }
            foo(args[0], args[1]);
        }

        public static void foo(string inputFile, string outputFile)
        {
            Stopwatch watch = new Stopwatch();
            
            if (File.Exists(outputFile))
            {
                Console.WriteLine("Deleting existing output file");
                File.Delete(outputFile);
            }
            watch.Start();
            Console.WriteLine("Starting Conversion In: {0} Out:{1}", inputFile, outputFile);
            using (StreamReader sr = new StreamReader(inputFile, Encoding.Unicode, false))
            {
                using (StreamWriter sw = new StreamWriter(outputFile, false, Encoding.UTF8))
                {
                    int readChars;
                    char[] buffer = new char[128 * 1024];
                    while ((readChars = sr.ReadBlock(buffer, 0, buffer.Length)) > 0)
                    {
                        sw.Write(buffer, 0, readChars);
                    }
                }
            }
            watch.Stop();
            Console.WriteLine("Processing Complete Took {0} seconds", (watch.ElapsedMilliseconds / 1000).ToString("#.##"));
            FileInfo inFileInfo = new FileInfo(inputFile);
            FileInfo outFileInfo = new FileInfo(outputFile);
            Console.WriteLine("Original File Size: {0} KB\nOutput File Size: {1} KB", (inFileInfo.Length / 1024).ToString("#,##"), (outFileInfo.Length / 1024).ToString("#,##"));
            decimal reductionPercent = (decimal)outFileInfo.Length / (decimal)inFileInfo.Length;
            Console.WriteLine("Reduction of %: {0}",(reductionPercent * 100).ToString("#.##"));
        }
    }
}
