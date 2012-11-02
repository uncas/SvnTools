using System.Collections.Generic;
using System.Linq;

namespace Uncas.SvnTools.Core
{
    public class MergeInfo
    {
        public MergeInfo(string branchName, string revisionRangeString)
        {
            BranchName = branchName;
            RevisionRangeString = revisionRangeString;
        }

        public string BranchName { get; private set; }

        private string RevisionRangeString { get; set; }

        private IEnumerable<RevisionRange> RevisionRanges
        {
            get
            {
                string[] ranges = RevisionRangeString.Split(',');
                var result = new List<RevisionRange>();
                result.AddRange(ranges.Select(MapToRange));
                return result;
            }
        }

        public long LastRevision
        {
            get { return RevisionRanges.Max(x => x.To); }
        }

        private static RevisionRange MapToRange(string value)
        {
            if (value.Contains("-"))
            {
                string[] parts = value.Split('-');
                return new RevisionRange(long.Parse(CleanValue(parts[0])),
                                         long.Parse(CleanValue(parts[1])));
            }

            string cleanedValue = CleanValue(value);
            return new RevisionRange(long.Parse(cleanedValue));
        }

        private static string CleanValue(string value)
        {
            return value.Replace("*", "");
        }
    }
}