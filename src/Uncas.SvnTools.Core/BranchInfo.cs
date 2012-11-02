namespace Uncas.SvnTools.Core
{
    public class BranchInfo
    {
        public string Name { get; set; }
        public RevisionInfo LastRevision { get; set; }
        //public IEnumerable<RevisionInfo> LastRevisions { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", LastRevision.Author, Name);
        }
    }
}