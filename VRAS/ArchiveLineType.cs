using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Live.Safety.Tools.VRAS
{
    /// <summary>
    /// ArchiveLineType : IComparable
    /// </summary>
    public partial class ArchiveLineType : IComparable<ArchiveLineType>
    {
        public int CompareTo(ArchiveLineType otherLine)
        {
            if (otherLine == null)
            {
                throw new ArgumentNullException("otherLine is null");
            }

            return this.Tier.CompareTo(otherLine.Tier);
        }
    }
}
