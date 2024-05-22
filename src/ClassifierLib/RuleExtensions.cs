using ClassifierLib.Entities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace ClassifierLib
{
    public static class RuleExtensions
    {
        #region MatchRes

        /// <summary>
        /// Rule match result
        /// </summary>
        public record MatchRes
        {
            /// <summary>
            /// Name of matching Rule
            /// </summary>
            public string? RuleName { get; set; }
            /// <summary>
            /// Name of matching RuleMatch in Rule
            /// </summary>
            public string? MatchName { get; set; }
            /// <summary>
            /// The matching pattern
            /// </summary>
            public string? InclExpr { get; set; }
            /// <summary>
            /// The matching pattern
            /// </summary>
            public string? ExclExpr { get; set; }
            /// <summary>
            /// Successful match status
            /// </summary>
            public bool IsMatch { get; set; }
        }

        #endregion

        /// <summary>
        /// Evaluate data on a Rule set and
        /// return the first matching Rule
        /// </summary>
        public static MatchRes Evaluate(this IEnumerable<Rule> rules, string data) =>
            rules
            .Select(r => r.Evaluate(data))
            .Where(r => r.IsMatch)
            .FirstOrDefault() ?? new();


        /// <summary>
        /// Evaluate all RuleMatch in the Rule
        /// and return a collection of result
        /// for each RuleMatch
        /// </summary>
        public static IEnumerable<MatchRes> EvaluateAll(this Rule rule, string data) =>
            rule.Matches
                .Select(m => m.Evaluate(data))
                .Where(r => !string.IsNullOrWhiteSpace(r.InclExpr))
                .ToList();


        /// <summary>
        /// Evaluate data on a Rule
        /// </summary>
        public static MatchRes Evaluate(this Rule rule, string data) =>
            rule.IsMatchAll switch
            {
                true => new() { 
                    // evaluate RuleMatch
                    IsMatch = rule.Matches.All(m => m.Evaluate(data).IsMatch) 
                },
                _ => rule.Matches
                        .Select(m => m.Evaluate(data)) // evaluate RuleMatch
                        .Where(r => r.IsMatch)
                        .Select(r => r with { RuleName = rule.Name })
                        .FirstOrDefault() ?? new()
            };


        /// <summary>
        /// Evaluate data on a RuleMatch
        /// </summary>
        static MatchRes Evaluate(this RuleMatch match, string data)
        {
            var res = new MatchRes();
            // find matching Include pattern
            var inc = match.Includes.EvaluateExpr(data, match.IsMatchAllInclExpr);
            res = res with { InclExpr = inc.Expr, IsMatch = inc.IsSuccess };
            if (res.IsMatch) {
                // if Include test is successful,
                // then find matching Exclude pattern
                var exc = match.Excludes.EvaluateExpr(data, match.IsMatchAllExclExpr);
                res = res with 
                { 
                    MatchName = match.Name, 
                    ExclExpr = exc.Expr,
                    // match is un-successful
                    // if an Exclude pattern match
                    IsMatch = !exc.IsSuccess    
                };
            }
            return res;
        }

        /// <summary>
        /// Evaluate data on a collection of matching patterns
        /// </summary>
        static (string? Expr, bool IsSuccess) EvaluateExpr(this IEnumerable<string> exprs, string data, bool matchAll)
        {
            var options = RegexOptions.IgnoreCase;

            if (matchAll) 
            {
                // test data on all match pattern
                // and get the first un-matched pattern
                var expr = exprs
                    .Where(e => !Regex.IsMatch(data, e, options))
                    .FirstOrDefault();
                return (
                    expr,
                    // un-sucessful if on pattern did not match
                    string.IsNullOrWhiteSpace(expr)
                );
            }
            else
            {
                // test data for the first successful match pattern
                var expr = exprs
                    .Where(e => Regex.IsMatch(data, e, options))
                    .FirstOrDefault();
                return (
                    expr, 
                    // successul if one match is found
                    !string.IsNullOrWhiteSpace(expr)
                );
            }
        }

        /// <summary>
        /// Parse an XML rule set
        /// </summary>
        public static IEnumerable<Rule> ReadRuleSet(string filePath)
        {
            var x = new XmlSerializer(typeof(List<Rule>), new XmlRootAttribute("Rules"));
            using StreamReader r = new StreamReader(filePath);
            var rs = x.Deserialize(r) as IEnumerable<Rule>;
            return rs ?? [];
        }


        /// <summary>
        /// Serialize a rule set to XML
        /// </summary>
        public static string SaveRuleSet(this IEnumerable<Rule> rules)
        {
            var x = new XmlSerializer(typeof(List<Rule>), new XmlRootAttribute("Rules"));
            var s = new StringWriter();
            x.Serialize(s, rules.ToList());
            return s.ToString();
        }

    }
}
