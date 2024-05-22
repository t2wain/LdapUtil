using ClassifierLib;
using ClassifierLib.Entities;
using LdapUtilLib;
using System.Text.Json;

namespace Test
{
    public class Context
    {
        public class TestData
        {
            public string LdapRootUrl { get; set; } = null!;
            public string[] UserIDs { get; set; } = [];
            public string[] GroupIDs { get; set; } = [];
            public string[] UserData { get; set; } = [];
        }

        TestData data = null!;
        public Context()
        {
            data = ReadTestData();
        }

        public TestData GetTestData() => data;

        public TestData ReadTestData()
        {
            using var r = File.OpenRead("C:\\devgit\\Data\\TestData.json");
            var td = JsonSerializer.Deserialize<TestData>(r);
            return td!;
        }

        public string[] UserIDs => data.UserIDs;

        public LdapConfig LdapConfig =>
            new LdapConfig
            {
                RootURL = data.LdapRootUrl
            };

        public ADUser GetUser(string username)
        {
            var lcfg = LdapConfig;
            var user = lcfg.GetUserInfo(username);
            lcfg.LoadManagers([user], 10);
            return user;
        }


        IEnumerable<Rule> _rules = null!;
        public IEnumerable<Rule> ReadRule()
        {
            var filePath = "C:\\devgit\\Data\\Rules.xml";
            if (_rules == null) 
                _rules = RuleExtensions.ReadRuleSet(filePath);
            return _rules;
        }

        public string GetUserData(ADUser user) =>
            $"UserName={user.UserID};Name={user.Name};Address={user.Address};City={user.Location};State={user.State};Country={user.Country};Department={user.Department};Organization={user.BusinessGroup};Division={user.Division};Company={user.Company};Managers=#{string.Join("#", user.Managers.Select(m => m.UserID).ToArray())}#;UserID={user.UserID};";


        public string UserInfo => data.UserData[0];

        public string UserInfo2 => data.UserData[1];

        public string UserInfo3 => data.UserData[2];

    }
}
