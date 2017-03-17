using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.Live.Safety.Tools.VRAS
{
    /// <summary>
    /// ArchiveFeatureType
    /// </summary>
    public partial class ArchiveFeatureType
    {
        private static string folderDelim = @"\";
        private static string folderUndefinedValue = @"unknownParseResult";
        
        public void SortLines()
        {
            if (this.ArchiveLines != null)
            {
                Array.Sort(this.ArchiveLines);
            }
        }

        public string GetNewPortionOfTieredFolder(string copySetName, FileInfo fullFileSourceNameAndPath)
        {
            // make sure the Tiers are sorted
            this.SortLines();

            string portion = String.Empty;

            // loop through each tier
            foreach (ArchiveLineType line in this.ArchiveLines)
            {
                portion += this.ReturnCurrentTierFolder(copySetName, fullFileSourceNameAndPath, line.Option.ArchiveOption, line.Option.ArchiveOptionValue);    
            }

            if (string.IsNullOrEmpty(portion))
            {
                return folderUndefinedValue + folderDelim;
            }
            else
            {
                return portion;
            }
        }

        private string ReturnCurrentTierFolder(string copySetName, FileInfo fullFileSourceNameAndPath, ArchiveTypeArchiveOption type, string optionValue)
        {
            string current = String.Empty;
            switch (type)
            {
                case ArchiveTypeArchiveOption.CopySetName:
                    current = copySetName;
                    break;
                case ArchiveTypeArchiveOption.FileTimeDateCreatedTime:
                    break;
                case ArchiveTypeArchiveOption.FileTimeLastModifyTime:
                    break;
                case ArchiveTypeArchiveOption.RegExOnFileName:
                    break;
                case ArchiveTypeArchiveOption.RegExOnFilePath:
                    break;
                case ArchiveTypeArchiveOption.StaticName:
                    break;
            }

            if (string.IsNullOrEmpty(current))
            {
                return folderUndefinedValue + folderDelim;
            }
            else
            {
                return current + folderDelim;
            }
        }
    }
}
