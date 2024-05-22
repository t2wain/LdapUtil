using System.Collections.Generic;
using System.Xml.Serialization;

namespace ClassifierLib.Entities
{
    /// <summary>
    /// Define a series of match patterns
    /// for the input data.
    /// </summary>
    public class RuleMatch
    {
        public string Name { get; set; } = null!;
        /// <summary>
        /// If true, all Exclude patterns must match.
        /// Otherwise, only one match is suffice.
        /// </summary>
        public bool IsMatchAllExclExpr { get; set; }
        /// <summary>
        /// If true, all Include patterns must match.
        /// Otherwise, only one match is suffice.
        /// </summary>
        public bool IsMatchAllInclExpr { get; set; }

        /// <summary>
        /// The match is successfull if data 
        /// matches one of these Include patterns
        /// </summary>
        [XmlArrayItem(ElementName = "RegExpr")]
        public List<string> Includes { get; set; } = [];

        /// <summary>
        /// The match is un-successfull if data 
        /// matches one of these Exclude patterns.
        /// Note, an Include successful match 
        /// must exist before data is test
        /// against the Exclude patterns.
        /// </summary>
        [XmlArrayItem(ElementName = "RegExpr")]
        public List<string> Excludes { get; set; } = [];
    }
}
