using System.Collections.Generic;
using System.Xml.Serialization;

namespace ClassifierLib.Entities
{
    /// <summary>
    /// Define a series of match patterns
    /// for the input data.
    /// </summary>
    public class Rule
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        /// <summary>
        /// If true, all RuleMatch must match.
        /// Otherwise, only one RuleMatch is suffice.
        /// </summary>
        public bool IsMatchAll { get; set; }
        public List<RuleMatch> Matches { get; set; } = [];
    }
}
