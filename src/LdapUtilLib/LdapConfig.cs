using System.Collections.Generic;
using System.Linq;

namespace LdapUtilLib
{
    public record LdapConfig
    {
        public string RootURL { get; set; } = null!;

        #region Queries

        public string UserQuery { get; set; } = "(&(objectClass=user)(|(cn={0})(sAMAccountName={0})(mailNickname={0})))";
        public string UserQuery2 { get; set; } = "(&(objectClass=user)(distinguishedName={0}))";
        public string UserGroupQuery { get; set; } = "(&(objectClass=group) (|(cn={0})(sAMAccountName={0})))";
        public string UserGroupQuery2 { get; set; } = "(&(objectClass=group) (distinguishedName={0}))";
        public string ComputerQuery { get; set; } = "(&(objectClass=computer) (|(cn={0})(dn={0})))";
        public string IsMemberQuery { get; set; } = "(&(objectClass=user)(sAMAccountName={0})(memberof={1}))";

        #endregion

        #region Properties

        public string pId { get; set; } = "sAMAccountName";
        public string pName { get; set; } = "displayName";
        public string pFName { get; set; } = "givenName";
        public string pLName { get; set; } = "sn";
        public string pLoc { get; set; } = "l";
        public string pState { get; set; } = "st";
        public string pCountry { get; set; } = "co";
        public string pAddr { get; set; } = "streetAddress";
        public string pEmail { get; set; } = "mail";
        public string pPhone { get; set; } = "telephoneNumber";
        public string pDN { get; set; } = "distinguishedName";
        public string pCompany { get; set; } = "company";
        public string pBusGroup { get; set; } = "extensionAttribute11";
        public string pDepartment { get; set; } = "department";
        public string pMember {get; set; }= "member";
        public string pDescription {get; set; }= "description";
        public string pMemberOf {get; set; }= "memberOf";
        public string pManager {get; set; }= "manager";
        public string pDivision {get; set; }= "division";
        public string pTitle {get; set; }= "title";
        public string pDirectReports {get; set; }= "directReports";
        public string pMailNickname { get; set; }  = "mailNickname";

        #endregion

        #region Configured List of Properties

        public IEnumerable<string> UserProps { get; set; } = [];
        public IEnumerable<string> GroupProps { get; set; } = [];
        public IEnumerable<string> GroupPropsWithoutMembers { get; set; } = [];

        #endregion

        #region Default properties if not configured

        /// <summary>
        /// Retrieve the following properties for user
        /// </summary>
        public IEnumerable<string> UserPropsDefault => (UserProps.Count() > 0 ? UserProps : [pId,
            pName,
            pFName,
            pLName,
            pLoc,
            pState,
            pCountry,
            pAddr,
            pEmail,
            pPhone,
            pDN,
            pCompany,
            pBusGroup,
            pDepartment,
            pManager,
            pDN,
            pDivision,
            pMailNickname]).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();


        /// <summary>
        /// Retrieve the following properties for group
        /// </summary>
        public IEnumerable<string> GroupPropsDefault => (GroupProps.Count() > 0 ? 
            GroupProps : [pId, pMember, pDescription, pDN])
            .Where(p => !string.IsNullOrWhiteSpace(p)).ToList();

        /// <summary>
        /// Retrieve the following properties for group, without the member info
        /// </summary>
        public IEnumerable<string> GroupPropsWithoutMembersDefault => 
            (GroupPropsWithoutMembers.Count() > 0 ? GroupPropsWithoutMembers : [ pId, pDescription, pDN ])
            .Where(p => !string.IsNullOrWhiteSpace(p)).ToList();

        #endregion

    }
}
