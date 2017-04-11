using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APAudit
{
    public abstract class SectionContainerShell
    {

        public List<Section> Sections { get; set; }

        public SectionContainerShell()
        {
            Sections = new List<Section>();
        }

        public void AddSection(Section s)
        {
            Sections.Add(s);
        }

        public Section GetSection(String name)
        {
            try
            {
                return Sections.Where(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            }
            catch (Exception)
            {

                return null;
            }


        }
    }
}
