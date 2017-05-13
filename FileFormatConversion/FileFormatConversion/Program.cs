using System;
using System.Collections.Generic;
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
            foo();
        }

        public static void foo()
        {
            if (File.Exists(@"D:\URF\my-utf8.urf"))
            {
                File.Delete(@"D:\URF\my-utf8.urf");
            }
            using (StreamReader sr = new StreamReader(@"D:\URF\my.urf", Encoding.Unicode, false))
            {
                using (StreamWriter sw = new StreamWriter(@"D:\URF\my-utf8.urf", false, Encoding.UTF8))
                {
                    int readChars;
                    char[] buffer = new char[128 * 1024];
                    while ((readChars = sr.ReadBlock(buffer, 0, buffer.Length)) > 0)
                    {
                        sw.Write(buffer, 0, readChars);
                    }
                }
            }
        }
    }
}
