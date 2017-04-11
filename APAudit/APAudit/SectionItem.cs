using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APAudit
{
    public enum  SectionItemStatus
    {
        source, // before compare: original data
        comparison, // before compare: regex to compare against source
        extra, // after compare: this was an extra item in the source
        missing, // after compare: this value was not in the source but should have been
        invalid, // after compare: name matches but the source data is wrong
        ok // after compare: all checked out from the source and comparison
    }
    public class SectionItem : IEquatable<SectionItem>
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string DesiredValue { get; set; }
        public SectionItemStatus Status { get; set; }

        [JsonConstructor]
        public SectionItem(string name, string value, SectionItemStatus status)
        {
            Name = name;
            Value = value;
            Status = status;
            DesiredValue = string.Empty;
        }

        public SectionItem(string name, string value, string desiredVal, SectionItemStatus status)
        {
            Name = name;
            Value = value;
            Status = status;
            DesiredValue = desiredVal;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            SectionItem objAsSectionItem = obj as SectionItem;
            if (objAsSectionItem == null)
            {
                return false;
            }
            else
            {
                return this.Name.Equals(objAsSectionItem.Name) &&
                    this.Value.Equals(objAsSectionItem.Value);
            }
        }

        public bool Equals(SectionItem other)
        {
            if (other == null)
            {
                return false;
            }
            return this.Name.Equals(other.Name) &&
                this.Value.Equals(other.Value);
        }
    }
}
