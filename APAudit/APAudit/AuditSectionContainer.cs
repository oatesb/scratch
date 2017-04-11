using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APAudit
{
    public enum SectionAuditType { mustContain, exactMatch }
    public class AuditSectionContainer
    {
        public string Name { get; set; }

        public List<Section> MustContainSections { get; set; }
        public List<Section> ExactMatchSections { get; set; }

        [JsonConstructor]
        public AuditSectionContainer(string name)
        {
            Name = name;
            MustContainSections = new List<Section>();
            ExactMatchSections = new List<Section>();
        }

        public void AddSection(Section s, SectionAuditType type)
        {
            switch (type)
            {
                case SectionAuditType.mustContain:
                    MustContainSections.Add(s);
                    break;
                case SectionAuditType.exactMatch:
                    ExactMatchSections.Add(s);
                    break;
                default:
                    throw new Exception(string.Format("Passed in a non supported SectionAuditType: {0} into AddSection", type.ToString()));
            }
        }

        public Section GetSection(String name, SectionAuditType type)
        {
            switch (type)
            {
                case SectionAuditType.mustContain:
                    return MustContainSections.Where(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                case SectionAuditType.exactMatch:
                    return ExactMatchSections.Where(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                default:
                    throw new Exception(string.Format("Passed in a non supported SectionAuditType: {0} into GetSection", type.ToString()));
            }
            
        }
    }
}
