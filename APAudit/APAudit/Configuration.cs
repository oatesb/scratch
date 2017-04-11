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
                    Section s = new Section(item.SectionName, SectionStatus.ok);
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
            foreach (var item in audit.Sections)
            {
                Section s = SectionData.GetSection(item.Name);
                if (s != null)
                {
                    s.CompareThisAgainst(item);
                    
                }
                else
                {
                    Section newSection = new Section(item.Name, SectionStatus.missing);
                    foreach (var i in item.Items)
                    {
                        newSection.Items.Add(new SectionItem(i.Name, string.Empty, i.Value, SectionItemStatus.missing));
                    }
                    SectionData.AddSection(newSection);
                }
            }
        }

        public void DisplayAuditResults()
        {
            Console.WriteLine("Displaying Audit results for {0}", FilePathOriginal);
            foreach (var item in SectionData.Sections)
            {
                
                var color = Console.ForegroundColor;
                switch (item.Status)
                {
                    case SectionStatus.ok:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\tSection: {0}", item.Name);
                        Console.WriteLine("\t=================================================");
                        break;
                    case SectionStatus.missing:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\tSection: {0}", item.Name);
                        Console.WriteLine("\t=================================================");
                        break;
                    default:
                        break;
                }
                Console.ForegroundColor = color;
                Console.WriteLine("\t\tName   Value   DesiredValue   Status");

                foreach (var sectionitem in item.Items)
                {
                    color = Console.ForegroundColor;
                    switch (sectionitem.Status)
                    {
                        case SectionItemStatus.source:
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("\t\t{0}: {1}: {2}: {3}", sectionitem.Name, sectionitem.Value, sectionitem.DesiredValue, sectionitem.Status);
                            break;
                        case SectionItemStatus.comparison:
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.WriteLine("\t\t{0}: {1}: {2}: {3}", sectionitem.Name, sectionitem.Value, sectionitem.DesiredValue, sectionitem.Status);
                            break;
                        case SectionItemStatus.extra:
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine("\t\t{0}: {1}: {2}: {3}", sectionitem.Name, sectionitem.Value, sectionitem.DesiredValue, sectionitem.Status);
                            break;
                        case SectionItemStatus.missing:
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine("\t\t{0}: {1}: {2}: {3}", sectionitem.Name, sectionitem.Value, sectionitem.DesiredValue, sectionitem.Status);
                            break;
                        case SectionItemStatus.invalid:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\t\t{0}: {1}: {2}: {3}", sectionitem.Name, sectionitem.Value, sectionitem.DesiredValue, sectionitem.Status);
                            break;
                        case SectionItemStatus.ok:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("\t\t{0}: {1}: {2}: {3}", sectionitem.Name, sectionitem.Value, sectionitem.DesiredValue, sectionitem.Status);
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.DarkMagenta;
                            Console.WriteLine("\t\t{0}: {1}: {2}: {3}", sectionitem.Name, sectionitem.Value, sectionitem.DesiredValue, sectionitem.Status);
                            break;
                    }

                    Console.ForegroundColor = color;
                }

            }
        }



    }
}
