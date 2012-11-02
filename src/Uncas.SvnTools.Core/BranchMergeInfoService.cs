using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uncas.SvnTools.Core
{
    public class BranchMergeInfoService
    {
        private readonly IVersionControlUtility _versionControlUtility;

        public BranchMergeInfoService(IVersionControlUtility versionControlUtility)
        {
            _versionControlUtility = versionControlUtility;
        }

        public void FindReleasedBranches(
            Uri trunkUrl, Uri branchParentUrl, string fileName)
        {
            Func<IList<MergeInfo>, BranchInfo, bool> comparison =
                (mergeInfo, branch) => mergeInfo.Any(x => BranchHasBeenMerged(x, branch));
            FindBranches(trunkUrl, branchParentUrl, fileName, comparison);
        }

        public void FindUnreleasedBranches(
            Uri trunkUrl, Uri branchesParentUrl, string fileName)
        {
            Func<IList<MergeInfo>, BranchInfo, bool> comparison =
                (mergeInfo, branch) => !mergeInfo.Any(x => BranchHasBeenMerged(x, branch));
            FindBranches(trunkUrl, branchesParentUrl, fileName, comparison);
        }

        public IEnumerable<MergeInfo> GetMergedBranches(
            Uri destinationUrl, Uri branchesParentUrl)
        {
            IList<MergeInfo> mergeInfoHead = GetMergeInfo(destinationUrl,
                                                          branchesParentUrl);
            long firstRevisionNumber =
                _versionControlUtility.GetFirstRevisionNumber(destinationUrl, 1);
            IList<MergeInfo> mergeInfoBase = GetMergeInfo(destinationUrl,
                                                          branchesParentUrl,
                                                          firstRevisionNumber);
            var result = new List<MergeInfo>();
            foreach (MergeInfo mergeInfo in mergeInfoHead)
            {
                string branchName = mergeInfo.BranchName;
                MergeInfo branchInfoInBase =
                    mergeInfoBase.SingleOrDefault(x => x.BranchName == branchName);
                if (branchInfoInBase == null)
                    result.Add(mergeInfo);
                else if (branchInfoInBase.LastRevision < mergeInfo.LastRevision)
                {
                    // TODO: Modify revision range:
                    result.Add(mergeInfo);
                }
            }

            return result;
        }

        private static bool BranchHasBeenMerged(MergeInfo mergeInfo, BranchInfo branch)
        {
            return mergeInfo.BranchName == branch.Name &&
                   mergeInfo.LastRevision == branch.LastRevision.Revision;
        }

        private static IList<MergeInfo> ParseMergeInfo(
            string mergeInfoString, string exclude)
        {
            var result = new List<MergeInfo>();
            string[] parts = mergeInfoString.Split(new[] {Environment.NewLine},
                                                   StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                string trimmed = part.Replace(exclude, string.Empty);
                string[] strings = trimmed.Split(':');
                string branchName = strings[0];
                string revisionRange = strings[1];
                result.Add(new MergeInfo(branchName, revisionRange));
            }

            return result;
        }

        private void FindBranches(
            Uri trunkUrl,
            Uri branchesParentUrl,
            string fileName,
            Func<IList<MergeInfo>, BranchInfo, bool> comparison)
        {
            IList<MergeInfo> mergeInfo = GetMergeInfo(trunkUrl, branchesParentUrl);

            // Compare with branches under Feature/branches:
            IEnumerable<BranchInfo> branches = GetBranches(branchesParentUrl);
            IList<BranchInfo> alreadyMerged =
                branches.Where(branch => comparison(mergeInfo, branch)).ToList();

            // Write output to file:
            File.WriteAllLines(fileName, alreadyMerged.Select(x => x.ToString()).ToArray());
        }

        private IList<MergeInfo> GetMergeInfo(
            Uri destinationUrl, Uri branchesParentUrl, long? revisionNumber = null)
        {
            string mergeInfoString = revisionNumber.HasValue
                                         ? _versionControlUtility.GetMergeInfo(
                                             destinationUrl, revisionNumber.Value)
                                         : _versionControlUtility.GetMergeInfo(
                                             destinationUrl);
            return ParseMergeInfo(mergeInfoString, branchesParentUrl.AbsolutePath + "/");
        }

        private IEnumerable<BranchInfo> GetBranches(Uri branchesUrl)
        {
            return _versionControlUtility.GetBranches(branchesUrl);
        }
    }
}