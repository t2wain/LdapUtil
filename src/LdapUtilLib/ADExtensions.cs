using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text.RegularExpressions;

namespace LdapUtilLib
{

    public static class ADExtensions
    {

        #region Querying LDAP - User

        public static ADUser GetUserInfo(this LdapConfig cfg, string userId, 
            bool includeMemberOf = false, bool includeManager = false)
        {
            var lu = cfg.GetUserInfo(new string[] { userId }, includeMemberOf, includeManager);
            return lu.FirstOrDefault();
        }

        public static IEnumerable<ADUser> GetUserInfo(this LdapConfig cfg, IEnumerable<string> userIds, 
            bool includeMemberOf = false, bool includeManager = false)
        {
            using DirectoryEntry root = new DirectoryEntry(cfg.RootURL);
            using DirectorySearcher search = CreateSearcher(root);
            
            List<ADUser> users = new List<ADUser>();

            // list of properties to retrieve
            var lstProps = new List<string>();
            lstProps.AddRange(cfg.UserPropsDefault);
            if (includeMemberOf)
                lstProps.Add(cfg.pMemberOf);
            search.PropertiesToLoad.AddRange(lstProps.ToArray());


            foreach (var id in userIds)
            {
                // Search user
                string idTemp = id.Replace("!", "");
                SearchResult? result = null;
                if (!String.IsNullOrEmpty(id.Trim())) {
                    search.Filter = String.Format(cfg.UserQuery, idTemp);
                    result = search.FindOne();
                }

                var u = new ADUser { UserID = id.ToUpper() };
                // User found therefore populate user info
                if (result != null)
                {
                    u = cfg.CreateUserInfo(result.Properties);
                    u.UserID = id.ToUpper();
                }

                // load manager
                if (includeManager && !String.IsNullOrWhiteSpace(u.ManagerDN))
                {
                    // search for manager
                    search.Filter = String.Format(cfg.UserQuery2, u.ManagerDN);
                    result = search.FindOne();

                    if (result != null)
                    {
                        // found manager therefore populate user info
                        var m = cfg.CreateUserInfo(result.Properties);
                        m.UserID = m.UserID.ToUpper();
                        u.Managers.Add(m);
                    }
                }

                users.Add(u);
            }

            return users;
            
        }

        #endregion

        #region Querying LDAP - Manager

        /// <summary>
        /// Get the reporting manager hierarchy 
        /// of user up to specified level
        /// </summary>
        public static List<ADUser> GetManagers(this LdapConfig cfg, ADUser u, int levels)
        {
            cfg.LoadManagers(new ADUser[] { u }, levels);
            var lm = new List<ADUser>();
            lm.AddRange(u.Managers);
            return lm;
        }


        /// <summary>
        /// Get the reporting manager hierarchy 
        /// of user up to specified level. The result
        /// is appended to the user's properties.
        /// </summary>
        public static void LoadManagers(this LdapConfig cfg, IEnumerable<ADUser> users, int levels)
        {
            var du = users.ToDictionary(u => u.UserDN.ToLower());

            #region Setup query

            using DirectoryEntry root = new DirectoryEntry(cfg.RootURL);
            using DirectorySearcher search = CreateSearcher(root);

            // list of properties to retrieve
            var lstProps = new List<string>();
            lstProps.AddRange(cfg.UserPropsDefault);
            search.PropertiesToLoad.AddRange(lstProps.ToArray());

            #endregion

            foreach (var u in users)
            {
                var sm = new HashSet<string>();
                var lm = new List<ADUser>();

                // current user
                ADUser m = u; 

                #region Search manager reporting chain

                // search for reporting chain managers
                for (int l = 0; l < levels; l++)
                {
                    // current user's manager
                    var mgrDn = m.ManagerDN;
                    var dnKey = mgrDn.ToLower();

                    if (String.IsNullOrWhiteSpace(mgrDn) || sm.Contains(dnKey))
                        // either user has no manager
                        // or the manager is already
                        // in the reporting chain
                        break;
                    else if (du.ContainsKey(dnKey))
                    {
                        // the managers UserInfo
                        // already retrieved previously
                        m = du[dnKey];

                        // keep track of reporting chain
                        lm.Add(m); 
                        sm.Add(dnKey);
                    }
                    else
                    {
                        // retrieve info for the manager
                        search.Filter = String.Format(cfg.UserQuery2, mgrDn);
                        var result = search.FindOne();

                        // cannot find the manager, therefore
                        // stop searching for upper managers
                        if (result == null)
                            break;

                        // populate properties for the manager
                        m = cfg.CreateUserInfo(result.Properties);
                        m.UserID = m.UserID.ToUpper();

                        // keep track of reporting chain
                        lm.Add(m);
                        sm.Add(dnKey);
                        du.Add(dnKey, m);
                    }

                    // this manager is self reporting, therefore
                    // stop searching for upper managers
                    if (String.Compare(mgrDn, m.ManagerDN, false) == 0)
                        break;

                }

                #endregion

                u.Managers.Clear();
                u.Managers.AddRange(lm);
            }
            
        }

        #endregion

        #region Querying LDAP - Group

        /// <summary>
        /// Get AD group info including all nested groups
        /// </summary>
        public static ADGroup GetGroupInfo(this LdapConfig cfg, string groupId, 
            bool includeMember = true, bool loadMember = false)
        {
            var qNestedGroups = new Queue<string>();
            var hSearchedMemberDNs = new HashSet<string>();
            
            #region Setup query

            using DirectoryEntry root = new DirectoryEntry(cfg.RootURL);
            using DirectorySearcher searchGroup = CreateSearcher(root);
            using DirectorySearcher searchUser = CreateSearcher(root);

            // properties of group and user to retrieve
            var groupProp = cfg.GroupPropsDefault;
            if (!includeMember)
                groupProp = cfg.GroupPropsWithoutMembersDefault;
            searchGroup.PropertiesToLoad.AddRange(groupProp.ToArray());
            searchUser.PropertiesToLoad.AddRange(cfg.UserPropsDefault.ToArray());

            #endregion

            #region Find parent group

            var g = new ADGroup { GroupID = groupId };
            SearchResult? groupResult = null;
            if (!String.IsNullOrEmpty(groupId.Trim()))
            {
                // start search
                searchGroup.Filter = String.Format(cfg.UserGroupQuery, groupId);
                groupResult = searchGroup.FindOne();
            }

            // group found
            if (groupResult != null)
                g = cfg.CreateGroupInfo(groupResult.Properties);

            #endregion

            var i = 0;
            // loop through the current group
            // and its nested groups if found
            while (groupResult != null && ++i < 100)
            {
                #region Populate group members

                if (groupResult.Properties.Contains(cfg.pMember))
                {
                    // loop through each member of the group
                    foreach (string mDN in groupResult.Properties[cfg.pMember])
                    {
                        if (hSearchedMemberDNs.Contains(mDN))
                            continue; // member already searched before

                        if (!loadMember)
                        {
                            g.Members.Add(new ADUser() { UserDN = mDN });
                        }
                        else
                        {
                            #region Query user info

                            // first, search member as a user
                            searchUser.Filter = String.Format(cfg.UserQuery2, mDN);
                            var userResult = searchUser.FindOne();
                            // tracking search result
                            hSearchedMemberDNs.Add(mDN);
                            if (userResult == null)
                            {
                                // user not found, therefore, it might be a nested group
                                // save the group for later query
                                qNestedGroups.Enqueue(mDN);
                                continue;
                            }
                            else
                            {
                                // found user
                                g.Members.Add(cfg.CreateUserInfo(userResult.Properties));
                            }

                            #endregion
                        }

                    }
                }

                #endregion

                #region Find nested groups

                groupResult = null;
                while (qNestedGroups.Count > 0)
                {
                    var groupDN = qNestedGroups.Dequeue();
                    // search group by DN
                    searchGroup.Filter = String.Format(cfg.UserGroupQuery2, groupDN);
                    groupResult = searchGroup.FindOne();
                    if (groupResult != null)
                    {
                        // found valid nested group, process member next
                        g.NestedGroupDNs.Add(groupDN);
                        break;
                    }
                }

                #endregion
            }

            return g;
            
        }


        /// <summary>
        /// Check if user is a member of the AD group
        /// </summary>
        public static bool IsMember(this LdapConfig cfg, string userId, string groupId)
        {
            bool isFound = false;
            using DirectoryEntry root = new DirectoryEntry(cfg.RootURL);
            using DirectorySearcher searchUser = CreateSearcher(root);
            
            searchUser.PropertiesToLoad.AddRange(new string[] { cfg.pDN });

            var g = cfg.GetGroupInfo(groupId, false);
            if (!String.IsNullOrWhiteSpace(g.DN))
            {
                searchUser.Filter = String.Format(cfg.IsMemberQuery, userId, g.DN);
                var userResult = searchUser.FindOne();
                if (userResult != null)
                {
                    isFound = true;
                    var u = cfg.CreateUserInfo(userResult.Properties);
                }
            }

            return isFound;
        }

        static DirectorySearcher CreateSearcher(DirectoryEntry root)
        {
            var search = new DirectorySearcher(root);
            search.SizeLimit = 1;
            search.ClientTimeout = new TimeSpan(0, 0, 5);
            search.CacheResults = false;
            return search;
        }

        #endregion

        #region Authentication

        public static bool Authenticate(string userName, string password, string domain)
        {
            bool result = false;
            try
            {
                DirectoryEntry entry = new DirectoryEntry($"LDAP://{domain}", userName, password);
                object nativeObject = entry.NativeObject;
                result = true;
            }
            catch (DirectoryServicesCOMException) { }
            return result;
        }

        #endregion

        #region Reading data

        static ADUser CreateUserInfo(this LdapConfig cfg, ResultPropertyCollection props)
        {
            var user = new ADUser();

            foreach (string pn in props.PropertyNames)
            {
                if (String.Compare(pn, cfg.pMemberOf, true) == 0)
                {
                    foreach (var m in props[cfg.pMemberOf])
                    {
                        user.MemberOfDN.Add(m.ToString());
                        var rm = Regex.Match(m.ToString(), "^CN=([^,]+),", RegexOptions.IgnoreCase);
                        if (rm.Success)
                            user.MemberOf.Add(rm.Groups[1].Value);
                    }
                    continue;
                }

                var v = props[pn][0].ToString();
                if (String.Compare(pn, cfg.pId, true) == 0)
                {
                    user.UserID = v.ToUpper();
                    user.LogonUserID = v.ToUpper();
                }
                else if (String.Compare(pn, cfg.pName, true) == 0)
                    user.Name = v;
                else if (String.Compare(pn, cfg.pFName, true) == 0)
                    user.FirstName = v;
                else if (String.Compare(pn, cfg.pLName, true) == 0)
                    user.LastName = v;
                else if (String.Compare(pn, cfg.pLoc, true) == 0)
                    user.Location = v;
                else if (String.Compare(pn, cfg.pState, true) == 0)
                    user.State = v;
                else if (String.Compare(pn, cfg.pCountry, true) == 0)
                    user.Country = v;
                else if (String.Compare(pn, cfg.pAddr, true) == 0)
                    user.Address = v;
                else if (String.Compare(pn, cfg.pEmail, true) == 0)
                    user.Email = v;
                else if (String.Compare(pn, cfg.pPhone, true) == 0)
                    user.Phone = v;
                else if (String.Compare(pn, cfg.pDN, true) == 0)
                {
                    user.URL = v;
                    user.UserDN = v;
                }
                else if (String.Compare(pn, cfg.pCompany, true) == 0)
                    user.Company = v;
                else if (String.Compare(pn, cfg.pBusGroup, true) == 0)
                    user.BusinessGroup = v;
                else if (String.Compare(pn, cfg.pDepartment, true) == 0)
                    user.Department = v;
                else if (String.Compare(pn, cfg.pManager, true) == 0)
                    user.ManagerDN = v;
                else if (String.Compare(pn, cfg.pDivision, true) == 0)
                    user.Division = v;
                else if (String.Compare(pn, cfg.pMailNickname, true) == 0)
                    user.MailNickname = v.Trim().ToUpper();
            }

            if (!String.IsNullOrWhiteSpace(user.MailNickname) && user.MailNickname.Length < user.UserID.Length)
            {
                user.UserID = user.MailNickname;
                user.LogonUserID = user.MailNickname;
            }

            return user;
        }

        static ADGroup CreateGroupInfo(this LdapConfig cfg, ResultPropertyCollection props)
        {
            var group = new ADGroup();

            foreach (string pn in props.PropertyNames)
            {
                var v = props[pn][0].ToString();
                if (String.Compare(pn, cfg.pId, true) == 0)
                    group.GroupID = v;
                else if (String.Compare(pn, cfg.pDescription, true) == 0)
                    group.Description = v;
                else if (String.Compare(pn, cfg.pDN, true) == 0)
                    group.DN = v;
            }
            return group;
        }

        #endregion
    }

}
