using System.Collections.Generic;

namespace LdapUtilLib
{
    public record ADGroup
    {
        public ADGroup()
        {
            this.Members = new List<ADUser>();
            this.NestedGroupDNs = new List<string>();
        }

        public string GroupID { get; set; } = null!;
        public string DN { get; set; } = null!;
        public string Description { get; set; } = "";
        public List<ADUser> Members { get; set; }
        public List<string> NestedGroupDNs { get; set; }
    }
}
