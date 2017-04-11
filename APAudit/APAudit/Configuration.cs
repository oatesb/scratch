using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using System.IO;

namespace APAudit
{
    public class Configuration
    {
        public string FilePathOriginal { get; set; }
        public string FilePathSave { get; set; }
        public SectionContainer SectionData { get; set; }
        private IniData _iniData;
        private FileIniDataParser _iniParser;

        public Configuration(string filepath, string fileSavePath)
        {
            FilePathOriginal = filepath;
            FilePathSave = fileSavePath;
            SectionData = new SectionContainer();

            Init();
        }

        private void Init()
        {
            try
            {
                if (!File.Exists(FilePathOriginal))
                {
                    throw new FileNotFoundException(string.Format("Could not find file: {0} during loading Configuration", FilePathOriginal));
                }
                _iniParser = new FileIniDataParser();

                _iniParser.Parser.Configuration.AssigmentSpacer = "";

                _iniData = _iniParser.ReadFile(FilePathOriginal);

                foreach (var item in _iniData.Sections)
                {
                    Section s = new Section(item.SectionName);
                    foreach (var sectionItem in item.Keys)
                    {
                        s.UpsertSectionItem(new SectionItem(sectionItem.KeyName, sectionItem.Value, SectionItemStatus.source));
                    }

                    SectionData.AddSection(s);
                }
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void SaveIniConfig()
        {
            try
            {
                _iniParser.WriteFile(FilePathSave, _iniData);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void PerformAudit(AuditSectionContainer audit)
        {
            // must have at least these in the source from the audit side
            foreach (var item in audit.MustContainSections)
            {
                Section s = SectionData.GetSection(item.Name);
                if (s != null)
                {
                    s.CompareThisAgainst(item);
                    
                }
                else
                {
                    Section newSection = new Section(item.Name);
                    foreach (var i in item.Items)
                    {
                        newSection.Items.Add(new SectionItem(i.Name, string.Empty, i.Value, SectionItemStatus.missing));
                    }
                    SectionData.AddSection(s);
                }
            }

            // source must look exactly like these.
            // must have at least these in the source from the audit side
            foreach (var item in audit.ExactMatchSections)
            {
                Section s = SectionData.GetSection(item.Name);
                if (s != null)
                {
                    s.CompareThisAgainst(item);
                }
                else
                {
                    Section newSection = new Section(item.Name);
                    foreach (var i in item.Items)
                    {
                        newSection.Items.Add(new SectionItem(i.Name, i.Value, SectionItemStatus.missing));
                    }
                    SectionData.AddSection(s);
                }
            }

        }



    }
}
