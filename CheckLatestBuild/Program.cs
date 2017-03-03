using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLatestBuild
{
    class Program
    {
        static void Main(string[] args)
        {
            string outputPath = @"\\BOONE-DEV\TestSeDeploy\results.txt";
            string rootPath = @"\\unistore\build\Safety\SmartScreenSvc\master";

            //var dirs = Directory.EnumerateDirectories(rootPath,"*",SearchOption.TopDirectoryOnly);
            var dd = new DirectoryInfo(rootPath);
            var dirs = dd.GetDirectories("*", SearchOption.TopDirectoryOnly);

            var dirsByDate = dirs.OrderByDescending(d => d.CreationTime).Take(10);

            var dirsByStringOrder = dirs.OrderByDescending(d => d.Name).Take(10);

            Console.WriteLine("++++++++ Name Order +++++++++++");
            foreach (var item in dirsByStringOrder)
            {
                Console.WriteLine(item);
            }

            Console.WriteLine("++++++++ Date Order +++++++++++");
            foreach (var item in dirsByDate)
            {
                Console.WriteLine(item);
            }

            var selectedBuild = dirs.OrderByDescending(d => d.Name).FirstOrDefault();


            Console.WriteLine("Last Build: {0}", selectedBuild.Name);

            if (File.Exists(outputPath))
            {
                var lastBuild = File.ReadAllText(outputPath);

                var result = BuildFirstGreaterThanSecondByStringName(selectedBuild.Name, lastBuild);

                if (result)
                {
                    File.WriteAllText(outputPath, selectedBuild.Name);
                }
            }
            else
            {
                File.WriteAllText(outputPath, selectedBuild.Name);
            }

            Console.Read();

        }

        public static bool BuildFirstGreaterThanSecondByStringName(string first, string second)
        {
            var result = first.CompareTo(second);
            if (result < 0)
            {
                return false;
            }
            else if (result > 1)
            {
                return true;
            }
            else // then they are =
            {
                return true;
            }
        }


    }
}
