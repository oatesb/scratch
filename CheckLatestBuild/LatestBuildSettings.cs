using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckLatestBuild
{
    public class LatestBuildSettings
    {
        public string outputPath { get; set; }
        public string rootPath { get; set; }
        public string orderType { get; set; }
        public int forceDeployAgeHours { get; set; }
        public List<LatestBuildValidation> validations;
    }
}
