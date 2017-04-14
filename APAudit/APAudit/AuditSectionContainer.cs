using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APAudit
{
    //public enum SectionAuditType { mustContain, exactMatch }
    public class AuditSectionContainer : SectionContainerShell
    {
        public string Name { get; set; }
        public List<FileFilter> FileFilters { get; set; }

        public AuditSectionContainer(string name) : base()
        {
            Name = name;
        }

        public List<FileInfo> GetFilteredFiles(string rootFolder, SearchOption resuriveFlag)
        {
            var files = new DirectoryInfo(rootFolder).GetFiles("*", resuriveFlag);

            List<string> folderNameContains = new List<string>();
            List<string> fileNameEquals = new List<string>();

            foreach (var item in FileFilters)
            {
                if (item.Type == FileFilterType.fileName)
                {
                    fileNameEquals.Add(item.Name);
                }
                else
                {
                    folderNameContains.Add(item.Name);
                }

            }
            List<FileInfo> returnme = new List<FileInfo>();
            foreach (var item in files)
            {
                if (folderNameContains != null && folderNameContains.Any(x => item.DirectoryName.Contains(x)))
                {
                    if (fileNameEquals != null && fileNameEquals.Any(x => item.Name.Equals(x, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        returnme.Add(item);
                    }
                }
            }

            Console.WriteLine("Found {0} files that meets your filters\n===============================", returnme.Count);
            foreach (var item in returnme)
            {
                Console.WriteLine(item.FullName);
            }
            return returnme;
        }

    }
}
