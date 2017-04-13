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



        public void UpdateAuditResults()
        {
            if (Utils.YesNoQuestion(string.Format("Would you like to edit file {0} ? (y or n)", FilePathOriginal)))
            {
                Console.WriteLine("Editing Selected...");
                EditConfiguration();
            }
            else
            {
                Console.WriteLine("Will not edit this file...");
            }
            
        }

        private void EditConfiguration()
        {
            Console.WriteLine("If needed unlock the file in your editor window.\n\tSD edit {0}", FilePathOriginal);
            if (Utils.YesNoQuestion("Would you like to Auto-Fix the file?\n\tAuto-Fix: Update all invalid status to the Desired config AND \n\tAdd the missing status Sections and KvPs."))
            {
                AutoFix();
            }
            else
            {
                ManaualFix();
            }

        }

        private string GetUpdatedEditedString(string section, string key, string originalVal, string desiredVal, SectionItemStatus status)
        {
            
            string userinput = string.Empty;
            string returnVal = string.Empty;
            do
            {
                Console.WriteLine("Enter option # value for [{0}][{1}] with status of {4}\n\to: {2}\n\td: {3}\n\tm: manually type", section, key, originalVal, desiredVal, status.ToString());
                userinput = Console.ReadLine();

            } while (userinput != "o" && userinput != "d" && userinput != "m");

            if (userinput == "o")
            {
                returnVal = originalVal;
            }
            else if (userinput == "d")
            {
                returnVal = desiredVal;

            }
            else
            {
                Console.Write("Manually Enter the new Value. "); 
                returnVal = Console.ReadLine();
            }

            return returnVal;
        }

        private void ManaualFix()
        {
            Console.WriteLine("Starting Manual-Fix");
            // invalid items or missing indiviual items
            foreach (var item in SectionData.Sections)
            {
                var filteredItems = item.Items.Where(x => x.Status != SectionItemStatus.source);
                {
                    var color = Console.ForegroundColor;
                    foreach (var sectionitem in filteredItems)
                    {
                        if (item.Status == SectionStatus.missing)
                        {
                            _iniData.Sections.AddSection(item.Name);
                        }
                        string input = GetUpdatedEditedString(item.Name, sectionitem.Name, sectionitem.Value, sectionitem.DesiredValue, sectionitem.Status);
                        // update the ini data source
                        if (sectionitem.Status == SectionItemStatus.missing)
                        {
                            _iniData[item.Name].AddKey(sectionitem.Name);
                        }
                        _iniData[item.Name][sectionitem.Name] = input;

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\t\tchanging Section[{0}][{1}] from {2} to {3}", item.Name, sectionitem.Name, sectionitem.Value, input);
                        Console.ForegroundColor = color;
                    }
                    
                }
            }
            
            Console.WriteLine("Saving the file: {0}", FilePathSave);
            _iniParser.WriteFile(FilePathSave, _iniData);
            Console.WriteLine("Complete.. Press ANY key to continue");
            Console.ReadKey();
        }

        private void AutoFix()
        {
            Console.WriteLine("Starting Auto-Fix");
            // invalid items or missing indiviual items
            foreach (var item in SectionData.Sections.Where(s => s.Status == SectionStatus.ok))
            {
                var filteredItems = item.Items.Where(x => x.Status == SectionItemStatus.invalid || x.Status == SectionItemStatus.missing);
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    foreach (var sectionitem in filteredItems)
                    {
                        // update the ini data source
                        _iniData[item.Name][sectionitem.Name] = sectionitem.DesiredValue;
                        Console.WriteLine("\t\tchanging Section[{0}][{1}] from {2} to {3}", item.Name, sectionitem.Name, sectionitem.Value, sectionitem.DesiredValue);
                    }
                    Console.ForegroundColor = color;
                }
            }

            // Add the Missing Sections
            foreach (var item in SectionData.Sections.Where(s => s.Status == SectionStatus.missing))
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                _iniData.Sections.AddSection(item.Name);
                foreach (var sectionitem in item.Items)
                {
                    // update the ini data source
                    _iniData[item.Name].AddKey(sectionitem.Name);
                    _iniData[item.Name][sectionitem.Name] = sectionitem.DesiredValue;
                    Console.WriteLine("\t\tAdding Section[{0}][{1}] DesiredValue {2}", item.Name, sectionitem.Name, sectionitem.Value);
                }
                Console.ForegroundColor = color;
            }

            Console.WriteLine("Saving the file: {0}", FilePathSave);
            _iniParser.WriteFile(FilePathSave, _iniData);
            Console.WriteLine("Complete.. Press ANY key to continue");
            Console.ReadKey();
        }

        public void DisplayAuditResults(bool hideSourceVal)
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
                IEnumerable<SectionItem> filteredItems;
                if (hideSourceVal)
                {
                    filteredItems = item.Items.Where(x => x.Status != SectionItemStatus.source);
                }
                else
                {
                    filteredItems = item.Items;
                }
                
                foreach (var sectionitem in filteredItems)
                {
                    color = Console.ForegroundColor;
                    switch (sectionitem.Status)
                    {
                        case SectionItemStatus.source:
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("\t\tName: {0}\n\t\tOriginal Val: {1}\n\t\tDesired Val: {2}\n\t\tStatus: {3}\n\t\t+++++++++++++++++", sectionitem.Name, sectionitem.Value, sectionitem.DesiredValue, sectionitem.Status);
                            break;
                        case SectionItemStatus.comparison:
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.WriteLine("\t\tName: {0}\n\t\tOriginal Val: {1}\n\t\tDesired Val: {2}\n\t\tStatus: {3}\n\t\t+++++++++++++++++", sectionitem.Name, sectionitem.Value, sectionitem.DesiredValue, sectionitem.Status);
                            break;
                        case SectionItemStatus.extra:
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine("\t\tName: {0}\n\t\tOriginal Val: {1}\n\t\tDesired Val: {2}\n\t\tStatus: {3}\n\t\t+++++++++++++++++", sectionitem.Name, sectionitem.Value, sectionitem.DesiredValue, sectionitem.Status);
                            break;
                        case SectionItemStatus.missing:
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine("\t\tName: {0}\n\t\tOriginal Val: {1}\n\t\tDesired Val: {2}\n\t\tStatus: {3}\n\t\t+++++++++++++++++", sectionitem.Name, sectionitem.Value, sectionitem.DesiredValue, sectionitem.Status);
                            break;
                        case SectionItemStatus.invalid:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\t\tName: {0}\n\t\tOriginal Val: {1}\n\t\tDesired Val: {2}\n\t\tStatus: {3}\n\t\t+++++++++++++++++", sectionitem.Name, sectionitem.Value, sectionitem.DesiredValue, sectionitem.Status);
                            break;
                        case SectionItemStatus.ok:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("\t\tName: {0}\n\t\tOriginal Val: {1}\n\t\tDesired Val: {2}\n\t\tStatus: {3}\n\t\t+++++++++++++++++", sectionitem.Name, sectionitem.Value, sectionitem.DesiredValue, sectionitem.Status);
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.DarkMagenta;
                            Console.WriteLine("\t\tName: {0}\n\t\tOriginal Val: {1}\n\t\tDesired Val: {2}\n\t\tStatus: {3}\n\t\t+++++++++++++++++", sectionitem.Name, sectionitem.Value, sectionitem.DesiredValue, sectionitem.Status);
                            break;
                    }

                    Console.ForegroundColor = color;
                }

            }
        }



    }
}
