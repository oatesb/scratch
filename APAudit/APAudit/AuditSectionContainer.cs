using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APAudit
{
    //public enum SectionAuditType { mustContain, exactMatch }
    public class AuditSectionContainer : SectionContainerShell
    {
        public string Name { get; set; }

        public AuditSectionContainer(string name) : base()
        {
            Name = name;
        }

    }
}
