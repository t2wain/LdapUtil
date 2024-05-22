using System;
using System.Collections.Generic;
using System.Linq;

namespace LdapUtilLib
{
    public record ADUser
    {
        public ADUser()
        {
            this.MemberOf = new List<string>();
            this.MemberOfDN = new List<string>();
            this.Managers = new List<ADUser>();
        }

        public string UserID { get; set; } = "";
        public string LogonUserID { get; set; } = "";
        public string Name { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Location { get; set; } = "";
        public string State { get; set; } = "";
        public string Country { get; set; } = "";
        public string Address { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string URL { get; set; } = "";
        public string Company { get; set; } = "";
        public string BusinessGroup { get; set; } = "";
        public string Department { get; set; } = "";
        public string Division { get; set; } = "";
        public string UserDN { get; set; } = "";
        public string MailNickname { get; set; } = "";
        public List<string> MemberOf { get; set; }
        public List<string> MemberOfDN { get; set; }

        public string ManagerDN { get; set; } = "";
        public List<ADUser> Managers { get; set; }


        public bool IsMemberOf(string groupId)
        {
            return this.MemberOf.Any(m => String.Compare(m, groupId, true) == 0);
        }
        public bool IsMemberOf(IEnumerable<string> groupIds)
        {
            return groupIds.Any(g => this.MemberOf.Any(m => String.Compare(m, g, true) == 0) );
        }
    }
}
