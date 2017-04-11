using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using Newtonsoft.Json;

namespace APAudit
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new FileIniDataParser();

            parser.Parser.Configuration.AssigmentSpacer = "";

            List<string> folderFilter = new List<string>();
            folderFilter.Add("bn2");
            folderFilter.Add("goodsfsdf");
            folderFilter.Add("global");

            List<string> fileFilter = new List<string>();
            fileFilter.Add("bn2");
            fileFilter.Add("Environment.ini");

            var files = Utils.GetFilteredFiles(".", SearchOption.AllDirectories, folderFilter, fileFilter);

            AuditSectionContainer ac = JsonConvert.DeserializeObject<AuditSectionContainer>(File.ReadAllText(@"auditvalues.json"));

            foreach (var item in files)
            {
                Configuration c = new Configuration(item.FullName, item.FullName + ".tmp");
                Console.WriteLine();
                //IniData data = parser.ReadFile(item.FullName);

                //data["General"].AddKey("Ben", "Oates");

                ////data.Sections.AddSection("FoooBaaar");
                ////data["FoooBaaar"].AddKey("Hello", "GoodBye");


                //string newFile = Path.Combine(item.DirectoryName + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(item.Name) + "_temp" + item.Extension);
                //parser.WriteFile(newFile, data);

                c.PerformAudit(ac);

            }

            
            SectionContainer container = new SectionContainer();
            Section one = new Section("one");
            Section oneComp = new Section("one");
            Section two = new Section("two");

            container.AddSection(one);
            container.AddSection(two);

            one.UpsertSectionItem(new SectionItem("food", "pizza", SectionItemStatus.source));
            one.UpsertSectionItem(new SectionItem("pet", "dog", SectionItemStatus.source));
            one.UpsertSectionItem(new SectionItem("name", "Ben", SectionItemStatus.source));

            oneComp.UpsertSectionItem(new SectionItem("pet", "dog", SectionItemStatus.source));
            oneComp.UpsertSectionItem(new SectionItem("name", "Jill", SectionItemStatus.source));
            oneComp.UpsertSectionItem(new SectionItem("sport", "football", SectionItemStatus.source));

            Console.Read();
        }
    }


    public static class Utils
    {
        public static List<FileInfo> GetFilteredFiles(string rootFolder, SearchOption resuriveFlag, List<string> folderNameContains, List<string> fileNameEquals )
        {
            var files = new DirectoryInfo(rootFolder).GetFiles("*", resuriveFlag);

            List<FileInfo> returnme = new List<FileInfo>();
            foreach (var item in files)
            {
                Console.WriteLine("Process: {0}", item.FullName);
                if (folderNameContains != null && folderNameContains.Any(x=> item.DirectoryName.Contains(x)))
                {
                    Console.WriteLine("\t{0} Passed the folder filter.  Seeing if it now passed the file filters", item.FullName);
                    if (fileNameEquals != null && fileNameEquals.Any(x=> item.Name.Equals(x, StringComparison.CurrentCultureIgnoreCase)))
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
