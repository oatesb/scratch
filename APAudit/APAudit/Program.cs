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
            string auditFile = @"auditvalues.json";

            if (args.Length == 1)
            {
                auditFile = args[0];
            }
            AuditSectionContainer ac = JsonConvert.DeserializeObject<AuditSectionContainer>(File.ReadAllText(auditFile));

            var files = ac.GetFilteredFiles(".", SearchOption.AllDirectories);

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
        }
    }
}
