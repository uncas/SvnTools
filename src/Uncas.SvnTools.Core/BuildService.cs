using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Uncas.SvnTools.Core
{
    public class BuildService
    {
        private const int NumberOfQueues = 2;

        private readonly IVersionControlUtility _versionControlUtility;

        public BuildService(IVersionControlUtility versionControlUtility)
        {
            _versionControlUtility = versionControlUtility;
        }

        public void UpdateBuildServer()
        {
            IEnumerable<BranchInfo> branches =
                _versionControlUtility.GetBranches(
                    new Uri("svn://subversion.example.com/branches"));
            var builder = new StringBuilder();
            IOrderedEnumerable<BranchInfo> selectedBranches =
                SelectBranches(branches).OrderByDescending(x => x.LastRevision.Revision);
            Console.WriteLine("Creating build script for {0} branches.",
                              selectedBranches.Count());
            int branchIndex = 0;
            foreach (BranchInfo branch in selectedBranches)
            {
                int queueNumber = branchIndex%NumberOfQueues + 1;
                builder.AppendLine(GetBuildScript(branch, queueNumber));
                builder.AppendLine();
                branchIndex++;
            }

            string buildScript = string.Format(CultureInfo.InvariantCulture,
                                               GetBuildTemplate(),
                                               builder);
            const string fileName = "AutoBranchBuild.config";
            File.WriteAllText(fileName, buildScript);
            string destination = Path.Combine(@"C:\Builds\CCNetConfig", fileName);
            File.Copy(fileName, destination, true);
            Console.WriteLine(buildScript);
            Console.WriteLine("Created build script for {0} branches.",
                              selectedBranches.Count());
        }

        private static IEnumerable<BranchInfo> SelectBranches(
            IEnumerable<BranchInfo> branches)
        {
            return branches.Where(x => !x.Name.StartsWith("_"));
        }

        private static string GetBuildScript(BranchInfo branch, int queueNumber)
        {
            BranchBuildCondition branchBuildCondition = GetBranchBuildCondition(branch);
            if (!branchBuildCondition.ShouldBuild)
                return string.Empty;
            string branchName = branch.Name;
            string projectName = branchName;
            string queueName = "Magic-" + queueNumber;
            string category = "Magic-" + branch.LastRevision.Author;
            return string.Format(CultureInfo.InvariantCulture,
                                 @"
  <cb:AutoBranchBuild 
    ProjectName=""{0}""
    BranchName=""{1}""
    Queue=""{2}""
    Category=""{3}""
    IntervalSeconds=""{4}""
    />",
                                 projectName,
                                 branchName,
                                 queueName,
                                 category,
                                 branchBuildCondition.Interval.TotalSeconds);
        }

        private static BranchBuildCondition GetBranchBuildCondition(BranchInfo branch)
        {
            RevisionInfo lastRevision = branch.LastRevision;
            var rules = new List<IntervalRule>
                {
                    new IntervalRule(TimeSpan.FromMinutes(10d), TimeSpan.FromMinutes(1d)),
                    new IntervalRule(TimeSpan.FromHours(1d), TimeSpan.FromMinutes(2d)),
                    new IntervalRule(TimeSpan.FromDays(1d), TimeSpan.FromMinutes(3d)),
                    new IntervalRule(TimeSpan.FromDays(7d), TimeSpan.FromMinutes(5d)),
                    new IntervalRule(TimeSpan.FromDays(46d), TimeSpan.FromMinutes(30d))
                };
            foreach (IntervalRule rule in rules)
            {
                if (DateTime.Now.Subtract(lastRevision.Created) <
                    rule.MaxTimeSinceLastRevision)
                    return new BranchBuildCondition
                        {ShouldBuild = true, Interval = rule.Interval};
            }

            return new BranchBuildCondition {ShouldBuild = false};
        }

        private static string GetBuildTemplate()
        {
            return File.ReadAllText("AutoBranchBuildTemplate.config");
        }

        #region Nested type: BranchBuildCondition

        private class BranchBuildCondition
        {
            public bool ShouldBuild { get; set; }
            public TimeSpan Interval { get; set; }
        }

        #endregion

        #region Nested type: IntervalRule

        private class IntervalRule
        {
            public IntervalRule(TimeSpan maxTimeSinceLastRevision, TimeSpan interval)
            {
                MaxTimeSinceLastRevision = maxTimeSinceLastRevision;
                Interval = interval;
            }

            public TimeSpan MaxTimeSinceLastRevision { get; private set; }
            public TimeSpan Interval { get; private set; }
        }

        #endregion
    }
}