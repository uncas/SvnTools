using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using SharpSvn;
using SharpSvn.Security;

namespace Uncas.SvnTools.Core
{
    public class SvnUtility : IVersionControlUtility
    {
        private readonly SvnClient _client;

        /// <summary>
        /// The SVN password.
        /// </summary>
        private readonly string _svnPassword;

        /// <summary>
        /// The SVN user name.
        /// </summary>
        private readonly string _svnUserName;

        public SvnUtility(ISvnUtilityConfiguration configuration)
        {
            _svnPassword = configuration.Password;
            _svnUserName = configuration.UserName;
            _client = GetSvnClient();
        }

        public SvnUtility() : this(new DefaultSvnUtilityConfiguration())
        {
        }

        #region IVersionControlUtility Members

        public void Dispose()
        {
            _client.Dispose();
        }

        public IEnumerable<BranchInfo> GetBranches(Uri branchesBaseUrl)
        {
            var files = new List<BranchInfo>();
            var branchesTarget = new SvnUriTarget(branchesBaseUrl);
            Collection<SvnListEventArgs> svnList;
            bool gotList = _client.GetList(branchesTarget, out svnList);
            if (gotList)
            {
                files.AddRange(from svnItem in svnList
                               where IsBranch(svnItem)
                               select MapToBranchInfo(svnItem));
            }

            return files;
        }

        public string GetMergeInfo(Uri url)
        {
            var target = new SvnUriTarget(url);
            return GetMergeInfo(target);
        }

        public string GetMergeInfo(Uri url, long revision)
        {
            var target = new SvnUriTarget(url, revision);
            return GetMergeInfo(target);
        }

        public long GetFirstRevisionNumber(Uri url, long lowerLimit)
        {
            Collection<SvnLogEventArgs> logItems;
            var svnLogArgs = new SvnLogArgs
                {StrictNodeHistory = true, End = new SvnRevision(lowerLimit)};
            bool success = _client.GetLog(url, svnLogArgs, out logItems);
            if (!success)
            {
                throw new NotSupportedException();
            }

            return logItems.Min(x => x.Revision);
        }

        public long GetFirstRevisionNumber(Uri url, DateTime since)
        {
            Collection<SvnLogEventArgs> logItems;
            var svnLogArgs = new SvnLogArgs
                {StrictNodeHistory = true, End = new SvnRevision(since)};
            bool success = _client.GetLog(url, svnLogArgs, out logItems);
            if (!success)
            {
                throw new NotSupportedException();
            }

            return logItems.Min(x => x.Revision);
        }

        public long GetLastRevisionNumber(Uri url)
        {
            Collection<SvnLogEventArgs> logItems;
            var svnLogArgs = new SvnLogArgs {StrictNodeHistory = true};
            bool success = _client.GetLog(url, svnLogArgs, out logItems);
            if (!success)
            {
                throw new NotSupportedException();
            }

            return logItems.Max(x => x.Revision);
        }

        /// <summary>
        /// Exports the revision range.
        /// </summary>
        /// <param name="repositoryUrl">The repository URL.</param>
        /// <param name="since">Export since this date.</param>
        /// <param name="exportFolder">The export folder.</param>
        public void ExportRevisionRange(
            Uri repositoryUrl, DateTime since, string exportFolder)
        {
            if (!Directory.Exists(exportFolder))
                Directory.CreateDirectory(exportFolder);
            long fromRevision = GetFirstRevisionNumber(repositoryUrl, since);
            long toRevision = GetLastRevisionNumber(repositoryUrl);
            string versioninfoXml = Path.Combine(exportFolder, "VersionInfo.xml");
            File.WriteAllText(versioninfoXml,
                              string.Format(@"<?xml version=""1.0""?>
<versions xmlns='http://www.example.com'>
  <fromRevision>{0}</fromRevision>
  <toRevision>{1}</toRevision>
</versions>",
                                            fromRevision,
                                            toRevision));
            ExportRevisionRange(repositoryUrl, fromRevision, toRevision, exportFolder);
        }

        /// <summary>
        /// Exports the revision range.
        /// </summary>
        /// <param name="repositoryUrl">The repository URL.</param>
        /// <param name="fromRevision">From revision.</param>
        /// <param name="toRevision">To revision.</param>
        /// <param name="exportFolder">The export folder.</param>
        public void ExportRevisionRange(
            Uri repositoryUrl, long fromRevision, long toRevision, string exportFolder)
        {
            SvnInfoEventArgs info;
            _client.GetInfo(new SvnUriTarget(repositoryUrl), out info);
            long maxRevision = Math.Min(info.LastChangeRevision, toRevision);
            var to = new SvnUriTarget(repositoryUrl, maxRevision);
            Collection<SvnDiffSummaryEventArgs> list;
            var from = new SvnUriTarget(repositoryUrl, fromRevision);
            _client.GetDiffSummary(from, to, out list);
            Console.WriteLine("Items in list: " + list.Count);
            var exceptions = new List<Exception>();

            foreach (SvnDiffSummaryEventArgs item in list)
            {
                if (item.DiffKind == SvnDiffKind.Deleted ||
                    item.NodeKind != SvnNodeKind.File)
                {
                    continue;
                }

                var target = new SvnUriTarget(item.ToUri);
                string exportPath = Path.Combine(exportFolder,
                                                 item.Path.Replace("/", "\\"));
                var fi = new FileInfo(exportPath);
                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }

                Console.WriteLine("Exporting {0}", item.Path);
                try
                {
                    _client.Export(target,
                                   exportPath,
                                   new SvnExportArgs {Revision = maxRevision});
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            foreach (Exception ex in exceptions)
            {
                Console.WriteLine(ex);
            }

            if (exceptions.Count > 0)
            {
                throw new Exception("Found errors.");
            }
        }

        #endregion

        private string GetMergeInfo(SvnTarget target)
        {
            string mergeInfo;
            _client.GetProperty(target, "svn:mergeinfo", out mergeInfo);
            return mergeInfo;
        }

        private static BranchInfo MapToBranchInfo(SvnListEventArgs svnItem)
        {
            //Collection<SvnLogEventArgs> logItems;
            //var svnLogArgs = new SvnLogArgs {Limit = 3};
            //_client.GetLog(svnItem.EntryUri, svnLogArgs, out logItems);
            var branchInfo = new BranchInfo
                {
                    Name = svnItem.Path,
                    LastRevision =
                        new RevisionInfo
                            {
                                Author = svnItem.Entry.Author,
                                Created = svnItem.Entry.Time,
                                Revision = svnItem.Entry.Revision
                            },
                    //LastRevisions = logItems.Select(MapToRevisionInfo)
                };
            return branchInfo;
        }

        private static RevisionInfo MapToRevisionInfo(SvnLogEventArgs svnLogEventArgs)
        {
            if (svnLogEventArgs == null)
                return null;
            return new RevisionInfo
                {
                    Author = svnLogEventArgs.Author,
                    Created = svnLogEventArgs.Time,
                    Revision = svnLogEventArgs.Revision
                };
        }

        private static bool IsBranch(SvnListEventArgs svnItem)
        {
            return svnItem.Entry.NodeKind == SvnNodeKind.Directory &&
                   !string.IsNullOrEmpty(svnItem.Path);
        }

        private SvnClient GetSvnClient()
        {
            var client = new SvnClient();
            client.Authentication.UserNamePasswordHandlers +=
                Authentication_UserNamePasswordHandlers;
            return client;
        }

        /// <summary>
        /// Handles the UserNamePasswordHandlers event of the Authentication control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SharpSvn.Security.SvnUserNamePasswordEventArgs"/> instance containing the event data.</param>
        private void Authentication_UserNamePasswordHandlers(
            object sender, SvnUserNamePasswordEventArgs e)
        {
            e.UserName = _svnUserName;
            e.Password = _svnPassword;
        }
    }
}