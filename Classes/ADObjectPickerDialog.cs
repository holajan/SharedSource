using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security;
using IMP.SharedControls.ADObjectPicker.CustomSettings;
using IMP.Shared;

namespace IMP.SharedControls.ADObjectPicker
{
    namespace CustomSettings
    {
        #region DSOP scope flags types
        #pragma warning disable 1591
        /// <summary>
        /// Flags that indicate the scope types described by this structure. You can combine multiple scope types if all specified scopes use the same settings. 
        /// </summary>
        [Flags, CLSCompliant(false)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
        public enum DSOPScopeType : uint
        {
            TargetComputer = 0x00000001,
            UplevelJoinedDomain = 0x00000002,
            DownlevelJoinedDomain = 0x00000004,
            EnterpriseDomain = 0x00000008,
            GlobalCatalog = 0x00000010,
            ExternalUplevelDomain = 0x00000020,
            ExternalDownlevelDomain = 0x00000040,
            Workgroup = 0x00000080,
            UserEnteredUplevelScope = 0x00000100,
            UserEnteredDownlevelScope = 0x00000200
        }

        /// <summary>
        /// Flags that indicate the format used to return ADsPath for objects selected from this scope. The flScope member can also indicate the initial scope displayed in the Look in drop-down list. 
        /// </summary>
        [Flags, CLSCompliant(false)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
        public enum DSOPScopeFlags : uint
        {
            StartingScope = 0x00000001,
            WantProviderWINNT = 0x00000002,
            WantProviderLDAP = 0x00000004,
            WantProviderGC = 0x00000008,
            WantSIDPath = 0x00000010,
            WantDownlevelBuiltinPath = 0x00000020,
            DefaultFilterUsers = 0x00000040,
            DefaultFilterGroups = 0x00000080,
            DefaultFilterComputers = 0x00000100,
            DefaultFilterContacts = 0x00000200
        }

        /// <summary>
        /// Filter flags to use for an up-level scope, regardless of whether it is a mixed or native mode domain. 
        /// </summary>
        [Flags, CLSCompliant(false)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
        public enum DSOPFilterFlagsUplevel : uint
        {
            IncludeAdvancedView = 0x00000001,
            Users = 0x00000002,
            BuiltinGroups = 0x00000004,
            WellKnownPrincipals = 0x00000008,
            UniversalGroupsDL = 0x00000010,
            UniversalGroupsSE = 0x00000020,
            GlobalGroupsDL = 0x00000040,
            GlobalGroupsSE = 0x00000080,
            DomainLocalGroupsDL = 0x00000100,
            DomainLocalGroupsSE = 0x00000200,
            Contacts = 0x00000400,
            Computers = 0x00000800
        }

        /// <summary>
        /// Contains the filter flags to use for down-level scopes
        /// </summary>
        [Flags, CLSCompliant(false)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2217:DoNotMarkEnumsWithFlags")]
        public enum DSOPFilterFlagsDownlevel : uint
        {
            Users = 0x80000001,
            LocalGroups = 0x80000002,
            GlobalGroups = 0x80000004,
            Computers = 0x80000008,
            World = 0x80000010,
            AuthenticatedUser = 0x80000020,
            Anonymous = 0x80000040,
            Batch = 0x80000080,
            CreatorOwner = 0x80000100,
            CreatorGroup = 0x80000200,
            Dialup = 0x80000400,
            Interactive = 0x80000800,
            Network = 0x80001000,
            Service = 0x80002000,
            System = 0x80004000,
            ExcludeBuiltinGroups = 0x80008000,
            TerminalServer = 0x80010000,
            AllWellKnownSIDs = 0x80020000,
            LocalService = 0x80040000,
            NETWORK_SERVICE = 0x80080000,
            RemoteLogon = 0x80100000
        }
        #pragma warning restore 1591
        #endregion

        #region ADObjectPickerDialogScope class
        /// <summary>
        /// Scope settings
        /// </summary>
        public class ADObjectPickerDialogScope : ICloneable
        {
            #region member varible and default property initialization
            /// <summary>
            /// členská proměnná vlastnosti <c>ScopeType</c>
            /// </summary>
            private DSOPScopeType m_ScopeType;

            /// <summary>
            /// členská proměnná vlastnosti <c>ScopeFlags</c>
            /// </summary>
            private DSOPScopeFlags m_ScopeFlags;

            /// <summary>
            /// členská proměnná vlastnosti <c>FilterFlagsUplevel</c>
            /// </summary>
            private DSOPFilterFlagsUplevel m_FilterFlagsUplevel;

            /// <summary>
            /// členská proměnná vlastnosti <c>FilterFlagsDownlevel</c>
            /// </summary>
            private DSOPFilterFlagsDownlevel m_FilterFlagsDownlevel;
            #endregion

            #region constructors and destructors
            /// <summary>
            /// Default constructor
            /// </summary>
            public ADObjectPickerDialogScope()
            {
                m_ScopeType = 0;
                m_ScopeFlags = 0;
                m_FilterFlagsUplevel = 0;
                m_FilterFlagsDownlevel = 0;
            }

            /// <summary>
            /// Clone constructor
            /// </summary>
            private ADObjectPickerDialogScope(ADObjectPickerDialogScope Scope)
            {
                m_ScopeType = Scope.m_ScopeType;
                m_ScopeFlags = Scope.m_ScopeFlags;
                m_FilterFlagsUplevel = Scope.m_FilterFlagsUplevel;
                m_FilterFlagsDownlevel = Scope.m_FilterFlagsDownlevel;
            }
            #endregion

            #region action methods
            /// <summary>
            /// Creates a new object that is a copy of the current instance.
            /// </summary>
            /// <returns>A new object that is a copy of this instance.</returns>
            public ADObjectPickerDialogScope Clone()
            {
                return new ADObjectPickerDialogScope(this);
            }
            #endregion

            #region property getters/setters
            /// <summary>
            /// A scope type is a generic category of scopes, such as all domains in the enterprise to which the target computer belongs,
            /// or the global catalog for the target computer's enterprise, or the target computer itself.
            /// For each specified scope type, the dialog box uses the context of the target computer to determine the scope list entries.
            /// </summary>
            [CLSCompliant(false)]
            public DSOPScopeType ScopeType
            {
                get { return m_ScopeType; }
                set { m_ScopeType = value; }
            }

            /// <summary>
            /// The Look in drop-down list contains the scopes from which a user can select objects. A scope is a domain, computer, workgroup,
            /// or global catalog that stores information about and provides access to a set of available objects.
            /// The entries in the scope list depend on the scope types and the target computer specified when the Initialize method was last called to initialize the object picker dialog box.
            /// </summary>
            [CLSCompliant(false)]
            public DSOPScopeFlags ScopeFlags
            {
                get { return m_ScopeFlags; }
                set { m_ScopeFlags = value; }
            }

            /// <summary>
            /// An up-level scope is a global catalog or a Windows 2000 domain that supports the ADSI LDAP provider.
            /// </summary>
            [CLSCompliant(false)]
            public DSOPFilterFlagsUplevel FilterFlagsUplevel
            {
                get { return m_FilterFlagsUplevel; }
                set { m_FilterFlagsUplevel = value; }
            }

            /// <summary>
            /// A down-level scope includes Windows NT 4.0 domains, workgroups, and all individual computers, whether running Windows 2000 or Windows NT 4.0
            /// The dialog box uses the ADSI WinNT provider to access a down-level scope. 
            /// </summary>
            [CLSCompliant(false)]
            public DSOPFilterFlagsDownlevel FilterFlagsDownlevel
            {
                get { return m_FilterFlagsDownlevel; }
                set { m_FilterFlagsDownlevel = value; }
            }
            #endregion

            #region ICloneable Members
            object ICloneable.Clone()
            {
                return new ADObjectPickerDialogScope(this);
            }
            #endregion
        }
        #endregion

        #region ADObjectPickerDialogScopeSettings class
        /// <summary>
        /// First and Second search scope settings
        /// </summary>
        public class ADObjectPickerDialogScopeSettings : ICloneable
        {
            #region member varible and default property initialization
            /// <summary>
            /// členská proměnná vlastnosti <c>FirstScope</c>
            /// </summary>
            private ADObjectPickerDialogScope m_FirstScope;

            /// <summary>
            /// členská proměnná vlastnosti <c>SecondScope</c>
            /// </summary>
            private ADObjectPickerDialogScope m_SecondScope;
            #endregion

            #region constructors and destructors
            /// <summary>
            /// Default constructor
            /// </summary>
            public ADObjectPickerDialogScopeSettings()
            {
                m_FirstScope = new ADObjectPickerDialogScope();
            }

            /// <summary>
            /// Constructor with first Scope
            /// </summary>
            public ADObjectPickerDialogScopeSettings(ADObjectPickerDialogScope Scope)
            {
                m_FirstScope = Scope;
            }

            /// <summary>
            /// Clone constructor
            /// </summary>
            private ADObjectPickerDialogScopeSettings(ADObjectPickerDialogScopeSettings ScopeSettings)
            {
                m_FirstScope = ScopeSettings.m_FirstScope.Clone();

                if (ScopeSettings.m_SecondScope != null)
                {
                    m_SecondScope = ScopeSettings.m_SecondScope.Clone();
                }
            }

            /// <summary>
            /// Main constructor
            /// </summary>
            /// <param name="ObjectTypes">Types of objects to search in dialog</param>
            /// <param name="ComputerObjectTypes">Types of objects for target computer or None for same as ObjectTypes</param>
            /// <param name="SelectedObjectTypes">Types of objects to be selected in dialog</param>
            /// <param name="Locations">Location to search in dialog</param>
            /// <param name="StartupLocation">Startup location from locations in dialog</param>
            /// <param name="ReturnType">Provider to return in objects from dialog</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
            public ADObjectPickerDialogScopeSettings(ADObjectTypes ObjectTypes, ADObjectTypes ComputerObjectTypes, ADObjectTypes SelectedObjectTypes, ADObjectsLocations Locations, ADObjectsLocation StartupLocation, ADReturnType ReturnType)
            {
                ADObjectPickerDialogScope Scope = new ADObjectPickerDialogScope();

                //Set ScopeType according to Locations
                Scope.ScopeType = 0;

                if (ObjectTypes == ADObjectTypes.BuiltinGroups &&
                    (Locations & ADObjectsLocations.EntireDirectory) == ADObjectsLocations.EntireDirectory &&
                    (Locations & ADObjectsLocations.Domain) == ADObjectsLocations.Domain)
                {
                    //Oprava špatného nastavení location při BuiltinGroups a 
                    //natavení EntireDirectory s Domain nebo all
                    if (StartupLocation == ADObjectsLocation.Domain)
                    {
                        Scope.ScopeType = Scope.ScopeType | DSOPScopeType.GlobalCatalog;
                    }
                    else if (StartupLocation == ADObjectsLocation.EntireDirectory)
                    {
                        Scope.ScopeType = Scope.ScopeType | DSOPScopeType.UplevelJoinedDomain;
                    }
                }
                else
                {
                    if ((Locations & ADObjectsLocations.TargetComputer) == ADObjectsLocations.TargetComputer)
                    {
                        Scope.ScopeType = Scope.ScopeType | DSOPScopeType.TargetComputer;
                    }
                    if ((Locations & ADObjectsLocations.EntireDirectory) == ADObjectsLocations.EntireDirectory)
                    {
                        Scope.ScopeType = Scope.ScopeType | DSOPScopeType.GlobalCatalog;
                    }
                    if ((Locations & ADObjectsLocations.Domain) == ADObjectsLocations.Domain)
                    {
                        Scope.ScopeType = Scope.ScopeType | DSOPScopeType.EnterpriseDomain
                                | DSOPScopeType.UplevelJoinedDomain | DSOPScopeType.DownlevelJoinedDomain
                                | DSOPScopeType.ExternalUplevelDomain | DSOPScopeType.ExternalDownlevelDomain
                                | DSOPScopeType.UserEnteredUplevelScope | DSOPScopeType.UserEnteredDownlevelScope;
                    }
                }

                //Set ScopeFlags according to SelectedObjectTypes
                Scope.ScopeFlags = 0;

                if ((SelectedObjectTypes & ADObjectTypes.Users) == ADObjectTypes.Users)
                {
                    Scope.ScopeFlags = Scope.ScopeFlags | DSOPScopeFlags.DefaultFilterUsers;
                }
                if ((SelectedObjectTypes & ADObjectTypes.Groups) == ADObjectTypes.Groups)
                {
                    Scope.ScopeFlags = Scope.ScopeFlags | DSOPScopeFlags.DefaultFilterGroups;
                }
                if ((SelectedObjectTypes & ADObjectTypes.Computers) == ADObjectTypes.Computers)
                {
                    Scope.ScopeFlags = Scope.ScopeFlags | DSOPScopeFlags.DefaultFilterComputers;
                }
                if ((SelectedObjectTypes & ADObjectTypes.Contacts) == ADObjectTypes.Contacts)
                {
                    Scope.ScopeFlags = Scope.ScopeFlags | DSOPScopeFlags.DefaultFilterContacts;
                }

                //Set FilterFlagsUplevel according to ObjectTypes
                Scope.FilterFlagsUplevel = 0;

                if ((ObjectTypes & ADObjectTypes.Users) == ADObjectTypes.Users)
                {
                    Scope.FilterFlagsUplevel = Scope.FilterFlagsUplevel | DSOPFilterFlagsUplevel.Users;
                }
                if ((ObjectTypes & ADObjectTypes.Groups) == ADObjectTypes.Groups)
                {
                    Scope.FilterFlagsUplevel = Scope.FilterFlagsUplevel
                                | DSOPFilterFlagsUplevel.UniversalGroupsDL | DSOPFilterFlagsUplevel.UniversalGroupsSE
                                | DSOPFilterFlagsUplevel.GlobalGroupsDL | DSOPFilterFlagsUplevel.GlobalGroupsSE;
                }
                if ((ObjectTypes & ADObjectTypes.BuiltinGroups) == ADObjectTypes.BuiltinGroups)
                {
                    Scope.FilterFlagsUplevel = Scope.FilterFlagsUplevel | DSOPFilterFlagsUplevel.BuiltinGroups;
                }
                if ((ObjectTypes & ADObjectTypes.BuiltinPrincipals) == ADObjectTypes.BuiltinPrincipals)
                {
                    Scope.FilterFlagsUplevel = Scope.FilterFlagsUplevel | DSOPFilterFlagsUplevel.WellKnownPrincipals;
                }
                if ((ObjectTypes & ADObjectTypes.Computers) == ADObjectTypes.Computers)
                {
                    Scope.FilterFlagsUplevel = Scope.FilterFlagsUplevel | DSOPFilterFlagsUplevel.Computers;
                }
                if ((ObjectTypes & ADObjectTypes.Contacts) == ADObjectTypes.Contacts)
                {
                    Scope.FilterFlagsUplevel = Scope.FilterFlagsUplevel | DSOPFilterFlagsUplevel.Contacts;
                }

                //Set FilterFlagsDownlevel according to ComputerObjectTypes or ObjectTypes
                Scope.FilterFlagsDownlevel = 0;

                if (ComputerObjectTypes == 0)
                {
                    //Same as ObjectTypes
                    ComputerObjectTypes = ObjectTypes;
                }

                if ((ComputerObjectTypes & ADObjectTypes.Users) == ADObjectTypes.Users)
                {
                    Scope.FilterFlagsDownlevel = Scope.FilterFlagsDownlevel | DSOPFilterFlagsDownlevel.Users;
                }
                if ((ComputerObjectTypes & ADObjectTypes.Groups) == ADObjectTypes.Groups)
                {
                    Scope.FilterFlagsDownlevel = Scope.FilterFlagsDownlevel
                                | DSOPFilterFlagsDownlevel.LocalGroups | DSOPFilterFlagsDownlevel.GlobalGroups;

                    if ((ComputerObjectTypes & ADObjectTypes.BuiltinGroups) != ADObjectTypes.BuiltinGroups)
                    {
                        Scope.FilterFlagsDownlevel = Scope.FilterFlagsDownlevel | DSOPFilterFlagsDownlevel.ExcludeBuiltinGroups;
                    }
                }
                if ((ComputerObjectTypes & ADObjectTypes.BuiltinPrincipals) == ADObjectTypes.BuiltinPrincipals)
                {
                    Scope.FilterFlagsDownlevel = Scope.FilterFlagsDownlevel | DSOPFilterFlagsDownlevel.AllWellKnownSIDs;
                }
                if ((ComputerObjectTypes & ADObjectTypes.Computers) == ADObjectTypes.Computers)
                {
                    Scope.FilterFlagsDownlevel = Scope.FilterFlagsDownlevel | DSOPFilterFlagsDownlevel.Computers;
                }

                //Set return type
                switch (ReturnType)
                {
                    case ADReturnType.ProviderLDAP:
                        Scope.ScopeFlags = Scope.ScopeFlags | DSOPScopeFlags.WantProviderLDAP;
                        break;
                    case ADReturnType.ProviderLDAPSIDPath:
                        Scope.ScopeFlags = Scope.ScopeFlags | DSOPScopeFlags.WantProviderLDAP | DSOPScopeFlags.WantSIDPath;
                        break;
                    case ADReturnType.ProviderWINNT:
                        Scope.ScopeFlags = Scope.ScopeFlags | DSOPScopeFlags.WantProviderWINNT;
                        break;
                    case ADReturnType.ProviderGC:
                        Scope.ScopeFlags = Scope.ScopeFlags | DSOPScopeFlags.WantProviderGC;
                        break;
                }

                //Set StartingScope according to StartupLocation
                ADObjectPickerDialogScope StartingScope = Scope.Clone();

                switch (StartupLocation)
                {
                    case ADObjectsLocation.TargetComputer:
                        StartingScope.ScopeType = DSOPScopeType.TargetComputer;
                        break;
                    case ADObjectsLocation.EntireDirectory:
                        StartingScope.ScopeType = DSOPScopeType.GlobalCatalog;
                        break;
                    case ADObjectsLocation.Domain:
                        StartingScope.ScopeType = DSOPScopeType.UplevelJoinedDomain;
                        break;
                }

                StartingScope.ScopeFlags = StartingScope.ScopeFlags | DSOPScopeFlags.StartingScope;

                m_FirstScope = StartingScope;
                m_SecondScope = Scope;
            }
            #endregion

            #region action methods
            /// <summary>
            /// Creates a new object that is a copy of the current instance.
            /// </summary>
            /// <returns>A new object that is a copy of this instance.</returns>
            public ADObjectPickerDialogScopeSettings Clone()
            {
                return new ADObjectPickerDialogScopeSettings(this);
            }
            #endregion

            #region property getters/setters
            /// <summary>
            /// First search scope
            /// </summary>
            public ADObjectPickerDialogScope FirstScope
            {
                get { return m_FirstScope; }
                set { m_FirstScope = value; }
            }

            /// <summary>
            /// Second search scope
            /// </summary>
            public ADObjectPickerDialogScope SecondScope
            {
                get { return m_SecondScope; }
                set { m_SecondScope = value; }
            }
            #endregion

            #region ICloneable Members
            object ICloneable.Clone()
            {
                return new ADObjectPickerDialogScopeSettings(this);
            }
            #endregion
        }
        #endregion
    }

    #region public enums declarations
    /// <summary>
    /// Types of objects to search in dialog flags
    /// </summary>
    [Flags]
    public enum ADObjectTypes
    {
        /// <summary>
        /// Users
        /// </summary>
        Users = 1,
        /// <summary>
        /// Groups
        /// </summary>
        Groups = 2,
        /// <summary>
        /// Builtin Groups
        /// </summary>
        BuiltinGroups = 4,
        /// <summary>
        /// Builtin Security Principals
        /// </summary>
        BuiltinPrincipals = 8,
        /// <summary>
        /// Computers
        /// </summary>
        Computers = 16,
        /// <summary>
        /// Contacts
        /// </summary>
        Contacts = 32,
        /// <summary>
        /// Users and Groups
        /// </summary>
        UsersGroups = Users | Groups,
        /// <summary>
        /// Groups and Builtin Groups
        /// </summary>
        GroupsBuiltinGroups = Groups | BuiltinGroups,
        /// <summary>
        /// Users, Groups and Builtin Groups
        /// </summary>
        UsersGroupsBuiltinGroups = Users | Groups | BuiltinGroups,
        /// <summary>
        /// Users, Groups and Builtin Security Principals
        /// </summary>
        UsersGroupsBuiltinPrincipals = Users | Groups | BuiltinPrincipals,
        /// <summary>
        /// Users, Groups, Builtin Groups and Builtin Security Principals
        /// </summary>
        UsersGroupsBuiltinGroupsBuiltinPrincipals = Users | Groups | BuiltinGroups | BuiltinPrincipals,
        /// <summary>
        /// Users, Groups, Builtin Security Principals and Computers
        /// </summary>
        UsersGroupsBuiltinPrincipalsComputers = Users | Groups | BuiltinPrincipals | Computers,
        /// <summary>
        /// None objects selected
        /// </summary>
        None = 0
    }

    /// <summary>
    /// Location to search in dialog flags
    /// </summary>
    [Flags]
    public enum ADObjectsLocations
    {
        /// <summary>
        /// Local or Target Computer
        /// </summary>
        TargetComputer = 1,
        /// <summary>
        /// Entire Directory
        /// </summary>
        EntireDirectory = 2,
        /// <summary>
        /// Current Domain
        /// </summary>
        Domain = 4,
        /// <summary>
        /// Entire Directory and domains
        /// </summary>
        EntireDirectoryAndDomains = EntireDirectory | Domain,
        /// <summary>
        /// Target Computer, Entire Directory and domains
        /// </summary>
        All = TargetComputer | EntireDirectory | Domain
    }

    /// <summary>
    /// Enum to set startup location from locations in dialog
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    public enum ADObjectsLocation
    {
        /// <summary>
        /// Local or target computer
        /// </summary>
        TargetComputer = 1,
        /// <summary>
        /// Entire Directory
        /// </summary>
        EntireDirectory = 2,
        /// <summary>
        /// Current Domain
        /// </summary>
        Domain = 4
    }

    /// <summary>
    /// Enum to set provider to return in objects from dialog
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum ADReturnType
    {
        /// <summary>
        /// LDAP, GC or WINNT provider by selected objects location
        /// </summary>
        ByLocation = 1,
        /// <summary>
        /// LDAP provider
        /// </summary>
        ProviderLDAP = 2,
        /// <summary>
        /// LDAP provider with SID path 
        /// </summary>
        ProviderLDAPSIDPath = 3,
        /// <summary>
        /// WINNT provider 
        /// </summary>
        ProviderWINNT = 4,
        /// <summary>
        /// GC provider 
        /// </summary>
        ProviderGC = 5
    }
    #endregion

    #region ADObject and ADObjectCollection class
    /// <summary>
    /// AD Objects item returned in collection from dialog
    /// </summary>
    public class ADObject
    {
        #region member varible and default property initialization
        private string m_Name;
        private string m_ADPath;
        private string m_ClassName;
        private string m_UPN;
        private ADObjectsLocation m_Location;
        #endregion

        #region constructors and destructors
        /// <summary>
        /// Initializes a new instance of the ADObject class.
        /// </summary>
        /// <param name="Name">The object's RDN</param>
        /// <param name="ADPath">The object's ADsPath</param>
        /// <param name="ClassName">The object's class attribute value</param>
        /// <param name="UPN">The object's userPrincipalName attribute value</param>
        /// <param name="Location">Location from which this object was selected</param>
        public ADObject(string Name, string ADPath, string ClassName, string UPN, ADObjectsLocation Location)
        {
            m_Name = Name;
            m_ADPath = ADPath;
            m_ClassName = ClassName;
            m_UPN = UPN;
            m_Location = Location;
        }
        #endregion

        #region property getters/setters
        /// <summary>
        /// The object's RDN
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// The object's ADsPath
        /// </summary>
        public string ADPath
        {
            get { return m_ADPath; }
        }

        /// <summary>
        /// The object's class attribute value
        /// </summary>
        public string ClassName
        {
            get { return m_ClassName; }
        }

        /// <summary>
        /// The object's userPrincipalName attribute value
        /// </summary>
        public string UPN
        {
            get { return m_UPN; }
        }

        /// <summary>
        /// Location from which this object was selected
        /// </summary>
        public ADObjectsLocation Location
        {
            get { return m_Location; }
        }
        #endregion
    }

    /// <summary>
    /// AD Objects collection returned from dialog
    /// </summary>
    public class ADObjectCollection : ReadOnlyCollection<ADObject>
    {
        internal ADObjectCollection(List<ADObject> list) : base(list) { }
    }
    #endregion

    /// <summary>
    /// Represents a common dialog box to call up the Active Directory object Picker dialog.
    /// This dialog can by used to select users and groups or find computers.
    /// </summary>
	/// <histories>
	/// <history date="18.12.2005" author="Jan Holan">Vytvoření</history>
    /// <history date="19.11.2006" author="Jan Holan">Oprava padání dialogu pod x64 systémy</history>
    /// </histories>
    [
    ToolboxItem(true),
    ToolboxBitmap(typeof(IMP.SharedControls.ADObjectPicker.ADObjectPickerDialog)),
    Description("Represents a common dialog box to call up the Active Directory object Picker dialog. This dialog can by used to select users and groups or find computers."),
    DefaultProperty("ObjectTypes"),
    ]
    public class ADObjectPickerDialog : CommonDialog
    {
        #region private member types definition
        /// <summary>
        /// Flags that determine the object picker options.
        /// </summary>
        private static class DSOPOptionFlags
        {
            public const int Multiselect = 0x00000001;
            public const int SkipTargetComputerDCCheck = 0x00000002;
        }

        /// <summary>
        /// This structure is used as a parameter in OLE functions and methods that require data format information.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct FORMATETC
        {
            public int cfFormat;
            public IntPtr ptd;
            public int dwAspect;
            public int lindex;
            public int tymed;
        }

        /// <summary>
        /// The STGMEDIUM structure is a generalized global memory handle used for data transfer operations by the IDataObject
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct STGMEDIUM
        {
            public int tymed;
            public IntPtr hGlobal;
            public IntPtr pUnkForRelease;
        }

        /// <summary>
        /// The DSOP_INIT_INFO structure contains data required to initialize an object picker dialog box. 
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct DSOP_INIT_INFO
        {
            public int cbSize;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pwzTargetComputer;
            public int cDsScopeInfos;
            public IntPtr aDsScopeInfos;
            public int flOptions;
            public int cAttributesToFetch;
            public IntPtr apwzAttributeNames;
        }

        /// <summary>
        /// The DSOP_SCOPE_INIT_INFO structure describes one or more scope types that have the same attributes. A scope type is a type of location, for example a domain, computer, or Global Catalog, from which the user can select objects.
        /// A scope type is a type of location, for example a domain, computer, or Global Catalog, from which the user can select objects. 
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct DSOP_SCOPE_INIT_INFO
        {
            public int cbSize;
            public int flType;
            public int flScope;
            [MarshalAs(UnmanagedType.Struct)]
            public DSOP_FILTER_FLAGS FilterFlags;
            public IntPtr pwzDcName;
            public IntPtr pwzADsPath;
            public int hr;
        }

        /// <summary>
        /// The DSOP_UPLEVEL_FILTER_FLAGS structure contains flags that indicate the filters to use for an up-level scope. An up-level scope is a scope that supports the ADSI LDAP provider.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct DSOP_UPLEVEL_FILTER_FLAGS
        {
            public int flBothModes;
            public int flMixedModeOnly;
            public int flNativeModeOnly;
        }

        /// <summary>
        /// The DSOP_FILTER_FLAGS structure contains flags that indicate the types of objects presented to the user for a specified scope or scopes.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct DSOP_FILTER_FLAGS
        {
            [MarshalAs(UnmanagedType.Struct)]
            public DSOP_UPLEVEL_FILTER_FLAGS Uplevel;
            public int flDownlevel;
        }

        /// <summary>
        /// The CFSTR_DSOP_DS_SELECTION_LIST clipboard format is provided by the IDataObject obtained by calling IDsObjectPicker.InvokeDialog
        /// </summary>
        private static class CLIPBOARD_FORMAT
        {
            public const string CFSTR_DSOP_DS_SELECTION_LIST = "CFSTR_DSOP_DS_SELECTION_LIST";
        }

        /// <summary>
        /// The TYMED enumeration values indicate the type of storage medium being used in a data transfer. 
        /// </summary>
        private enum TYMED
        {
            TYMED_HGLOBAL = 1,
            TYMED_FILE = 2,
            TYMED_ISTREAM = 4,
            TYMED_ISTORAGE = 8,
            TYMED_GDI = 16,
            TYMED_MFPICT = 32,
            TYMED_ENHMF = 64,
            TYMED_NULL = 0
        }

        /// <summary>
        /// The DVASPECT enumeration values specify the desired data or view aspect of the object when drawing or getting data.
        /// </summary>
        private enum DVASPECT
        {
            DVASPECT_CONTENT = 1,
            DVASPECT_THUMBNAIL = 2,
            DVASPECT_ICON = 4,
            DVASPECT_DOCPRINT = 8
        }
        #endregion

        #region DSObjectPicker COM Interop types
        /// <summary>
        /// The object picker dialog box.
        /// </summary>
        [ComImport, Guid("17D6CCD8-3B7B-11D2-B9E0-00C04FD8DBF7")]
        private class DSObjectPicker { }

        /// <summary>
        /// The IDsObjectPicker interface is used by an application to initialize and display an object picker dialog box. 
        /// </summary>
        [ComImport, Guid("0C87E64E-3B7A-11D2-B9E0-00C04FD8DBF7")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDsObjectPicker
        {
            void Initialize([In()]ref DSOP_INIT_INFO pInitInfo);
            void InvokeDialog([In()]HandleRef hWnd, out IDataObject lpDataObject);
        }

        /// <summary>
        /// Interface to enable data transfers
        /// </summary>
        [ComImport, Guid("0000010E-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IDataObject
        {
            void GetData(ref FORMATETC pFormatEtc, ref STGMEDIUM pStg);
            void GetDataHere(ref FORMATETC pFormatEtc, ref STGMEDIUM pStg);
            void QueryGetData(ref FORMATETC pFormatEtc);
            void GetCanonicalFormatEtc(ref FORMATETC pFormatEtcIn, ref FORMATETC pFormatEtcOut);
            void SetData(ref FORMATETC pFormatEtc, ref STGMEDIUM pStg, bool fRelease);
            void EnumFormatEtc(uint dwDirection, ref IntPtr ppEnumFormat);
            void DAdvise(ref FORMATETC pFormatEtc, int advf, ref IntPtr pAdvSink, ref int pdfConnection);
            void DUnadvise(int dwConnection);
            void EnumDAdvise(ref IntPtr ppEnumAdvise);
        }
        #endregion

        #region UnsafeNativeMethods class
        [SuppressUnmanagedCodeSecurity]
        private static class UnsafeNativeMethods
        {
            /// <summary>
            /// The GlobalLock function locks a global memory object and returns a pointer to the first byte of the object's memory block.
            /// GlobalLock function increments the lock count by one.
            /// Needed for the clipboard functions when getting the data from IDataObject
            /// </summary>
            /// <param name="hMem"></param>
            /// <returns></returns>
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage")]
            public static extern IntPtr GlobalLock(IntPtr hMem);

            /// <summary>
            /// The GlobalUnlock function decrements the lock count associated with a memory object.
            /// </summary>
            /// <param name="hMem"></param>
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage")]
            public static extern Int32 GlobalUnlock(IntPtr hMem);

            [DllImport("ole32.dll", EntryPoint = "ReleaseStgMedium", PreserveSig = false)]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage")]
            public static extern void ReleaseStgMedium(ref STGMEDIUM medium);
        }
        #endregion

        #region member varible and default property initialization
        /// <summary>
        /// členská proměnná vlastnosti <c>ObjectTypes</c>
        /// </summary>
        private ADObjectTypes m_ObjectTypes = ADObjectTypes.UsersGroupsBuiltinGroups;

        /// <summary>
        /// členská proměnná vlastnosti <c>ComputerObjectTypes</c>
        /// </summary>
        private ADObjectTypes m_ComputerObjectTypes = ADObjectTypes.None;

        /// <summary>
        /// členská proměnná vlastnosti <c>SelectedObjectTypes</c>
        /// </summary>
        private ADObjectTypes m_SelectedObjectTypes = ADObjectTypes.UsersGroupsBuiltinGroups;

        /// <summary>
        /// členská proměnná vlastnosti <c>Location</c>
        /// </summary>
        private ADObjectsLocations m_Locations = ADObjectsLocations.All;

        /// <summary>
        /// členská proměnná vlastnosti <c>StartupLocation</c>
        /// </summary>
        private ADObjectsLocation m_StartupLocation = ADObjectsLocation.Domain;

        /// <summary>
        /// členská proměnná vlastnosti <c>Return</c>
        /// </summary>
        private ADReturnType m_ReturnType = ADReturnType.ByLocation;

        /// <summary>
        /// členská proměnná vlastnosti <c>CustomScopeSettings</c>
        /// </summary>
        private ADObjectPickerDialogScopeSettings m_CustomScopeSettings;

        /// <summary>
        /// členská proměnná vlastnosti <c>Multiselect</c>
        /// </summary>
        private bool m_Multiselect = true;
            
        /// <summary>
        /// členská proměnná vlastnosti <c>SkipTargetComputerDCCheck</c>
        /// </summary>
        private bool m_SkipTargetComputerDCCheck = true;

        /// <summary>
        /// členská proměnná vlastnosti <c>ComputerName</c>
        /// </summary>
        private string m_ComputerName = "";

        /// <summary>
        /// členská proměnná vlastnosti <c>ADObjectColection</c>
        /// </summary>
        private List<ADObject> m_SelectedObjects;
        #endregion

        #region constructors and destructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public ADObjectPickerDialog()  { }

        /// <summary>
        /// Scope settings constructor
        /// </summary>
        /// <param name="CustomScopeSettings">Scope settings</param>
        public ADObjectPickerDialog(ADObjectPickerDialogScopeSettings CustomScopeSettings)
        {
            m_CustomScopeSettings = CustomScopeSettings;
        }

        /// <summary>
        /// Scope property settings constructor
        /// </summary>
        /// <param name="ObjectTypes">Types of objects to search in dialog</param>
        /// <param name="ComputerObjectTypes">Types of objects for target computer or None for same as ObjectTypes</param>
        /// <param name="SelectedObjectTypes">Types of objects to be selected in dialog</param>
        /// <param name="Locations">Location to search in dialog</param>
        /// <param name="StartupLocation">Startup location from locations in dialog</param>
        /// <param name="ReturnType">Provider to return in objects from dialog</param>
        public ADObjectPickerDialog(ADObjectTypes ObjectTypes, ADObjectTypes ComputerObjectTypes, ADObjectTypes SelectedObjectTypes, ADObjectsLocations Locations, ADObjectsLocation StartupLocation, ADReturnType ReturnType)
        {
            if (ObjectTypes == ADObjectTypes.None)
            {
                throw new ArgumentException("Invalid ObjectTypes", "ObjectTypes");
            }

            m_ObjectTypes = ObjectTypes;
            m_ComputerObjectTypes = ComputerObjectTypes;
            m_SelectedObjectTypes = SelectedObjectTypes;
            m_Locations = Locations;
            m_StartupLocation = StartupLocation;
            m_ReturnType = ReturnType;
        }
        #endregion

        #region property getters/setters
        /// <summary>
        /// Types of objects to search in dialog
        /// </summary>
        [
        Browsable(true), Category("Behavior"),
        Description("Types of objects to search in dialog."),
        DefaultValue(typeof(ADObjectTypes), "UsersGroupsBuiltinGroups"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "System.InvalidOperationException.#ctor(System.String)")
        ]
        public ADObjectTypes ObjectTypes
        {
            get { return m_ObjectTypes; }
            set 
            {
                if (value == ADObjectTypes.None)
                {
                    throw new InvalidOperationException("Invalid ObjectTypes");
                }

                m_ObjectTypes = value; 
            }
        }

        /// <summary>
        /// Types of objects for target computer or <c>None</c> for same as <c>ObjectTypes</c>
        /// </summary>
        [
        Browsable(true), Category("Behavior"),
        Description("Types of objects for target computer or None for same as ObjectTypes."),
        DefaultValue(typeof(ADObjectTypes), "None")
        ]
        public ADObjectTypes ComputerObjectTypes
        {
            get { return m_ComputerObjectTypes; }
            set { m_ComputerObjectTypes = value; }
        }

        /// <summary>
        /// Types of objects to be selected in dialog
        /// </summary>
        [
        Browsable(true), Category("Behavior"),
        Description("Types of objects to be selected in dialog."),
        DefaultValue(typeof(ADObjectTypes), "UsersGroupsBuiltinGroups")
        ]
        public ADObjectTypes SelectedObjectTypes
        {
            get { return m_SelectedObjectTypes; }
            set { m_SelectedObjectTypes = value; }
        }

        /// <summary>
        /// Location to search in dialog
        /// </summary>
        [
        Browsable(true), Category("Behavior"),
        Description("Location to search in dialog."),
        DefaultValue(typeof(ADObjectsLocations), "All")
        ]
        public ADObjectsLocations Locations
        {
            get { return m_Locations; }
            set { m_Locations = value; }
        }

        /// <summary>
        /// Startup location from locations in dialog
        /// </summary>
        [
        Browsable(true), Category("Behavior"),
        Description("Startup location from locations in dialog."),
        DefaultValue(typeof(ADObjectsLocation), "Domain")
        ]
        public ADObjectsLocation StartupLocation
        {
            get { return m_StartupLocation; }
            set { m_StartupLocation = value; }
        }

        /// <summary>
        /// Provider to return in objects from dialog
        /// </summary>
        [
        Browsable(true), Category("Behavior"),
        Description("Provider to return in objects from dialog."),
        DefaultValue(typeof(ADReturnType), "ByLocation")
        ]
        public ADReturnType ReturnType
        {
            get { return m_ReturnType; }
            set { m_ReturnType = value; }
        }

        /// <summary>
        /// Custom search scope settings for dialog
        /// </summary>
        [Browsable(false)]
        public ADObjectPickerDialogScopeSettings CustomScopeSettings
        {
            get { return m_CustomScopeSettings; }
            set { m_CustomScopeSettings = value; }
        }

        /// <summary>
        /// Allow user to select multiple objects
        /// </summary>
        [
        Browsable(true), Category("Behavior"),
        Description("Allow user to select multiple objects."),
        DefaultValue(typeof(bool), "true")
        ]
        public bool Multiselect
        {
            get { return m_Multiselect; }
            set { m_Multiselect = value; }
        }

        /// <summary>
        /// Specify that this is not a domain controller
        /// </summary>
        [
        Browsable(false),
        DefaultValue(typeof(bool), "true")
        ]
        public bool SkipTargetComputerDCCheck
        {
            get { return m_SkipTargetComputerDCCheck; }
            set { m_SkipTargetComputerDCCheck = value; }
        }

        /// <summary>
        /// Computer name or empty string for local computer
        /// </summary>
        [
        Browsable(true), Category("Behavior"),
        Description("Computer name or empty string for local computer."),
        DefaultValue(typeof(string), "")
        ]
        public string ComputerName
        {
            get { return m_ComputerName; }
            set { m_ComputerName = value; }
        }

        /// <summary>
        /// AD Objects result from dialog
        /// </summary>
        [Browsable(false)]
        public ADObjectCollection SelectedObjects
        {
            get { return new ADObjectCollection(m_SelectedObjects); }
        }
        #endregion

        #region private member functions
        /// <summary>
        /// Initialize IDsObjectPicker object
        /// </summary>
        /// <returns>IDsObjectPicker object</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private IDsObjectPicker InitializePicker()
        {
            ADObjectPickerDialogScopeSettings ScopeSettings = m_CustomScopeSettings;

            if (ScopeSettings == null)
            {
                ScopeSettings = new ADObjectPickerDialogScopeSettings(m_ObjectTypes, m_ComputerObjectTypes, m_SelectedObjectTypes, m_Locations, m_StartupLocation, m_ReturnType);
            }

            DSObjectPicker Picker = new DSObjectPicker();
            IDsObjectPicker iPicker = (IDsObjectPicker)Picker;

            DSOP_SCOPE_INIT_INFO[] scopeInitInfo = new DSOP_SCOPE_INIT_INFO[2];

            //Initialize 1st search scope
            scopeInitInfo[0].cbSize = Marshal.SizeOf(typeof(DSOP_SCOPE_INIT_INFO));
            scopeInitInfo[0].flType = (int)ScopeSettings.FirstScope.ScopeType;
            scopeInitInfo[0].flScope = (int)ScopeSettings.FirstScope.ScopeFlags;
            scopeInitInfo[0].FilterFlags.Uplevel.flBothModes = (int)ScopeSettings.FirstScope.FilterFlagsUplevel;
            scopeInitInfo[0].FilterFlags.Uplevel.flMixedModeOnly = 0;
            scopeInitInfo[0].FilterFlags.Uplevel.flNativeModeOnly = 0;
            scopeInitInfo[0].FilterFlags.flDownlevel = unchecked((int)ScopeSettings.FirstScope.FilterFlagsDownlevel);
            scopeInitInfo[0].pwzADsPath = IntPtr.Zero;
            scopeInitInfo[0].pwzDcName = IntPtr.Zero;
            scopeInitInfo[0].hr = 0;

            //Initialize 2nd search scope
            scopeInitInfo[1].cbSize = Marshal.SizeOf(typeof(DSOP_SCOPE_INIT_INFO));

            if (ScopeSettings.SecondScope != null)
            {
                scopeInitInfo[1].flType = (int)ScopeSettings.SecondScope.ScopeType;
                scopeInitInfo[1].flScope = (int)ScopeSettings.SecondScope.ScopeFlags;
                scopeInitInfo[1].FilterFlags.Uplevel.flBothModes = (int)ScopeSettings.SecondScope.FilterFlagsUplevel;
                scopeInitInfo[1].FilterFlags.Uplevel.flMixedModeOnly = 0;
                scopeInitInfo[1].FilterFlags.Uplevel.flNativeModeOnly = 0;
                scopeInitInfo[1].FilterFlags.flDownlevel = unchecked((int)ScopeSettings.SecondScope.FilterFlagsDownlevel);
                scopeInitInfo[1].pwzADsPath = IntPtr.Zero;
                scopeInitInfo[1].pwzDcName = IntPtr.Zero;
                scopeInitInfo[1].hr = 0;
            }

            //Allocate memory from the unmananged mem of the process, this should be freed later
            IntPtr refScopeInitInfo = Marshal.AllocHGlobal
                (Marshal.SizeOf(typeof(DSOP_SCOPE_INIT_INFO)) * 2);

            //Marshal structs to pointers
            Marshal.StructureToPtr(scopeInitInfo[0],
                refScopeInitInfo, true);

            Marshal.StructureToPtr(scopeInitInfo[1],
                (IntPtr)((int)refScopeInitInfo +
                Marshal.SizeOf(typeof(DSOP_SCOPE_INIT_INFO))), true);

            //Initialize structure with data to initialize an object picker dialog box. 
            DSOP_INIT_INFO initInfo = new DSOP_INIT_INFO();
            initInfo.cbSize = Marshal.SizeOf(initInfo);
            initInfo.pwzTargetComputer = (m_ComputerName.Length == 0 ? null : m_ComputerName);
            initInfo.cDsScopeInfos = (ScopeSettings.SecondScope == null ? 1 : 2);
            initInfo.aDsScopeInfos = refScopeInitInfo;
            initInfo.flOptions = 0;
            if (m_Multiselect)
            {
                initInfo.flOptions = initInfo.flOptions | DSOPOptionFlags.Multiselect;
            }
            if (m_SkipTargetComputerDCCheck)
            {
                initInfo.flOptions = initInfo.flOptions | DSOPOptionFlags.SkipTargetComputerDCCheck;
            }

            //We're not retrieving any additional attributes
            initInfo.cAttributesToFetch = 0;
            initInfo.apwzAttributeNames = IntPtr.Zero;

            try
            {
                //Initialize the Object Picker Dialog Box with our options
                iPicker.Initialize(ref initInfo);
                return iPicker;
            }
            catch
            {
                //Dialog allready display error message
                return null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "IMP.SharedControls.ADObjectPicker.ADObjectPickerDialog+UnsafeNativeMethods.GlobalUnlock(System.IntPtr)")]
        private void SetData(IDataObject DataObject)
        {
            List<ADObject> ObjectColection = null;

            //The STGMEDIUM structure is a generalized global memory handle used for data transfer operations
            STGMEDIUM stg = new STGMEDIUM();
            stg.tymed = (int)TYMED.TYMED_HGLOBAL;
            stg.hGlobal = IntPtr.Zero;
            stg.pUnkForRelease = IntPtr.Zero;

            try
            {
                //The FORMATETC structure is a generalized Clipboard format.
                FORMATETC fe = new FORMATETC();
                //The CFSTR_DSOP_DS_SELECTION_LIST clipboard format is provided by the IDataObject obtained by calling IDsObjectPicker::InvokeDialog
                fe.cfFormat = System.Windows.Forms.DataFormats.GetFormat(CLIPBOARD_FORMAT.CFSTR_DSOP_DS_SELECTION_LIST).Id;
                fe.ptd = IntPtr.Zero;
                fe.dwAspect = (int)DVASPECT.DVASPECT_CONTENT;
                fe.lindex = -1; // all of the data
                fe.tymed = (int)TYMED.TYMED_HGLOBAL; //The storage medium is a global memory handle (HGLOBAL)

                DataObject.GetData(ref fe, ref stg);

                IntPtr pDSSelList = UnsafeNativeMethods.GlobalLock(stg.hGlobal);

                try
                {
                    //if we selected at least 1 object
                    if (pDSSelList != IntPtr.Zero)
                    {
                        //pDSSelList is pointer to DS_SELECTION_LIST structure contains data about the objects the user selected from an object picker dialog box.
                        //
                        //DS_SELECTION_LIST
                        //=================
                        //Available as a clipboard format from the data object returned by IDsObjectPicker::InvokeDialog.
                        //Contains a list of objects that the user selected.
                        //
                        //typedef struct _DS_SELECTION_LIST
                        //{
                        //    ULONG           cItems;                      - Number of elements in the aDsSelection array.
                        //    ULONG           cFetchedAttributes;          - Number of elements in each DSSELECTION.avarFetchedAttributes member.    
                        //    DS_SELECTION    aDsSelection[ANYSIZE_ARRAY]; - Array of cItems DSSELECTION structures.
                        //} DS_SELECTION_LIST, *PDS_SELECTION_LIST;

                        //get the count of items selected from cItems field of DS_SELECTION_LIST structure
                        int Items = Marshal.ReadInt32(pDSSelList);

                        ObjectColection = new List<ADObject>();

                        //now loop through the structures
                        for (int i = 0; i < Items; i++)
                        {
                            //get the pointer to DS_SELECTION structure from aDsSelection field of DS_SELECTION_LIST structure
                            int Current = 2 * Marshal.SizeOf(typeof(Int32)) + i * 6 * Marshal.SizeOf(typeof(IntPtr));

                            //Current is pointer to DS_SELECTION structure contains data about an object the user selected from an object picker dialog box.
                            //DS_SELECTION
                            //============
                            //Describes an object selected by the user.
                            //The DS_SELECTION_LIST structure contains an array of DS_SELECTION structures.
                            //
                            //typedef struct _DS_SELECTION
                            //{
                            //    PWSTR      pwzName;               - The object's RDN.
                            //    PWSTR      pwzADsPath;            - The object's ADsPath.
                            //    PWSTR      pwzClass;              - The object's class attribute value.
                            //    PWSTR      pwzUPN;                - The object's userPrincipalName attribute value.
                            //    VARIANT   *pvarFetchedAttributes; - An array of VARIANTs, one for each attribute fetched.
                            //    ULONG      flScopeType;           - A single DSOP_SCOPE_TYPE_* flag describing the type of the scope from which this object was selected.
                            //} DS_SELECTION, *PDS_SELECTION;

                            //get the strings from fields of DS_SELECTION structure
                            string Name = Marshal.PtrToStringUni(Marshal.ReadIntPtr(pDSSelList, Current));
                            string ADsPath = Marshal.PtrToStringUni(Marshal.ReadIntPtr(pDSSelList, 1 * Marshal.SizeOf(typeof(IntPtr)) + Current));
                            string Class = Marshal.PtrToStringUni(Marshal.ReadIntPtr(pDSSelList, 2 * Marshal.SizeOf(typeof(IntPtr)) + Current));
                            string UPN = Marshal.PtrToStringUni(Marshal.ReadIntPtr(pDSSelList, 3 * Marshal.SizeOf(typeof(IntPtr)) + Current));
                            DSOPScopeType ScopeType = (DSOPScopeType)Marshal.ReadInt32(pDSSelList, 5 * Marshal.SizeOf(typeof(IntPtr)) + Current);

                            ObjectColection.Add(new ADObject(Name, ADsPath, Class, UPN, GetLocation(ScopeType)));
                        }
                    }
                }
                finally
                {
                    UnsafeNativeMethods.GlobalUnlock(pDSSelList);
                }
            }
            finally
            {
                UnsafeNativeMethods.ReleaseStgMedium(ref stg);
            }

            m_SelectedObjects = ObjectColection;
        }

        private static ADObjectsLocation GetLocation(DSOPScopeType ScopeType)
        {
            if (ScopeType == DSOPScopeType.TargetComputer)
            {
                return ADObjectsLocation.TargetComputer;
            }

            if (ScopeType == DSOPScopeType.GlobalCatalog)
            {
                return ADObjectsLocation.EntireDirectory;
            }

            return ADObjectsLocation.Domain;
        }
        #endregion

		#region CommonDialog Members
        /// <summary>
        /// Resets the properties of a common dialog box to their default values.
        /// </summary>
        public override void Reset()
        {
            m_ObjectTypes = ADObjectTypes.UsersGroupsBuiltinGroups;
            m_ComputerObjectTypes = ADObjectTypes.None;
            m_SelectedObjectTypes = ADObjectTypes.UsersGroupsBuiltinGroups;
            m_Locations = ADObjectsLocations.All;
            m_StartupLocation = ADObjectsLocation.Domain;
            m_ReturnType = ADReturnType.ByLocation;
            m_CustomScopeSettings = null;
            m_Multiselect = true;
            m_SkipTargetComputerDCCheck = true;
            m_ComputerName = "";
        }

        /// <summary>
        /// Invoke ADObjectPicker common dialog box.
        /// </summary>
        /// <param name="hwndOwner">A value that represents the window handle of the owner window for the common dialog box.</param>
        /// <returns>true if the dialog box was successfully run; otherwise, false.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "System.PlatformNotSupportedException.#ctor(System.String)")]
        protected override bool RunDialog(IntPtr hwndOwner)
        {
            try
            {
                IDataObject DataObject = null;
                IDsObjectPicker iPicker = InitializePicker();

                if (iPicker == null)
                {
                    //Dialog error
                    return false;
                }

                iPicker.InvokeDialog(new HandleRef(this, hwndOwner), out DataObject);

                if (DataObject == null) //Cancel
                {
                    return false;   //Returns DialogResult.Cancel form ShowDialog method
                }

                SetData(DataObject);

                return true;    //Returns DialogResult.OK form ShowDialog method
            }
            catch (COMException)
            {
                if (ApplicationInfo.OSPlatform == PlatformID.Win32S || ApplicationInfo.OSPlatform == PlatformID.Win32Windows)
                {
                    throw new PlatformNotSupportedException("Windows 9x/Me is not supported");
                }
                else
                {
                    throw;
                }
            }
        }
        #endregion
    }
}
