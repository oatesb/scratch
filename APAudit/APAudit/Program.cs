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

                c.PerformAudit(ac);
                c.DisplayAuditResults(true);
                //c.DisplayAuditResults(false);

                c.UpdateAuditResults();
                Console.WriteLine("\n==============================================================================\n");

            }
            Console.Read();
        }
    }
}
