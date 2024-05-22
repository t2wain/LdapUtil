using System.Collections.Generic;

namespace LdapUtilLib
{
    public class LdapExample
    {
        private readonly LdapConfig _cfg;

        public LdapExample(LdapConfig cfg)
        {
            this._cfg = cfg;
        }

        public ADUser FindUser(string userId)
        {
            var u = _cfg.GetUserInfo(userId);
            return u;
        }

        public IEnumerable<ADUser> FindUsers(IEnumerable<string> userids)
        {
            var lstu = _cfg.GetUserInfo(userids);
            return lstu;
        }

    }
}
