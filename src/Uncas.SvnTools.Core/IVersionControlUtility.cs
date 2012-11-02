using System;
using System.Collections.Generic;

namespace Uncas.SvnTools.Core
{
    public interface IVersionControlUtility : IDisposable
    {
        IEnumerable<BranchInfo> GetBranches(Uri branchesBaseUrl);

        string GetMergeInfo(Uri url);

        /// <summary>
        /// Exports the revision range.
        /// </summary>
        /// <param name="repositoryUrl">The repository URL.</param>
        /// <param name="fromRevision">From revision.</param>
        /// <param name="toRevision">To revision.</param>
        /// <param name="exportFolder">The export folder.</param>
        void ExportRevisionRange(
            Uri repositoryUrl, long fromRevision, long toRevision, string exportFolder);

        /// <summary>
        /// Exports the revision range.
        /// </summary>
        /// <param name="repositoryUrl">The repository URL.</param>
        /// <param name="since">Export since this date.</param>
        /// <param name="exportFolder">The export folder.</param>
        void ExportRevisionRange(Uri repositoryUrl, DateTime since, string exportFolder);

        string GetMergeInfo(Uri url, long revision);

        long GetFirstRevisionNumber(Uri url, long lowerLimit);
        long GetFirstRevisionNumber(Uri url, DateTime since);

        long GetLastRevisionNumber(Uri url);
    }
}