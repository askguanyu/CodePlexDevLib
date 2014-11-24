//-----------------------------------------------------------------------
// <copyright file="LdapEntry.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.DirectoryServices
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.DirectoryServices;
    using System.Security.Permissions;

    /// <summary>
    /// Provides methods for interacting with the Active Directory Domain Services.
    /// </summary>
    [Serializable]
    public class LdapEntry : MarshalByRefObject
    {
        /// <summary>
        /// Field _ldapPath.
        /// </summary>
        private readonly string _ldapPath;

        /// <summary>
        /// Field _username.
        /// </summary>
        private readonly string _username;

        /// <summary>
        /// Field _password.
        /// </summary>
        private readonly string _password;

        /// <summary>
        /// Field _authenticationType.
        /// </summary>
        private readonly AuthenticationTypes? _authenticationType;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapEntry" /> class.
        /// The calling process is assumed to have rights to read from and write to the directory.
        /// </summary>
        /// <param name="ldapPath">The path at which to bind the System.DirectoryServices.DirectoryEntry to the directory.</param>
        public LdapEntry(string ldapPath)
        {
            this._ldapPath = ldapPath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapEntry" /> class.
        /// </summary>
        /// <param name="ldapPath">The path at which to bind the System.DirectoryServices.DirectoryEntry to the directory.</param>
        /// <param name="username">The user name to use when authenticating the client.</param>
        /// <param name="password">The user password.</param>
        public LdapEntry(string ldapPath, string username, string password)
        {
            this._ldapPath = ldapPath;
            this._username = username;
            this._password = password;
            this._authenticationType = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapEntry" /> class.
        /// </summary>
        /// <param name="ldapPath">The path at which to bind the System.DirectoryServices.DirectoryEntry to the directory.</param>
        /// <param name="username">The user name to use when authenticating the client.</param>
        /// <param name="password">The user password.</param>
        /// <param name="authenticationType">One of the System.DirectoryServices.AuthenticationTypes values.</param>
        public LdapEntry(string ldapPath, string username, string password, AuthenticationTypes authenticationType)
        {
            this._ldapPath = ldapPath;
            this._username = username;
            this._password = password;
            this._authenticationType = authenticationType;
        }

        /// <summary>
        /// Authenticates a user against a directory.
        /// </summary>
        /// <param name="userName">User account to authenticate.</param>
        /// <param name="password">User password.</param>
        /// <param name="getAdditionalInfo">Indicates additional account information should be retrieved upon a successful authentication.</param>
        /// <returns>LdapResult instance.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public LdapResult Authenticate(string userName, string password, bool getAdditionalInfo = false)
        {
            LdapResult result = new LdapResult();

            DirectoryEntry directoryEntry = null;
            DirectorySearcher directorySearcher = null;

            try
            {
                directoryEntry = new DirectoryEntry(this._ldapPath, userName, password);
                object directoryObject = directoryEntry.NativeObject;

                directorySearcher = new DirectorySearcher(directoryEntry);
                directorySearcher.Filter = "(SAMAccountName=" + userName + ")";
                directorySearcher.PropertiesToLoad.Add("cn");

                if (getAdditionalInfo)
                {
                    directorySearcher.PropertiesToLoad.Add("MemberOf");
                    directorySearcher.PropertiesToLoad.Add("mail");
                    directorySearcher.PropertiesToLoad.Add("department");
                    directorySearcher.PropertiesToLoad.Add("telephoneNumber");
                }

                SearchResult searchResult = directorySearcher.FindOne();

                if (searchResult == null)
                {
                    result.Succeeded = false;
                    result.Message = "Authentication failed.";
                    result.Source = "DevLib.DirectoryServices.LdapEntry.Authenticate";
                }
                else
                {
                    result.User = new LdapUser();
                    result.User.DisplayName = searchResult.Properties["cn"][0].ToString();

                    if (getAdditionalInfo)
                    {
                        if (searchResult.Properties["mail"].Count > 0)
                        {
                            result.User.EmailAddress = searchResult.Properties["mail"][0].ToString();
                        }

                        if (searchResult.Properties["department"].Count > 0)
                        {
                            result.User.Department = searchResult.Properties["department"][0].ToString();
                        }

                        if (searchResult.Properties["telephoneNumber"].Count > 0)
                        {
                            result.User.PhoneNumber = searchResult.Properties["telephoneNumber"][0].ToString();
                        }

                        result.User.MailingAddress = this.GetUserAddress(userName);

                        List<string> groups = new List<string>();

                        if (searchResult.Properties["MemberOf"] != null)
                        {
                            foreach (object obj in searchResult.Properties["MemberOf"])
                            {
                                groups.Add(this.TrimToName((string)obj));
                            }
                        }

                        result.User.Groups = groups;
                    }
                }

                result.Succeeded = true;

                return result;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                result.Succeeded = false;
                result.Message = e.ToString().Trim();
                result.Source = e.Source.Trim();

                return result;
            }
            finally
            {
                if (directoryEntry != null)
                {
                    directoryEntry.Dispose();
                    directoryEntry = null;
                }

                if (directorySearcher != null)
                {
                    directorySearcher.Dispose();
                    directorySearcher = null;
                }
            }
        }

        /// <summary>
        /// Gets UserGroup Membership.
        /// </summary>
        /// <param name="userName">User account to get.</param>
        /// <returns>UserGroup Membership list.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public List<string> GetUserGroupMembership(string userName)
        {
            List<string> result = null;

            string filter = "(&(objectCategory=person)(samAccountName=" + userName + "))";
            List<string> searchResults = this.Search(filter, "MemberOf");

            result = new List<string>();

            if (searchResults != null)
            {
                foreach (string searchResult in searchResults)
                {
                    result.Add(searchResult);

                    List<string> subGroups = this.GetGroupMembershipByGroup(searchResult);

                    foreach (string subGroup in subGroups)
                    {
                        if (!result.Contains(subGroup))
                        {
                            result.Add(subGroup);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets LocalGroup Membership.
        /// </summary>
        /// <param name="userName">User account to get.</param>
        /// <returns>LocalGroup Membership list.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public List<string> GetLocalGroupMembership(string userName)
        {
            using (DirectoryEntry directoryEntry = this.GetDirectoryEntry())
            {
                using (DirectoryEntry directoryEntryChildren = directoryEntry.Children.Find(userName))
                {
                    object groupsObject = directoryEntryChildren.Invoke("Groups");

                    List<string> result = new List<string>();

                    foreach (object adsObject in (IEnumerable)groupsObject)
                    {
                        using (DirectoryEntry directoryEntryChildrenGroup = new DirectoryEntry(adsObject))
                        {
                            result.Add(directoryEntryChildrenGroup.Name);
                        }
                    }

                    return result;
                }
            }
        }

        /// <summary>
        /// Gets Groups.
        /// </summary>
        /// <returns>Groups list.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public List<string> GetGroups()
        {
            string filter = "(&(objectClass=group))";

            List<string> result = this.Search(filter);

            return result;
        }

        /// <summary>
        /// Gets Users.
        /// </summary>
        /// <param name="filter">The search filter string.</param>
        /// <returns>LdapUserObject list.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public List<LdapUser> GetUsers(string filter = null)
        {
            using (DirectoryEntry directoryEntry = this.GetDirectoryEntry())
            {
                using (DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry))
                {
                    if (string.IsNullOrEmpty(filter))
                    {
                        directorySearcher.Filter = "(&(objectCategory=person))";
                    }
                    else
                    {
                        directorySearcher.Filter = filter;
                    }

                    directorySearcher.PropertiesToLoad.Add("samAccountName");
                    directorySearcher.PropertiesToLoad.Add("displayName");

                    using (SearchResultCollection searchResultCollection = directorySearcher.FindAll())
                    {
                        List<LdapUser> result = new List<LdapUser>();

                        foreach (SearchResult searchResult in searchResultCollection)
                        {
                            LdapUser ldapUserObject = new LdapUser();

                            ldapUserObject.UserName = searchResult.Properties["samAccountName"][0].ToString();

                            if (searchResult.Properties["displayName"].Count > 0)
                            {
                                ldapUserObject.DisplayName = searchResult.Properties["displayName"][0].ToString();
                            }

                            result.Add(ldapUserObject);
                        }

                        return result;
                    }
                }
            }
        }

        /// <summary>
        /// Gets UserAddress.
        /// </summary>
        /// <param name="userName">User account to get.</param>
        /// <returns>MailingAddress instance.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public MailingAddress GetUserAddress(string userName)
        {
            MailingAddress result = new MailingAddress();

            string streetAddress = this.GetUserProperty(userName, "streetAddress");
            int newLineIndex = streetAddress.IndexOf(Environment.NewLine);

            if (newLineIndex == -1)
            {
                result.StreetLine1 = streetAddress;
            }
            else
            {
                result.StreetLine1 = streetAddress.Substring(0, newLineIndex);
                result.StreetLine2 = streetAddress.Substring(newLineIndex + 2, streetAddress.Length - newLineIndex - 2);
            }

            result.POBox = this.GetUserProperty(userName, "postOfficeBox");
            result.City = this.GetUserProperty(userName, "l");
            result.State = this.GetUserProperty(userName, "st");
            result.PostalCode = this.GetUserProperty(userName, "postalCode");
            result.Country = this.GetUserProperty(userName, "c");

            return result;
        }

        /// <summary>
        /// Gets User EmailAddress.
        /// </summary>
        /// <param name="userName">User account to get.</param>
        /// <returns>User EmailAddress string.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public string GetUserEmailAddress(string userName)
        {
            return this.GetUserProperty(userName, "mail");
        }

        /// <summary>
        /// Gets User Company.
        /// </summary>
        /// <param name="userName">User account to get.</param>
        /// <returns>User Company string.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public string GetUserCompany(string userName)
        {
            return this.GetUserProperty(userName, "company");
        }

        /// <summary>
        /// Gets User DisplayName.
        /// </summary>
        /// <param name="userName">User account to get.</param>
        /// <returns>User DisplayName string.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public string GetUserDisplayName(string userName)
        {
            return this.GetUserProperty(userName, "displayName");
        }

        /// <summary>
        /// Gets User Department.
        /// </summary>
        /// <param name="userName">User account to get.</param>
        /// <returns>User Department string.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public string GetUserDepartment(string userName)
        {
            return this.GetUserProperty(userName, "department");
        }

        /// <summary>
        /// Gets User PhoneNumber.
        /// </summary>
        /// <param name="userName">User account to get.</param>
        /// <returns>User PhoneNumber string.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public string GetUserPhoneNumber(string userName)
        {
            return this.GetUserProperty(userName, "telephoneNumber");
        }

        /// <summary>
        /// Gets Password last set time.
        /// </summary>
        /// <param name="userName">User account to get.</param>
        /// <returns>Password last set time.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public DateTime GetPasswordLastSetTime(string userName)
        {
            long fileTime = Convert.ToInt64(this.GetUserProperty(userName, "pwdLastSet"));

            return DateTime.FromFileTime(fileTime);
        }

        /// <summary>
        /// Determines whether the specified user account exists.
        /// </summary>
        /// <param name="userName">User account to check.</param>
        /// <returns>true if the user account exists; otherwise, false.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public bool ExistsUser(string userName)
        {
            string filter = "(&(objectCategory=person)(samAccountName=" + userName + "))";

            List<string> searchResult = this.Search(filter);

            if (searchResult.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets UserName By Email.
        /// </summary>
        /// <param name="emailAddress">User email address to get.</param>
        /// <returns>User email address string.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public string GetUserNameByEmail(string emailAddress)
        {
            string result = null;

            string filter = null;
            string emailUserName = null;
            List<string> searchResults = null;

            filter = "(&(objectCategory=person)(mail=" + emailAddress + "))";
            searchResults = this.Search(filter);

            if (searchResults.Count > 0)
            {
                result = searchResults[0];
            }

            emailUserName = emailAddress.Substring(0, emailAddress.IndexOf("@"));
            filter = "(&(objectCategory=person)(samAccountName=" + emailUserName + "))";
            searchResults = this.Search(filter);

            if (searchResults.Count > 0)
            {
                result = searchResults[0];
            }

            return result;
        }

        /// <summary>
        /// Change Password.
        /// </summary>
        /// <param name="userName">User account to set.</param>
        /// <param name="newPassword">New password to set.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public void ChangePassword(string userName, string newPassword)
        {
            using (DirectoryEntry directoryEntry = this.GetDirectoryEntryForUser(userName))
            {
                directoryEntry.Invoke("SetPassword", new object[] { newPassword });
                directoryEntry.Properties["lockoutTime"].Value = 0;
                directoryEntry.CommitChanges();
            }
        }

        /// <summary>
        /// Finds all active user accounts whose 'physicalDeliveryOfficeName' property is blank.
        /// </summary>
        /// <returns>LdapUserObject list.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public List<LdapUser> GetUsersWithMissingOffice()
        {
            return this.GetUsersWithMissingProperty("(&(objectCategory=person)(!physicalDeliveryOfficeName=*)(!userAccountControl:1.2.840.113556.1.4.803:=2))");
        }

        /// <summary>
        /// Finds all active user accounts whose 'telephoneNumber' property is blank.
        /// </summary>
        /// <returns>LdapUserObject list.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public List<LdapUser> GetUsersWithMissingPhoneNumber()
        {
            return this.GetUsersWithMissingProperty("(&(objectCategory=person)(!telephoneNumber=*)(!userAccountControl:1.2.840.113556.1.4.803:=2))");
        }

        /// <summary>
        /// Finds all active user accounts whose 'mail' property is blank.
        /// </summary>
        /// <returns>LdapUserObject list.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public List<LdapUser> GetUsersWithMissingEmailAddress()
        {
            return this.GetUsersWithMissingProperty("(&(objectCategory=person)(!mail=*)(!userAccountControl:1.2.840.113556.1.4.803:=2))");
        }

        /// <summary>
        /// Finds all active user accounts whose 'streetAddress' property is blank.
        /// </summary>
        /// <returns>LdapUserObject list.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public List<LdapUser> GetUsersWithMissingAddress()
        {
            return this.GetUsersWithMissingProperty("(&(objectCategory=person)(!streetAddress=*)(!userAccountControl:1.2.840.113556.1.4.803:=2))");
        }

        /// <summary>
        /// Finds all active user accounts whose 'department' property is blank.
        /// </summary>
        /// <returns>LdapUserObject list.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public List<LdapUser> GetUsersWithMissingDepartment()
        {
            return this.GetUsersWithMissingProperty("(&(objectCategory=person)(!department=*)(!userAccountControl:1.2.840.113556.1.4.803:=2))");
        }

        /// <summary>
        /// Returns a list of users and their password expiration date.
        /// </summary>
        /// <param name="filter">The search filter string.</param>
        /// <returns>LdapUserObject list.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public List<LdapUser> GetUsersPasswordExpiration(string filter = null)
        {
            using (DirectoryEntry directoryEntry = this.GetDirectoryEntry())
            {
                using (DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry))
                {
                    if (string.IsNullOrEmpty(filter))
                    {
                        directorySearcher.Filter = "(&(objectCategory=person))";
                    }
                    else
                    {
                        directorySearcher.Filter = filter;
                    }

                    directorySearcher.PropertiesToLoad.Add("samAccountName");
                    directorySearcher.PropertiesToLoad.Add("displayName");
                    directorySearcher.PropertiesToLoad.Add("pwdLastSet");

                    using (SearchResultCollection searchResultCollection = directorySearcher.FindAll())
                    {
                        List<LdapUser> result = new List<LdapUser>();

                        foreach (SearchResult searchResult in searchResultCollection)
                        {
                            LdapUser ldapUserObject = new LdapUser();
                            ldapUserObject.UserName = searchResult.Properties["samAccountName"][0].ToString();
                            ldapUserObject.DisplayName = searchResult.Properties["displayName"][0].ToString();
                            ldapUserObject.PasswordLastSetTime = DateTime.FromFileTime((long)searchResult.Properties["pwdLastSet"][0]);
                            result.Add(ldapUserObject);
                        }

                        return result;
                    }
                }
            }
        }

        /// <summary>
        /// Compares two specified LdapUserObject objects and returns an integer that indicates their relationship to one another in the sort order.
        /// </summary>
        /// <param name="x">The first LdapUserObject.</param>
        /// <param name="y">The second LdapUserObject.</param>
        /// <returns>
        /// A 32-bit signed integer indicating the lexical relationship between the two comparands.
        /// Value Condition Less than zero x.DisplayName is less than y.DisplayName.
        /// Zero x.DisplayName equals y.DisplayName.
        /// Greater than zero x.DisplayName is greater than y.DisplayName.
        /// </returns>
        public int CompareLdapUsers(LdapUser x, LdapUser y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {
                    return string.Compare(x.DisplayName, y.DisplayName);
                }
            }
        }

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns>An infinite lifetime.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// Gets GroupMembership By Group.
        /// </summary>
        /// <param name="groupName">Group name to get.</param>
        /// <returns>GroupMembership list.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        private List<string> GetGroupMembershipByGroup(string groupName)
        {
            string filter = "(&(objectCategory=group)(cn=" + groupName + "))";
            List<string> searchResults = this.Search(filter, "MemberOf");

            List<string> result = new List<string>();

            if (searchResults != null)
            {
                foreach (string searchResult in searchResults)
                {
                    if (searchResult == groupName)
                    {
                        continue;
                    }

                    result.Add(searchResult);

                    List<string> subGroups = this.GetGroupMembershipByGroup(searchResult);

                    foreach (string subGroup in subGroups)
                    {
                        if (!result.Contains(subGroup))
                        {
                            result.Add(subGroup);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets DirectoryEntry.
        /// </summary>
        /// <returns>DirectoryEntry instance.</returns>
        private DirectoryEntry GetDirectoryEntry()
        {
            if (string.IsNullOrEmpty(this._username))
            {
                return new DirectoryEntry(this._ldapPath);
            }
            else
            {
                if (this._authenticationType == null)
                {
                    return new DirectoryEntry(this._ldapPath, this._username, this._password);
                }
                else
                {
                    return new DirectoryEntry(this._ldapPath, this._username, this._password, this._authenticationType.Value);
                }
            }
        }

        /// <summary>
        /// Gets DirectoryEntry For User.
        /// </summary>
        /// <param name="userName">User account to get.</param>
        /// <returns>DirectoryEntry instance.</returns>
        private DirectoryEntry GetDirectoryEntryForUser(string userName)
        {
            using (DirectoryEntry directoryEntry = this.GetDirectoryEntry())
            {
                using (DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry))
                {
                    directorySearcher.Filter = "(&(objectCategory=person)(samAccountName=" + userName + "))";
                    directorySearcher.SearchScope = SearchScope.Subtree;

                    SearchResult result = directorySearcher.FindOne();

                    return result.GetDirectoryEntry();
                }
            }
        }

        /// <summary>
        /// Gets Users with missing property.
        /// </summary>
        /// <param name="filter">The search filter string.</param>
        /// <returns>LdapUserObject list.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        private List<LdapUser> GetUsersWithMissingProperty(string filter)
        {
            using (DirectoryEntry directoryEntry = this.GetDirectoryEntry())
            {
                using (DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry))
                {
                    directorySearcher.Filter = filter;
                    directorySearcher.PropertiesToLoad.Add("samAccountName");
                    directorySearcher.PropertiesToLoad.Add("cn");

                    using (SearchResultCollection searchResultCollection = directorySearcher.FindAll())
                    {
                        List<LdapUser> result = new List<LdapUser>();

                        if (searchResultCollection != null)
                        {
                            foreach (SearchResult searchResult in searchResultCollection)
                            {
                                LdapUser ldapUserObject = new LdapUser();
                                ldapUserObject.UserName = searchResult.Properties["samAccountName"][0].ToString();
                                ldapUserObject.DisplayName = searchResult.Properties["cn"][0].ToString();

                                result.Add(ldapUserObject);
                            }
                        }

                        return result;
                    }
                }
            }
        }

        /// <summary>
        /// Search by filter and propertyName.
        /// </summary>
        /// <param name="filter">The search filter string.</param>
        /// <param name="propertyName">The name of the property to retrieve.</param>
        /// <returns>Search result list.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        private List<string> Search(string filter, string propertyName = null)
        {
            using (DirectoryEntry directoryEntry = this.GetDirectoryEntry())
            {
                using (DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry))
                {
                    directorySearcher.Filter = filter;

                    using (SearchResultCollection searchResultCollection = directorySearcher.FindAll())
                    {
                        List<string> result = new List<string>();

                        foreach (SearchResult searchResult in searchResultCollection)
                        {
                            if (string.IsNullOrEmpty(propertyName))
                            {
                                result.Add(this.TrimToName(searchResult.GetDirectoryEntry().Name));
                            }
                            else
                            {
                                foreach (object resultProperty in searchResult.Properties[propertyName])
                                {
                                    result.Add(this.TrimToName((string)resultProperty));
                                }
                            }
                        }

                        return result;
                    }
                }
            }
        }

        /// <summary>
        /// Gets User Property.
        /// </summary>
        /// <param name="userName">User account to get.</param>
        /// <param name="propertyName">The name of the property to retrieve.</param>
        /// <returns>User Property string.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        private string GetUserProperty(string userName, string propertyName)
        {
            using (DirectoryEntry directoryEntry = this.GetDirectoryEntry())
            {
                using (DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry))
                {
                    directorySearcher.Filter = "(&(objectCategory=person)(samAccountName=" + userName + "))";
                    directorySearcher.SearchScope = SearchScope.Subtree;

                    SearchResult searchResult = directorySearcher.FindOne();

                    return this.TrimToName(searchResult.Properties[propertyName][0].ToString());
                }
            }
        }

        /// <summary>
        /// Trim To Name.
        /// </summary>
        /// <param name="path">Path to trim.</param>
        /// <returns>Name string.</returns>
        private string TrimToName(string path)
        {
            string[] parts = path.Split(',');

            return parts[0].Replace("CN=", string.Empty);
        }
    }
}
