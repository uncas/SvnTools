namespace Uncas.SvnTools.Core
{
    public class SvnUtilityConfiguration : ISvnUtilityConfiguration
    {
        public SvnUtilityConfiguration(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

        #region ISvnUtilityConfiguration Members

        public string Password { get; private set; }
        public string UserName { get; private set; }

        #endregion
    }
}