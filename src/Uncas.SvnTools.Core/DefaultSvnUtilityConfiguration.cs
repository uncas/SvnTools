namespace Uncas.SvnTools.Core
{
    public class DefaultSvnUtilityConfiguration : ISvnUtilityConfiguration
    {
        #region ISvnUtilityConfiguration Members

        public string Password
        {
            get { return "subversion22"; }
        }

        public string UserName
        {
            get { return "subversion"; }
        }

        #endregion
    }
}