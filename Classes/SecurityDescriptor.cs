using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Security.Principal;

namespace IMP.Security
{
    #region public enums declarations
    [Flags]
    internal enum AceRights
    {
        None = 0x00000000,
        GenericAll = 0x00000001,
        GenericRead = 0x00000002,
        GenericWrite = 0x00000004,
        GenericExecute = 0x00000008,
        StandardReadControl = 0x00000010,
        StandardDelete = 0x00000020,
        StandardWriteDAC = 0x00000040,
        StandardWriteOwner = 0x00000080,
        DirectoryReadProperty = 0x00000100,
        DirectoryWriteProperty = 0x00000200,
        DirectoryCreateChild = 0x00000400,
        DirectoryDeleteChild = 0x00000800,
        DirectoryListChildren = 0x00001000,
        DirectorySelfWrite = 0x00002000,
        DirectoryListObject = 0x00004000,
        DirectoryDeleteTree = 0x00008000,
        DirectoryControlAccess = 0x00010000,
        FileAll = 0x00020000,
        FileRead = 0x00040000,
        FileWrite = 0x00080000,
        FileExecute = 0x00100000,
        KeyAll = 0x00200000,
        KeyRead = 0x00400000,
        KeyWrite = 0x00800000,
        KeyExecute = 0x01000000
    }

    internal enum AceType
    {
        AccessAllowed = 0,
        AccessDenied,
        ObjectAccessAllowed,
        ObjectAccessDenied,
        Audit,
        Alarm,
        ObjectAudit,
        ObjectAlarm
    }

    [Flags]
    internal enum AceFlags
    {
        None = 0x0000,
        ContainerInherit = 0x0001,
        ObjectInherit = 0x0002,
        NoPropogate = 0x0004,
        InheritOnly = 0x0008,
        Inherited = 0x0010,
        AuditSuccess = 0x0020,
        AuditFailure = 0x0040
    }

    /// <summary>
    /// Access Control List Flags
    /// </summary>
    [Flags]
    internal enum AclFlags
    {
        None = 0x00,
        Protected = 0x01,
        MustInherit = 0x02,
        Inherited = 0x04
    }
    #endregion

    /// <summary>
    /// Security Descriptor
    /// </summary>
    /// <remarks>The Security Descriptor is the top level of the Access 
    /// Control API. It represents all the Access Control data that is 
    /// associated with the secured object.</remarks>
    internal class SecurityDescriptor
    {
        #region constants
        /// <summary>
        /// Regular Expression used to parse SDDL strings
        /// </summary>
        private const string cSddlExpr = @"^(O:(?'owner'[A-Z]+?|S(-[0-9]+)+)?)?(G:(?'group'[A-Z]+?|S(-[0-9]+)+)?)?(D:(?'dacl'[A-Z]*(\([^\)]*\))*))?(S:(?'sacl'[A-Z]*(\([^\)]*\))*))?$";
        #endregion

        #region member varible and default property initialization
        private SecurityIdentity ownerSid;
        private SecurityIdentity groupSid;
        private AccessControlList dacl;
        private AccessControlList sacl;
        #endregion

        #region constructors and destructors
        /// <summary>
        /// Creates a blank Security Descriptor
        /// </summary>
        public SecurityDescriptor() { }

        /// <summary>
        /// Creates a Security Descriptor from an SDDL string
        /// </summary>
        /// <param name="sddl">The SDDL string that represents the Security Descriptor</param>
        /// <exception cref="System.FormatException">
        /// Invalid SDDL String Format
        /// </exception>
        public SecurityDescriptor(string sddl)
        {
            Regex sddlRegex = new Regex(cSddlExpr, RegexOptions.IgnoreCase);

            Match m = sddlRegex.Match(sddl);
            if (!m.Success)
            {
                throw new FormatException("Invalid SDDL String Format");
            }

            if (m.Groups["owner"] != null && m.Groups["owner"].Success && !String.IsNullOrEmpty(m.Groups["owner"].Value))
            {
                this.Owner = SecurityIdentity.SecurityIdentityFromSIDorAbbreviation(m.Groups["owner"].Value);
            }

            if (m.Groups["group"] != null && m.Groups["group"].Success && !String.IsNullOrEmpty(m.Groups["group"].Value))
            {
                this.Group = SecurityIdentity.SecurityIdentityFromSIDorAbbreviation(m.Groups["group"].Value);
            }

            if (m.Groups["dacl"] != null && m.Groups["dacl"].Success && !String.IsNullOrEmpty(m.Groups["dacl"].Value))
            {
                this.DACL = new AccessControlList(m.Groups["dacl"].Value);
            }

            if (m.Groups["sacl"] != null && m.Groups["sacl"].Success && !String.IsNullOrEmpty(m.Groups["sacl"].Value))
            {
                this.SACL = new AccessControlList(m.Groups["sacl"].Value);
            }
        }
        #endregion

        #region action methods
        /// <summary>
        /// Renders the Security Descriptor as an SDDL string
        /// </summary>
        /// <remarks>For more info on SDDL see <a href="http://msdn.microsoft.com/en-us/library/aa379570(VS.85).aspx">MSDN: Security Descriptor String Format.</a></remarks>
        /// <returns>An SDDL string</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (this.ownerSid != null)
            {
                sb.AppendFormat("O:{0}", this.ownerSid.ToString());
            }

            if (this.groupSid != null)
            {
                sb.AppendFormat("G:{0}", this.groupSid.ToString());
            }

            if (this.dacl != null)
            {
                sb.AppendFormat("D:{0}", this.dacl.ToString());
            }

            if (this.sacl != null)
            {
                sb.AppendFormat("S:{0}", this.sacl.ToString());
            }

            return sb.ToString();
        }
        #endregion

        #region property getters/setters
        /// <summary>
        /// Gets or Sets the Owner
        /// </summary>
        public SecurityIdentity Owner
        {
            get { return this.ownerSid; }
            set { this.ownerSid = value; }
        }

        /// <summary>
        /// Gets or Sets the Group
        /// </summary>
        /// <remarks>Security Descriptor Groups are present for Posix compatibility reasons and are usually ignored.</remarks>
        public SecurityIdentity Group
        {
            get { return this.groupSid; }
            set { this.groupSid = value; }
        }

        /// <summary>
        /// Gets or Sets the DACL
        /// </summary>
        /// <remarks>The DACL (Discretionary Access Control List) is the 
        /// Access Control List that grants or denies various types of access 
        /// for different users and groups.</remarks>
        public AccessControlList DACL
        {
            get { return this.dacl; }
            set { this.dacl = value; }
        }

        /// <summary>
        /// Gets or Sets the SACL
        /// </summary>
        /// <remarks>The SACL (System Access Control List) is the Access 
        /// Control List that specifies what actions should be auditted</remarks>
        public AccessControlList SACL
        {
            get { return this.sacl; }
            set { this.sacl = value; }
        }
        #endregion
    }

    #region SecurityIdentity class
    /// <summary>
    /// Security Identity
    /// </summary>
    /// <remarks>The SecurityIdentity class is a read only representation of a 
    /// SID. The class has no public constructors, instead use the static 
    /// SecurityIdentityFrom* methods to instantiate it.</remarks>
    internal class SecurityIdentity
    {
        #region constants
        #region Well known SIDs
        /// <summary>
        /// Table of well known SID strings
        /// </summary>
        /// <remarks>The table indicies correspond to <see cref="WELL_KNOWN_SID_TYPE"/>s</remarks>
        private static readonly string[] wellKnownSids = new string[]
        {
            "S-1-0-0", // NULL SID
            "S-1-1-0", // Everyone
            "S-1-2-0", // LOCAL
            "S-1-3-0", // CREATOR OWNER
            "S-1-3-1", // CREATOR GROUP
            "S-1-3-2", // CREATOR OWNER SERVER
            "S-1-3-3", // CREATOR GROUP SERVER
            "S-1-5", // NT Pseudo Domain\NT Pseudo Domain
            "S-1-5-1", // NT AUTHORITY\DIALUP
            "S-1-5-2", // NT AUTHORITY\NETWORK
            "S-1-5-3", // NT AUTHORITY\BATCH
            "S-1-5-4", // NT AUTHORITY\INTERACTIVE
            "S-1-5-6", // NT AUTHORITY\SERVICE
            "S-1-5-7", // NT AUTHORITY\ANONYMOUS LOGON
            "S-1-5-8", // NT AUTHORITY\PROXY
            "S-1-5-9", // NT AUTHORITY\ENTERPRISE DOMAIN CONTROLLERS
            "S-1-5-10", // NT AUTHORITY\SELF
            "S-1-5-11", // NT AUTHORITY\Authenticated Users
            "S-1-5-12", // NT AUTHORITY\RESTRICTED
            "S-1-5-13", // NT AUTHORITY\TERMINAL SERVER USER
            "S-1-5-14", // NT AUTHORITY\REMOTE INTERACTIVE LOGON
            "", // Unknown
            "S-1-5-18", // NT AUTHORITY\SYSTEM
            "S-1-5-19", // NT AUTHORITY\LOCAL SERVICE
            "S-1-5-20", // NT AUTHORITY\NETWORK SERVICE
            "S-1-5-32", // BUILTIN\BUILTIN
            "S-1-5-32-544", // BUILTIN\Administrators
            "S-1-5-32-545", // BUILTIN\Users
            "S-1-5-32-546", // BUILTIN\Guests
            "S-1-5-32-547", // BUILTIN\Power Users
            "S-1-5-32-548", // BUILTIN\Account Operators
            "S-1-5-32-549", // BUILTIN\System Operators
            "S-1-5-32-550", // BUILTIN\Print Operators
            "S-1-5-32-551", // BUILTIN\Backup Operators
            "S-1-5-32-552", // BUILTIN\Replicator
            "S-1-5-32-554", // BUILTIN\PreWindows2000CompatibleAccess
            "S-1-5-32-555", // BUILTIN\Remote Desktop Users
            "S-1-5-32-556", // BUILTIN\Network Configuration Operators
            "", // Unknown
            "", // Unknown
            "", // Unknown
            "", // Unknown
            "", // Unknown
            "", // Unknown
            "", // Unknown
            "", // Unknown
            "", // Unknown
            "", // Unknown
            "", // Unknown
            "", // Unknown
            "", // Unknown
            "S-1-5-64-10", // NT AUTHORITY\NTLM Authentication
            "S-1-5-64-21", // NT AUTHORITY\Digest Authentication
            "S-1-5-64-14", // NT AUTHORITY\SChannel Authentication
            "S-1-5-15", // NT AUTHORITY\This Organization
            "S-1-5-1000", // NT AUTHORITY\Other Organization
            "S-1-5-32-557", // BUILTIN\Incoming Forest Trust Builders
            "S-1-5-32-558", // BUILTIN\Performance Monitor Users
            "S-1-5-32-559", // BUILTIN\Performance Log Users
            "S-1-5-32-560", // BUILTIN\Authorization Access
            "S-1-5-32-561", // BUILTIN\Terminal Server License Servers
            "S-1-5-32-562", // BUILTIN\Distributed COM Users
            "S-1-5-32-568", // BUILTIN\IIS_IUSRS
            "S-1-5-17", // NT AUTHORITY\IUSR
            "S-1-5-32-569", // BUILTIN\Cryptographic Operators
            "S-1-16-0", // Mandatory Label\Untrusted Mandatory Level
            "S-1-16-4096", // Mandatory Label\Low Mandatory Level
            "S-1-16-8192", // Mandatory Label\Medium Mandatory Level
            "S-1-16-12288", // Mandatory Label\High Mandatory Level
            "S-1-16-16384", // Mandatory Label\System Mandatory Level
            "S-1-5-33", // NT AUTHORITY\WRITE RESTRICTED
            "S-1-3-4", // OWNER RIGHTS
            "", // Unknown
            "", // Unknown
            "S-1-5-22", // NT AUTHORITY\ENTERPRISE READ-ONLY DOMAIN CONTROLLERS BETA
            "", // Unknown
            "S-1-5-32-573" // BUILTIN\Event Log Readers
        };
        #endregion

        #region Well known SID abbreviations
        /// <summary>
        /// Table of SDDL SID abbreviations
        /// </summary>
        /// <remarks>The table indicies correspond to <see cref="WELL_KNOWN_SID_TYPE"/>s</remarks>
        private static readonly string[] wellKnownSidAbbreviations = new string[] {
            "",
            "WD",
            "",
            "CO",
            "CG",
            "",
            "",
            "",
            "",
            "NU",
            "",
            "IU",
            "SU",
            "AN",
            "",
            "EC",
            "PS",
            "AU",
            "RC",
            "",
            "",
            "",
            "SY",
            "LS",
            "NS",
            "",
            "BA",
            "BU",
            "BG",
            "PU",
            "AO",
            "SO",
            "PO",
            "BO",
            "RE",
            "RU",
            "RD",
            "NO",
            "LA",
            "LG",
            "",
            "DA",
            "DU",
            "DG",
            "DC",
            "DD",
            "CA",
            "SA",
            "EA",
            "PA",
            "RS",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            ""
        };
        #endregion
        #endregion

        #region Win32 API
        [DllImport("Advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern bool ConvertStringSidToSid([MarshalAs(UnmanagedType.LPWStr)] string StringSid, out IntPtr Sid);

        [DllImport("Advapi32.dll")]
        private static extern bool ConvertSidToStringSid(IntPtr Sid, out IntPtr StringSid);

        [DllImport("Advapi32.dll")]
        private static extern uint GetSidLengthRequired(ushort nSubAuthorityCount);

        [DllImport("Advapi32.dll")]
        private static extern bool CreateWellKnownSid(int WellKnownSidType, IntPtr DomainSid, IntPtr pSid, ref uint cbSid);

        [DllImport("Advapi32.dll")]
        private static extern bool LookupAccountSid([MarshalAs(UnmanagedType.LPTStr)]string lpSystemName,
                                                     IntPtr lpSid,
                                                     IntPtr lpName,
                                                     ref uint cchName,
                                                     IntPtr lpReferencedDomainName,
                                                     ref uint cchReferencedDomainName,
                                                     out SID_NAME_USE peUse);

        [DllImport("Advapi32.dll")]
        private static extern int LsaOpenPolicy(IntPtr SystemName,
                                                ref LSA_OBJECT_ATTRIBUTES ObjectAttributes,
                                                ACCESS_MASK DesiredAccess,
                                                out IntPtr PolicyHandle);

        [DllImport("Advapi32.dll")]
        private static extern uint LsaNtStatusToWinError(int Status);

        [DllImport("Advapi32.dll")]
        private static extern int LsaLookupNames2(IntPtr PolicyHandle,
                                                   uint Flags,
                                                   uint Count,
                                                   LSA_UNICODE_STRING[] Names,
                                                   out IntPtr ReferencedDomains,
                                                   out IntPtr Sids);
        [DllImport("Advapi32.dll")]
        private static extern int LsaClose(IntPtr ObjectHandle);

        [DllImport("Advapi32.dll")]
        private static extern int LsaFreeMemory(IntPtr Buffer);

        [DllImport("Kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern UInt32 FormatMessage(UInt32 dwFlags, IntPtr lpSource, UInt32 dwMessageId, UInt32 dwLanguageId, [MarshalAs(UnmanagedType.LPTStr)] ref string lpBuffer, int nSize, IntPtr[] Arguments);

        private const ushort SID_MAX_SUB_AUTHORITIES = 15;

        private enum SID_NAME_USE
        {
            SidTypeUser = 1,
            SidTypeGroup,
            SidTypeDomain,
            SidTypeAlias,
            SidTypeWellKnownGroup,
            SidTypeDeletedAccount,
            SidTypeInvalid,
            SidTypeUnknown,
            SidTypeComputer
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct LSA_UNICODE_STRING
        {
            public ushort Length;
            public ushort MaxLength;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LSA_TRANSLATED_SID2
        {
            public SID_NAME_USE Use;
            public IntPtr Sid;
            public int DomainIndex;
            public uint Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LSA_OBJECT_ATTRIBUTES
        {
            public uint Length;
            public IntPtr RootDirectory;
            public IntPtr ObjectName;
            public uint Attributes;
            public IntPtr SecurityDescriptor;
            public IntPtr SecurityQualityOfService;
        }

        [Flags]
        private enum ACCESS_MASK
        {
            POLICY_VIEW_LOCAL_INFORMATION = 0x0001,
            POLICY_VIEW_AUDIT_INFORMATION = 0x0002,
            POLICY_GET_PRIVATE_INFORMATION = 0x0004,
            POLICY_TRUST_ADMIN = 0x0008,
            POLICY_CREATE_ACCOUNT = 0x0010,
            POLICY_CREATE_SECRET = 0x0020,
            POLICY_CREATE_PRIVILEGE = 0x0040,
            POLICY_SET_DEFAULT_QUOTA_LIMITS = 0x0080,
            POLICY_SET_AUDIT_REQUIREMENTS = 0x0100,
            POLICY_AUDIT_LOG_ADMIN = 0x0200,
            POLICY_SERVER_ADMIN = 0x0400,
            POLICY_LOOKUP_NAMES = 0x0800
        }
        #endregion

        #region member varible and default property initialization
        private string name;
        private string sid;
        private WellKnownSidType wellKnownSidType;
        #endregion

        #region constructors and destructors
        /// <summary>
        /// Creates Security Identity from a SID string or an object name
        /// </summary>
        /// <param name="value">A SID string (Format: S-1-1-...) or an object name (e.g. DOMAIN\AccountName)</param>
        public SecurityIdentity(string value)
        {
            this.wellKnownSidType = (WellKnownSidType) - 1;

            if (value.StartsWith("S-"))
            {
                //SID string
                SetFromSid(value);
            }
            else
            {
                //SID string
                SetFromName(value);
            }
        }

        /// <summary>
        /// Creates a Security Identity for a well known SID (such as LOCAL SYSTEM)
        /// </summary>
        /// <param name="sidType">The type of well known SID</param>
        public SecurityIdentity(WellKnownSidType sidType)
        {
            this.wellKnownSidType = (WellKnownSidType) - 1;

            SetFromWellKnownSidType(sidType);
        }
        #endregion

        #region action methods
        /// <summary>
        /// Creates a Security Identity from a SID string
        /// </summary>
        /// <param name="sid">A SID string (Format: S-1-1-...) or well known SID abbreviation (e.g. DA)</param>
        /// <returns>A populated Security Identity</returns>
        public static SecurityIdentity SecurityIdentityFromSIDorAbbreviation(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("sid");
            }

            if (value.Length == 0)
            {
                throw new ArgumentException("Argument 'value' cannot be the empty string.", "value");
            }

            if (!value.StartsWith("S-"))
            {
                // If the string is not a SID string (S-1-n-...) assume it is a SDDL abbreviation
                return new SecurityIdentity(SecurityIdentity.GetWellKnownSidTypeFromSddlAbbreviation(value));
            }

            return new SecurityIdentity(value);
        }

        /// <summary>
        /// Creates a Security Identity from an object name (e.g. DOMAIN\AccountName)
        /// </summary>
        /// <param name="name">A security object name (i.e. a Computer, Account, or Group)</param>
        /// <returns>A populated Security Identity</returns>
        public static SecurityIdentity SecurityIdentityFromName(string name)
        {
            return new SecurityIdentity(name);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is SecurityIdentity)
            {
                SecurityIdentity sd = (SecurityIdentity)obj;

                return (String.Compare(this.sid, sd.sid, true) == 0);
            }
            else return false;
        }

        public static bool operator ==(SecurityIdentity obj1, SecurityIdentity obj2)
        {
            if (Object.ReferenceEquals(obj1, null) && Object.ReferenceEquals(obj2, null)) return true;
            else if (Object.ReferenceEquals(obj1, null) || Object.ReferenceEquals(obj2, null)) return false;
            return obj1.Equals(obj2);
        }

        public static bool operator !=(SecurityIdentity obj1, SecurityIdentity obj2)
        {
            if (Object.ReferenceEquals(obj1, null) && Object.ReferenceEquals(obj2, null)) return false;
            else if (Object.ReferenceEquals(obj1, null) || Object.ReferenceEquals(obj2, null)) return true;
            return !obj1.Equals(obj2);
        }

        public override int GetHashCode()
        {
            return this.sid != null ? this.sid.GetHashCode() : base.GetHashCode();
        }

        /// <summary>
        /// Renders the Security Identity as a SDDL SID string or abbreviation
        /// </summary>
        /// <returns>An SDDL SID string or abbreviation</returns>
        public override string ToString()
        {
            if (this.WellKnownSidAbbreviations == null)
            {
                return this.sid;
            }
            else
            {
                return this.WellKnownSidAbbreviations;
            }
        }
        #endregion

        #region property getters/setters
        /// <summary>
        /// Gets the name of to security object represented by the SID
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Gets the SID string of the security object
        /// </summary>
        public string SID
        {
            get { return this.sid; }
        }

        /// <summary>
        /// Gets a value indicating whether or not the SID is a well known SID or not
        /// </summary>
        public bool IsWellKnownSid
        {
            get { return (int)this.wellKnownSidType != -1; }
        }

        /// <summary>
        /// Gets the SDDL abbreviation of well known SID
        /// </summary>
        public string WellKnownSidAbbreviations
        {
            get
            {
                if (this.IsWellKnownSid && !String.IsNullOrEmpty(SecurityIdentity.wellKnownSidAbbreviations[(int)this.wellKnownSidType]))
                {
                    return SecurityIdentity.wellKnownSidAbbreviations[(int)this.wellKnownSidType];
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the type of well known SID
        /// </summary>
        public WellKnownSidType WellKnownSidType
        {
            get { return this.wellKnownSidType; }
        }
        #endregion

        #region private member functions
        private void SetFromSid(string sid)
        {
            if (sid == null)
            {
                throw new ArgumentNullException("sid");
            }

            if (sid.Length == 0)
            {
                throw new ArgumentException("Argument 'sid' cannot be the empty string.", "sid");
            }

            this.sid = sid;

            // Check if the SID is a well known SID
            this.wellKnownSidType = (WellKnownSidType)Array.IndexOf<string>(SecurityIdentity.wellKnownSids, sid);

            IntPtr sidStruct;

            // Convert the SID string to a SID structure
            if (!ConvertStringSidToSid(sid, out sidStruct))
            {
                throw new ExternalException(String.Format("Error Converting SID String to SID Structur: {0}", GetErrorMessage(GetLastError())));
            }

            try
            {
                uint nameLen = 0;
                uint domainLen = 0;

                SID_NAME_USE nameUse;

                // Get the lengths of the object and domain names
                LookupAccountSid(null, sidStruct, IntPtr.Zero, ref nameLen, IntPtr.Zero, ref domainLen, out nameUse);

                if (nameLen != 0)
                {
                    IntPtr accountName = Marshal.AllocHGlobal((IntPtr)nameLen);
                    IntPtr domainName = domainLen > 0 ? Marshal.AllocHGlobal((IntPtr)domainLen) : IntPtr.Zero;

                    try
                    {
                        // Get the object and domain names
                        if (!SecurityIdentity.LookupAccountSid(null, sidStruct, accountName, ref nameLen, domainName, ref domainLen, out nameUse))
                        {
                            throw new ExternalException("Unable to Find SID");
                        }

                        // Marshal and store the object name
                        this.name = String.Format("{0}{1}{2}", domainLen > 1 ? Marshal.PtrToStringAnsi(domainName) : "", domainLen > 1 ? "\\" : "", Marshal.PtrToStringAnsi(accountName));
                    }
                    finally
                    {
                        if (accountName != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(accountName);
                        }
                        if (domainName != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(domainName);
                        }
                    }
                }
            }
            finally
            {
                if (sidStruct != IntPtr.Zero)
                {
                    LocalFree(sidStruct);
                }
            }
        }

        private void SetFromName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (name.Length == 0)
            {
                throw new ArgumentException("Argument 'name' cannot be the empty string.", "name");
            }

            LSA_OBJECT_ATTRIBUTES attribs = new LSA_OBJECT_ATTRIBUTES();
            attribs.Attributes = 0;
            attribs.ObjectName = IntPtr.Zero;
            attribs.RootDirectory = IntPtr.Zero;
            attribs.SecurityDescriptor = IntPtr.Zero;
            attribs.SecurityQualityOfService = IntPtr.Zero;
            attribs.Length = (uint)Marshal.SizeOf(attribs);

            IntPtr handle;

            int status = SecurityIdentity.LsaOpenPolicy(IntPtr.Zero, ref attribs, ACCESS_MASK.POLICY_LOOKUP_NAMES, out handle);

            if (status != 0)
            {
                throw new ExternalException("Unable to Find Object: " + GetErrorMessage(SecurityIdentity.LsaNtStatusToWinError(status)));
            }

            try
            {
                LSA_UNICODE_STRING nameString = new LSA_UNICODE_STRING();
                nameString.Buffer = name;
                nameString.Length = (ushort)(name.Length * UnicodeEncoding.CharSize);
                nameString.MaxLength = (ushort)(name.Length * UnicodeEncoding.CharSize + UnicodeEncoding.CharSize);

                IntPtr domains;
                IntPtr sids;

                status = SecurityIdentity.LsaLookupNames2(handle, 0, 1, new LSA_UNICODE_STRING[] { nameString }, out domains, out sids);

                if (status != 0)
                {
                    throw new ExternalException("Unable to Find Object: " + GetErrorMessage(SecurityIdentity.LsaNtStatusToWinError(status)));
                }

                try
                {
                    LSA_TRANSLATED_SID2 lsaSid = (LSA_TRANSLATED_SID2)Marshal.PtrToStructure(sids, typeof(LSA_TRANSLATED_SID2));

                    IntPtr sidStruct = lsaSid.Sid;

                    IntPtr sidString = IntPtr.Zero;

                    // Get the SID string
                    if (!SecurityIdentity.ConvertSidToStringSid(sidStruct, out sidString))
                    {
                        throw new ExternalException("Unable to Find Object: " + GetErrorMessage(GetLastError()));
                    }

                    try
                    {
                        // Marshal and store the SID string
                        this.sid = Marshal.PtrToStringAnsi(sidString);
                    }
                    finally
                    {
                        if (sidString != IntPtr.Zero)
                        {
                            LocalFree(sidString);
                        }
                    }

                    // Check if the SID is a well known SID
                    this.wellKnownSidType = (WellKnownSidType)Array.IndexOf<string>(SecurityIdentity.wellKnownSids, this.sid);

                    SID_NAME_USE nameUse;

                    uint nameLen = 0;
                    uint domainLen = 0;

                    // Get the lengths for the object and domain names
                    SecurityIdentity.LookupAccountSid(null, sidStruct, IntPtr.Zero, ref nameLen, IntPtr.Zero, ref domainLen, out nameUse);

                    if (nameLen == 0)
                    {
                        throw new ExternalException("Unable to Find SID: " + GetErrorMessage(GetLastError()));
                    }

                    IntPtr accountName = Marshal.AllocHGlobal((IntPtr)nameLen);
                    IntPtr domainName = domainLen > 0 ? Marshal.AllocHGlobal((IntPtr)domainLen) : IntPtr.Zero;

                    try
                    {
                        // Get the object and domain names
                        if (!SecurityIdentity.LookupAccountSid(null, sidStruct, accountName, ref nameLen, domainName, ref domainLen, out nameUse))
                        {
                            throw new ExternalException("Unable to Find SID: " + GetErrorMessage(GetLastError()));
                        }

                        // Marshal and store the object name
                        this.name = String.Format("{0}{1}{2}", domainLen > 1 ? Marshal.PtrToStringAnsi(domainName) : "", domainLen > 1 ? "\\" : "", Marshal.PtrToStringAnsi(accountName));
                    }
                    finally
                    {
                        if (accountName != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(accountName);
                        }
                        if (domainName != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(domainName);
                        }
                    }
                }
                finally
                {
                    if (domains != IntPtr.Zero)
                    {
                        SecurityIdentity.LsaFreeMemory(domains);
                    }
                    if (sids != IntPtr.Zero)
                    {
                        SecurityIdentity.LsaFreeMemory(sids);
                    }
                }
            }
            finally
            {
                if (handle != IntPtr.Zero)
                {
                    SecurityIdentity.LsaClose(handle);
                }
            }
        }

        private void SetFromWellKnownSidType(WellKnownSidType sidType)
        {
            if ((int)sidType == -1)
            {
                throw new ExternalException("Unable to Get Well Known SID");
            }

            this.wellKnownSidType = sidType;

            // Get the size required for the SID
            uint size = SecurityIdentity.GetSidLengthRequired(SecurityIdentity.SID_MAX_SUB_AUTHORITIES);

            IntPtr sidStruct = Marshal.AllocHGlobal((IntPtr)size);

            try
            {
                // Get the SID struct from the well known SID type
                if (!SecurityIdentity.CreateWellKnownSid((int)sidType, IntPtr.Zero, sidStruct, ref size))
                {
                    throw new ExternalException("Unable to Get Well Known SID");
                }

                IntPtr sidString = IntPtr.Zero;

                // Convert the SID structure to a SID string
                SecurityIdentity.ConvertSidToStringSid(sidStruct, out sidString);

                try
                {
                    // Marshal and store the SID string
                    this.sid = Marshal.PtrToStringAnsi(sidString);
                }
                finally
                {
                    if (sidString != IntPtr.Zero)
                    {
                        LocalFree(sidString);
                    }
                }

                uint nameLen = 0;
                uint domainLen = 0;

                SID_NAME_USE nameUse;

                // Get the lengths of the object and domain names
                SecurityIdentity.LookupAccountSid(null, sidStruct, IntPtr.Zero, ref nameLen, IntPtr.Zero, ref domainLen, out nameUse);

                if (nameLen != 0)
                {
                    IntPtr accountName = Marshal.AllocHGlobal((IntPtr)nameLen);
                    IntPtr domainName = domainLen > 0 ? Marshal.AllocHGlobal((IntPtr)domainLen) : IntPtr.Zero;

                    try
                    {
                        // Get the object and domain names
                        if (!SecurityIdentity.LookupAccountSid(null, sidStruct, accountName, ref nameLen, domainName, ref domainLen, out nameUse))
                        {
                            throw new ExternalException("Unable to Find SID");
                        }

                        // Marshal and store the object name
                        this.name = String.Format("{0}{1}{2}", domainLen > 1 ? Marshal.PtrToStringAnsi(domainName) : "", domainLen > 1 ? "\\" : "", Marshal.PtrToStringAnsi(accountName));
                    }
                    finally
                    {
                        if (accountName != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(accountName);
                        }
                        if (domainName != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(domainName);
                        }
                    }
                }
            }
            finally
            {
                if (sidStruct != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(sidStruct);
                }
            }
        }

        /// <summary>
        /// Gets the Well Known SID Type for an SDDL abbreviation
        /// </summary>
        /// <param name="abbreviation">The SDDL abbreviation</param>
        /// <returns>The Well Known SID Type that corresponds to the abbreviation</returns>
        private static WellKnownSidType GetWellKnownSidTypeFromSddlAbbreviation(string abbreviation)
        {
            if (abbreviation == null)
            {
                throw new ArgumentNullException("abbreviation");
            }

            if (abbreviation == "")
            {
                throw new ArgumentException("Argument 'abbreviation' cannot be the empty string.", "abbreviation");
            }

            return (WellKnownSidType)Array.IndexOf<string>(SecurityIdentity.wellKnownSidAbbreviations, abbreviation);
        }

        private static string GetErrorMessage(UInt32 errorCode)
        {
            UInt32 FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
            UInt32 FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
            UInt32 FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;

            UInt32 dwFlags = FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS;

            IntPtr source = new IntPtr();

            string msgBuffer = "";

            UInt32 retVal = FormatMessage(dwFlags, source, errorCode, 0, ref msgBuffer, 512, null);

            return msgBuffer.ToString();
        }
        #endregion
    }
    #endregion

    #region AccessControlEntry class
    /// <summary>
    /// Access Control Entry
    /// </summary>
    internal class AccessControlEntry
    {
        #region constants
        private static readonly string[] aceTypeStrings = new string[] { "A", "D", "OA", "OD", "AU", "AL", "OU", "OL" };
        private static readonly string[] aceFlagStrings = new string[] { "CI", "OI", "NP", "IO", "ID", "SA", "FA" };
        private static readonly string[] rightsStrings = new string[] {
            "GA",
            "GR",
            "GW",
            "GX",
            "RC",
            "SD",
            "WD",
            "WO",
            "RP",
            "WP",
            "CC",
            "DC",
            "LC",
            "SW",
            "LO",
            "DT",
            "CR",
            "FA",
            "FR",
            "FW",
            "FX",
            "KA",
            "KR",
            "KW",
            "KX"
        };

        private const string cAceExpr = @"^(?'ace_type'[A-Z]+)?;(?'ace_flags'([A-Z]{2})+)?;(?'rights'([A-Z]{2})+|0x[0-9A-Fa-f]+)?;(?'object_guid'[0-9A-Fa-f\-]+)?;(?'inherit_object_guid'[0-9A-Fa-f\-]+)?;(?'account_sid'[A-Z]+?|S(-[0-9]+)+)?$";
        #endregion

        #region member varible and default property initialization
        private AceType aceType = AceType.AccessAllowed;
        private AceFlags flags = AceFlags.None;
        private AceRights rights = AceRights.None;
        private Guid objectGuid = Guid.Empty;
        private Guid inheritObjectGuid = Guid.Empty;
        private SecurityIdentity accountSID;
        #endregion

        #region constructors and destructors
        /// <summary>
        /// Creates a Access Control Entry with account SID
        /// </summary>
        /// <param name="account">Account SID</param>
        public AccessControlEntry(SecurityIdentity account)
        {
            this.accountSID = account;
        }

        /// <summary>
        /// Creates a Access Control Entry of type AccessAllowed with account SID and Rights
        /// </summary>
        /// <param name="account">Account SID</param>
        /// <param name="rights">Rights</param>
        public AccessControlEntry(SecurityIdentity account, AceRights rights)
        {
            this.accountSID = account;
            this.rights = rights;
        }

        /// <summary>
        /// Creates a Access Control Entry of type AccessAllowed with account SID, Type and Rights
        /// </summary>
        /// <param name="account">Account SID</param>
        /// <param name="aceType">Type of Access Control</param>
        /// <param name="rights">Rights</param>
        public AccessControlEntry(SecurityIdentity account, AceType aceType, AceRights rights)
        {
            this.accountSID = account;
            this.aceType = aceType;
            this.rights = rights;
        }

        /// <summary>
        /// Creates a deep copy of an existing Access Control Entry
        /// </summary>
        /// <param name="original">Original AccessControlEntry</param>
        public AccessControlEntry(AccessControlEntry original)
        {
            this.accountSID = original.accountSID;
            this.aceType = original.aceType;
            this.flags = original.flags;
            this.inheritObjectGuid = original.inheritObjectGuid;
            this.objectGuid = original.objectGuid;
            this.rights = original.rights;
        }

        /// <summary>
        /// Creates a Access Control Entry from a ACE string
        /// </summary>
        /// <param name="aceString">ACE string</param>
        public AccessControlEntry(string aceString)
        {
            Regex aceRegex = new Regex(cAceExpr, RegexOptions.IgnoreCase);

            Match aceMatch = aceRegex.Match(aceString);
            if (!aceMatch.Success)
            {
                throw new FormatException("Invalid ACE String Format");
            }

            if (aceMatch.Groups["ace_type"] != null && aceMatch.Groups["ace_type"].Success && !String.IsNullOrEmpty(aceMatch.Groups["ace_type"].Value))
            {
                int aceTypeValue = Array.IndexOf<string>(AccessControlEntry.aceTypeStrings, aceMatch.Groups["ace_type"].Value.ToUpper());

                if (aceTypeValue == -1) throw new FormatException("Invalid ACE String Format");

                this.aceType = (AceType)aceTypeValue;
            }
            else
            {
                throw new FormatException("Invalid ACE String Format");
            }

            if (aceMatch.Groups["ace_flags"] != null && aceMatch.Groups["ace_flags"].Success && !String.IsNullOrEmpty(aceMatch.Groups["ace_flags"].Value))
            {
                string aceFlagsValue = aceMatch.Groups["ace_flags"].Value.ToUpper();
                for (int i = 0; i < aceFlagsValue.Length - 1; i += 2)
                {
                    int flagValue = Array.IndexOf<string>(AccessControlEntry.aceFlagStrings, aceFlagsValue.Substring(i, 2));

                    if (flagValue == -1) throw new FormatException("Invalid ACE String Format");

                    this.flags = this.flags | ((AceFlags)(int)Math.Pow(2.0d, flagValue));
                }
            }

            if (aceMatch.Groups["rights"] != null && aceMatch.Groups["rights"].Success && !String.IsNullOrEmpty(aceMatch.Groups["rights"].Value))
            {
                string rightsValue = aceMatch.Groups["rights"].Value.ToUpper();
                for (int i = 0; i < rightsValue.Length - 1; i += 2)
                {
                    int rightValue = Array.IndexOf<string>(AccessControlEntry.rightsStrings, rightsValue.Substring(i, 2));

                    if (rightValue == -1)
                    {
                        throw new FormatException("Invalid ACE String Format");
                    }

                    this.rights = this.rights | (AceRights)(int)Math.Pow(2.0d, rightValue);
                }
            }

            if (aceMatch.Groups["object_guid"] != null && aceMatch.Groups["object_guid"].Success && !String.IsNullOrEmpty(aceMatch.Groups["object_guid"].Value))
            {
                this.objectGuid = new Guid(aceMatch.Groups["object_guid"].Value);
            }

            if (aceMatch.Groups["inherit_object_guid"] != null && aceMatch.Groups["inherit_object_guid"].Success && !String.IsNullOrEmpty(aceMatch.Groups["inherit_object_guid"].Value))
            {
                this.inheritObjectGuid = new Guid(aceMatch.Groups["inherit_object_guid"].Value);
            }

            if (aceMatch.Groups["account_sid"] != null && aceMatch.Groups["account_sid"].Success && !String.IsNullOrEmpty(aceMatch.Groups["account_sid"].Value))
            {
                this.accountSID = SecurityIdentity.SecurityIdentityFromSIDorAbbreviation(aceMatch.Groups["account_sid"].Value.ToUpper());
            }
            else
            {
                throw new FormatException("Invalid ACE String Format");
            }
        }
        #endregion

        #region action methods
        /// <summary>
        /// Returns IEnumerable of rights
        /// </summary>
        /// <returns>IEnumerable of rights</returns>
        public IEnumerable<AceRights> GetRightsEnumerator()
        {
            int current = (int)AceRights.GenericAll;
            for (int col = (int)this.rights; col != 0; col = col >> 1, current = current << 1)
            {
                while (col != 0 && (col & 1) != 1)
                {
                    col = col >> 1;
                    current = current << 1;
                }

                if ((col & 1) == 1)
                {
                    yield return (AceRights)current;
                }
            }
        }

        /// <summary>
        /// Renders the Access Control Entry as an SDDL ACE string
        /// </summary>
        /// <returns>An SDDL ACE string.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0};", AccessControlEntry.aceTypeStrings[(int)this.aceType]);

            for (int flag = 0x01; flag <= (int)AceFlags.AuditFailure; flag = flag << 1)
            {
                if ((flag & (int)this.flags) == flag) sb.Append(AccessControlEntry.aceFlagStrings[(int)Math.Log(flag, 2.0d)]);
            }

            sb.Append(';');

            foreach (var right in this.GetRightsEnumerator())
            {
                sb.Append(AccessControlEntry.rightsStrings[(int)Math.Log((int)right, 2.0d)]);
            }

            sb.Append(';');

            sb.AppendFormat("{0};", this.objectGuid != Guid.Empty ? this.objectGuid.ToString() : "");

            sb.AppendFormat("{0};", this.inheritObjectGuid != Guid.Empty ? this.inheritObjectGuid.ToString() : "");

            if (this.accountSID != null) sb.Append(this.accountSID.ToString());

            return sb.ToString();
        }
        #endregion

        #region property getters/setters
        /// <summary>
        /// Gets or Sets the Access Control Entry Type
        /// </summary>
        public AceType AceType
        {
            get { return this.aceType; }
            set { this.aceType = value; }
        }

        /// <summary>
        /// Gets or Sets the Access Control Entry Flags
        /// </summary>
        public AceFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }

        /// <summary>
        /// Gets or Sets the Access Control Entry Rights
        /// </summary>
        /// <remarks>This is a binary flag value, and can be more easily 
        /// accessed via the Access Control Entry collection methods.</remarks>
        public AceRights Rights
        {
            get { return this.rights; }
            set { this.rights = value; }
        }

        /// <summary>
        /// Gets or Sets the Object Guid
        /// </summary>
        public Guid ObjectGuid
        {
            get { return this.objectGuid; }
            set { this.objectGuid = value; }
        }

        /// <summary>
        /// Gets or Sets the Inherit Object Guid
        /// </summary>
        public Guid InheritObjectGuid
        {
            get { return this.inheritObjectGuid; }
            set { this.inheritObjectGuid = value; }
        }

        /// <summary>
        /// Gets or Sets the Account SID
        /// </summary>
        public SecurityIdentity AccountSID
        {
            get { return this.accountSID; }
            set { this.accountSID = value; }
        }
        #endregion
    }
    #endregion

    #region AccessControlList class
    /// <summary>
    /// Access Control List
    /// </summary>
    internal class AccessControlList : IList<AccessControlEntry>
    {
        #region constants
        private const string cAclExpr = @"^(?'flags'[A-Z]+)?(?'ace_list'(\([^\)]+\))+)$";
        private const string cAceListExpr = @"\((?'ace'[^\)]+)\)";
        #endregion

        #region member varible and default property initialization
        private AclFlags flags = AclFlags.None;
        private List<AccessControlEntry> aceList;
        #endregion

        #region constructors and destructors
        /// <summary>
        /// Creates a Blank Access Control List
        /// </summary>
        public AccessControlList()
        {
            this.aceList = new List<AccessControlEntry>();
        }

        /// <summary>
        /// Creates a deep copy of an existing Access Control List
        /// </summary>
        /// <param name="original">Original AccessControlList</param>
        public AccessControlList(AccessControlList original)
        {
            this.aceList = new List<AccessControlEntry>();
            this.flags = original.flags;

            foreach (AccessControlEntry ace in original)
            {
                this.Add(new AccessControlEntry(ace));
            }
        }

        /// <summary>
        /// Creates an Access Control List from the DACL or SACL portion of an SDDL string
        /// </summary>
        /// <param name="aclString">The ACL String</param>
        public AccessControlList(string aclString)
        {
            this.aceList = new List<AccessControlEntry>();

            Regex aclRegex = new Regex(cAclExpr, RegexOptions.IgnoreCase);

            Match aclMatch = aclRegex.Match(aclString);
            if (!aclMatch.Success)
            {
                throw new FormatException("Invalid ACL String Format");
            }

            if (aclMatch.Groups["flags"] != null && aclMatch.Groups["flags"].Success && !String.IsNullOrEmpty(aclMatch.Groups["flags"].Value))
            {
                string flagString = aclMatch.Groups["flags"].Value.ToUpper();
                for (int i = 0; i < flagString.Length; i++)
                {
                    if (flagString[i] == 'P')
                    {
                        this.flags = this.flags | AclFlags.Protected;
                    }
                    else if (flagString.Length - i >= 2)
                    {
                        switch (flagString.Substring(i, 2))
                        {
                            case "AR":
                                this.flags = this.flags | AclFlags.MustInherit;
                                i++;
                                break;
                            case "AI":
                                this.flags = this.flags | AclFlags.Inherited;
                                i++;
                                break;
                            default:
                                throw new FormatException("Invalid ACL String Format");
                        }
                    }
                    else
                    {
                        throw new FormatException("Invalid ACL String Format");
                    }
                }
            }

            if (aclMatch.Groups["ace_list"] != null && aclMatch.Groups["ace_list"].Success && !String.IsNullOrEmpty(aclMatch.Groups["ace_list"].Value))
            {
                Regex aceListRegex = new Regex(cAceListExpr);

                foreach (Match aceMatch in aceListRegex.Matches(aclMatch.Groups["ace_list"].Value))
                {
                    this.Add(new AccessControlEntry(aceMatch.Groups["ace"].Value));
                }
            }
        }
        #endregion

        #region action methods
        /// <summary>
        /// Renders the Access Control List and an SDDL ACL string.
        /// </summary>
        /// <returns>An SDDL ACL string</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if ((this.flags & AclFlags.Protected) == AclFlags.Protected) sb.Append('P');
            if ((this.flags & AclFlags.MustInherit) == AclFlags.MustInherit) sb.Append("AR");
            if ((this.flags & AclFlags.Inherited) == AclFlags.Inherited) sb.Append("AI");

            foreach (AccessControlEntry ace in this.aceList)
            {
                sb.AppendFormat("({0})", ace.ToString());
            }

            return sb.ToString();
        }
        #endregion

        #region property getters/setters
        /// <summary>
        /// Gets or Sets the Access Control List flags
        /// </summary>
        public AclFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }
        #endregion

        #region List members
        /// <summary>
        /// Gets the Index of an <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" />
        /// </summary>
        /// <param name="item">The <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /></param>
        /// <returns>The index of the <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" />, or -1 if the Access Control Entry is not found</returns>
        public int IndexOf(AccessControlEntry item)
        {
            return this.aceList.IndexOf(item);
        }

        /// <summary>
        /// Inserts an <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /> into the Access Control List
        /// </summary>
        /// <param name="index">The insertion position</param>
        /// <param name="item">The <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /> to insert</param>
        public void Insert(int index, AccessControlEntry item)
        {
            this.aceList.Insert(index, item);
        }

        /// <summary>
        /// Removes the <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /> at the specified position
        /// </summary>
        /// <param name="index">The position to remove</param>
        public void RemoveAt(int index)
        {
            this.aceList.RemoveAt(index);
        }

        /// <summary>
        /// Gets or Sets an <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /> by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public AccessControlEntry this[int index]
        {
            get { return this.aceList[index]; }
            set { this.aceList[index] = value; }
        }

        /// <summary>
        /// Adds a <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /> to the Access Control List
        /// </summary>
        /// <param name="item">The <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /> to add</param>
        public void Add(AccessControlEntry item)
        {
            this.aceList.Add(item);
        }

        /// <summary>
        /// Clears all <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /> items from the Access Control List
        /// </summary>
        public void Clear()
        {
            this.aceList.Clear();
        }

        /// <summary>
        /// Checks if an <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /> exists in the Access Control List
        /// </summary>
        /// <param name="item">The <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /></param>
        /// <returns>true if the <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /> exists, otherwise false</returns>
        public bool Contains(AccessControlEntry item)
        {
            return this.aceList.Contains(item);
        }

        /// <summary>
        /// Copies all <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /> items from the Access Control List to an Array
        /// </summary>
        /// <param name="array">The array to copy to</param>
        /// <param name="arrayIndex">The index of the array at which to begin copying</param>
        public void CopyTo(AccessControlEntry[] array, int arrayIndex)
        {
            this.aceList.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /> items in the Access Control List
        /// </summary>
        public int Count
        {
            get { return this.aceList.Count; }
        }

        /// <summary>
        /// Returns false
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes an <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /> from the Access Control List
        /// </summary>
        /// <param name="item">The <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /> to remove</param>
        /// <returns>true if the <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /> was found and removed, otherwise false</returns>
        public bool Remove(AccessControlEntry item)
        {
            return this.aceList.Remove(item);
        }

        /// <summary>
        /// Gets an Enumerator over all <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /> items in the Access Control List
        /// </summary>
        /// <returns>An Enumerator over all <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /> items</returns>
        public IEnumerator<AccessControlEntry> GetEnumerator()
        {
            return this.aceList.GetEnumerator();
        }

        /// <summary>
        /// Gets an Enumerator over all <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /> items in the Access Control List
        /// </summary>
        /// <returns>An Enumerator over all <see cref="HttpNamespaceManager.Lib.AccessControl.AccessControlEntry" /> items</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)this.aceList).GetEnumerator();
        }
        #endregion
    }
    #endregion
}