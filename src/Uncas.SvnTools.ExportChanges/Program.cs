using System;
using Uncas.SvnTools.Core;

namespace Uncas.SvnTools.ExportChanges
{
    /// <summary>
    /// Program for exporting changes from source control.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Mains the specified args.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        private static void Main(string[] args)
        {
            Run(args);

            // Demo: urlString fromRevision toRevision exportFolder svnUserName svnPassword
            //program.Run(new[]
            //                {
            //                    "svn://subversion.example.com",
            //                    "30000",
            //                    "30200",
            //                    @"C:\Temp\ExampleExport",
            //                    "subversion",
            //                    "subversion22"
            //                });
        }

        /// <summary>
        /// Runs the specified args.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        private static void Run(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine(
                    "Arguments: urlString (fromRevision toRevision) exportFolder svnUserName svnPassword");
                Console.WriteLine("Examples:");
                Console.WriteLine(
                    "  -  https://svn.example.com/trunk 123 174 C:\\Builds\\Example myUserName myPassword");
                Console.WriteLine(
                    "  -  https://svn.example.com/trunk C:\\Builds\\Example myUserName myPassword");
                return;
            }

            Console.WriteLine("Starting SVN Export");

            ExportConfiguration conf = ExportConfiguration.Parse(args);

            string urlString = conf.UrlString;
            long? fromRevision = conf.FromRevision;
            long? toRevision = conf.ToRevision;
            string exportFolder = conf.ExportFolder;
            string svnUserName = conf.SvnUserName;
            string svnPassword = conf.SvnPassword;

            var svnUtility =
                new SvnUtility(new SvnUtilityConfiguration(svnUserName, svnPassword));
            var repositoryUrl = new Uri(urlString);
            if (fromRevision.HasValue && toRevision.HasValue)
            {
                svnUtility.ExportRevisionRange(repositoryUrl,
                                               fromRevision.Value,
                                               toRevision.Value,
                                               exportFolder);
            }
            else
            {
                svnUtility.ExportRevisionRange(repositoryUrl,
                                               DateTime.Now.AddDays(-61d),
                                               exportFolder);
            }

            Console.WriteLine("Stopping SVN Export");
        }
    }
}