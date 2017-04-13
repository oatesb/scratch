using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APAudit
{
    public enum FileFilterType { folderName, fileName}
    public class FileFilter
    {
        public string Name { get; set; }
        public FileFilterType Type { get; set; }

        public FileFilter(string name, FileFilterType type)
        {
            Name = name;
            Type = type;
        }
    }
}
