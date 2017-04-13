using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APAudit
{
    public static class Utils
    {
        public static bool YesNoQuestion(string question)
        {
            string answer = string.Empty;
            do
            {
                Console.Write(question);
                answer = Console.ReadLine();

            } while (answer != "n" && answer != "y");

            answer = answer == "n" ? "false" : "true";
            return System.Convert.ToBoolean(answer);
        }
        public static List<FileInfo> GetFilteredFiles(string rootFolder, SearchOption resuriveFlag, List<string> folderNameContains, List<string> fileNameEquals)
        {
            var files = new DirectoryInfo(rootFolder).GetFiles("*", resuriveFlag);

            List<FileInfo> returnme = new List<FileInfo>();
            foreach (var item in files)
            {
                Console.WriteLine("Process: {0}", item.FullName);
                if (folderNameContains != null && folderNameContains.Any(x => item.DirectoryName.Contains(x)))
                {
                    Console.WriteLine("\t{0} Passed the folder filter.  Seeing if it now passed the file filters", item.FullName);
                    if (fileNameEquals != null && fileNameEquals.Any(x => item.Name.Equals(x, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        Console.WriteLine("\t\tAdd: {0}", item.FullName);
                        returnme.Add(item);
                    }

                }


            }

            return returnme;
        }
    }

}
