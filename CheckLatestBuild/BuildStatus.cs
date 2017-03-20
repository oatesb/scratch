using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLatestBuild
{
    public class BuildStatus
    {
        public string CurrentRunningBuild { get; set; }
        public string LastCompleteBuild { get; set; }
    }
}
