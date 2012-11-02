namespace Uncas.SvnTools.Core
{
    public struct RevisionRange
    {
        public RevisionRange(long from, long to) : this()
        {
            From = from;
            To = to;
        }

        public RevisionRange(long revision) : this()
        {
            From = revision;
            To = revision;
        }

        public long From { get; private set; }
        public long To { get; private set; }
    }
}