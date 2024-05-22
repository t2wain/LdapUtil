using LdapUtilLib;

namespace Test
{
    public class LdapTest : IClassFixture<Context>
    {
        private readonly Context _ctx;

        public LdapTest(Context ctx)
        {
            this._ctx = ctx;
        }

        [Fact]
        public void ReadUserInfo()
        {
            var lcfg = _ctx.LdapConfig;
            var user = lcfg.GetUserInfo(_ctx.UserIDs[0], true, true);

            lcfg.LoadManagers([user], 10);
            var data = _ctx.GetUserData(user);
        }

        [Fact]
        public void ReadGroupInfo()
        {
            var lcfg = _ctx.LdapConfig;
            var group = lcfg.GetGroupInfo(_ctx.GetTestData().GroupIDs[0]);
        }
    }
}
