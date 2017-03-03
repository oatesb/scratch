using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APAudit
{
    public class AuditItem
    {
        public string Section { get; set; }
        List<AuditResults> Audits;
    }
}
