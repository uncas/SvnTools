using System;

namespace Uncas.SvnTools.Core
{
    public class RevisionInfo
    {
        public string Author { get; set; }
        public long Revision { get; set; }
        public DateTime Created { get; set; }
    }
}