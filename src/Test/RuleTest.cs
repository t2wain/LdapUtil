using ClassifierLib;

namespace Test
{
    public class RuleTest : IClassFixture<Context>
    {
        private readonly Context _ctx;

        public RuleTest(Context ctx)
        {
            this._ctx = ctx;
        }

        [Fact]
        public void ExportRuleToXml()
        {
            var rules = _ctx.ReadRule();
            var res = RuleExtensions.SaveRuleSet(rules);
        }

        [Fact]
        public void ReadRuleXml()
        {
            var rules = _ctx.ReadRule();
            Assert.True(rules.Count() > 0);
        }

        [Fact]
        public void EvaluateRule()
        {
            var rules = _ctx.ReadRule();
            var res1 = rules!.Evaluate(_ctx.UserInfo);
            var res2 = rules!.Evaluate(_ctx.UserInfo2);
            var res3 = rules!.Evaluate(_ctx.UserInfo3);
        }
    }
}