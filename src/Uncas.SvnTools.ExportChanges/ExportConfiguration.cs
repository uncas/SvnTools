using System;

namespace Uncas.SvnTools.ExportChanges
{
    public class ExportConfiguration
    {
        public string UrlString { get; set; }
        public long? FromRevision { get; set; }
        public long? ToRevision { get; set; }
        public string ExportFolder { get; set; }
        public string SvnUserName { get; set; }
        public string SvnPassword { get; set; }

        public static ExportConfiguration Parse(string[] args)
        {
            var configuration = new ExportConfiguration();

            int currentIndex = 0;
            configuration.UrlString = args[currentIndex];
            currentIndex++;

            long result;
            if (Int64.TryParse(args[currentIndex], out result))
            {
                configuration.FromRevision = result;
                currentIndex++;
            }

            if (Int64.TryParse(args[2], out result))
            {
                configuration.ToRevision = result;
                currentIndex++;
            }

            configuration.ExportFolder = args[currentIndex];
            currentIndex++;

            configuration.SvnUserName = args[currentIndex];
            currentIndex++;

            configuration.SvnPassword = args[currentIndex];

            return configuration;
        }
    }
}