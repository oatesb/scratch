using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APAudit
{
    public class SectionContainer
    {
        public List<Section> Sections { get; set; }

        public SectionContainer()
        {
            Sections = new List<Section>();
        }

        public void AddSection(Section s)
        {
            Sections.Add(s);
        }

        public Section GetSection(String name)
        {
            return Sections.Where(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
        }
    }
}
