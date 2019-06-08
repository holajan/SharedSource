using System;
using System.Runtime.InteropServices;
using System.Reflection;

namespace SystemInfo
{
    #region public types
    //From System.Runtime.InteropServices.Architecture in System.Runtime.InteropServices.RuntimeInformation.dll (System.Runtime.InteropServices.RuntimeInformation package)
    public struct OSPlatform : IEquatable<OSPlatform>
    {
        private readonly string m_OSPlatform;

        private OSPlatform(string osPlatform)
        {
            if (osPlatform == null)
            {
                throw new ArgumentNullException(nameof(osPlatform));
            }

            m_OSPlatform = osPlatform;
        }

        public static OSPlatform Windows
        {
            get { return new OSPlatform("WINDOWS"); }
        }

        public bool Equals(OSPlatform other)
        {
            return Equals(other.m_OSPlatform);
        }

        internal bool Equals(string other)
        {
            return string.Equals(m_OSPlatform, other, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is OSPlatform && Equals((OSPlatform)obj);
        }

        public override int GetHashCode()
        {
            return m_OSPlatform == null ? 0 : m_OSPlatform.GetHashCode();
        }


        public override string ToString()
        {
            return m_OSPlatform ?? string.Empty;
        }

        public static bool operator ==(OSPlatform left, OSPlatform right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OSPlatform left, OSPlatform right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// Identifies platform of windows operating system
    /// </summary>
    [Serializable]
    internal enum OSWindowsID
    {
        /// <summary>
        /// Unknown OS
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Windows 2000 (v. 5.0)
        /// </summary>
        /// <remarks>
        /// Support up to FW 2.0
        /// </remarks>
        Windows2K = 4,
        /// <summary>
        /// Windows XP or Windows Server 2003 (v. 5.1, 5.2)
        /// </summary>
        /// <remarks>
        /// Support up to FW 4.0
        /// </remarks>
        WindowsXP_2K3 = 5,
        /// <summary>
        /// Windows Vista or Windows Server 2008 (v. 6.0)
        /// </summary>
        WindowsVista_2K8 = 6,
        /// <summary>
        /// Windows 7 or Windows Server 2008 R2 (v. 6.1)
        /// </summary>
        Windows7_2K8R2 = 7,
        /// <summary>
        /// Windows 8 or Windows Server 2012 (v. 6.2)
        /// </summary>
        Windows8_2K12 = 8,
        /// <summary>
        /// Windows 10, Windows Server 10
        /// </summary>
        Windows10 = 12
    }

    /// <summary>
    /// Identifies the product type of operating system.
    /// </summary>
    [Serializable]
    internal enum ProductTypeID
    {
        /// <summary>
        /// Workstation (Client) operating system
        /// </summary>
        Workstation = 0,
        /// <summary>
        /// Server operating system
        /// </summary>
        Server = 1
    }

    //From System.Runtime.InteropServices.Architecture in System.Runtime.InteropServices.RuntimeInformation.dll (System.Runtime.InteropServices.RuntimeInformation package)
    [Serializable]
    public enum Architecture
    {
        X86 = 0,
        X64 = 1,
        Arm = 2,
        Arm64 = 3
    }
    #endregion

    /// <summary>
    /// Class gets information about platform, edition, version of operating system that are currently installed
    /// and gets concatenated string with description of operating system.
    /// </summary>
    /// <remarks>
    /// Supports .NET Framework 4.0 or later
    /// </remarks>
    internal sealed class OsVersionInfo
    {
        #region Win32 API classes
        #region OSVERSIONINFOEX struct
        /// <summary>
        /// OSVERSIONINFOEX struct contains operating system version information.
        /// The information includes major and minor version numbers, a build number, a platform identifier,
        /// and information about product suites and the latest Service Pack installed on the system.
        /// This structure is used with the <c>GetVersionEx</c> function.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct OSVERSIONINFOEX
        {
            /// <summary>
            /// dwOSVersionInfoSize
            /// </summary>
            public int dwOSVersionInfoSize;
            /// <summary>
            /// dwMajorVersion
            /// </summary>
            public int dwMajorVersion;
            /// <summary>
            /// dwMinorVersion
            /// </summary>
            public int dwMinorVersion;
            /// <summary>
            /// dwBuildNumber
            /// </summary>
            public int dwBuildNumber;
            /// <summary>
            /// dwPlatformId
            /// </summary>
            public int dwPlatformId;
            /// <summary>
            /// szCSDVersion
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szCSDVersion;
            /// <summary>
            /// wServicePackMajor
            /// </summary>
            public short wServicePackMajor;
            /// <summary>
            /// wServicePackMinor
            /// </summary>
            public short wServicePackMinor;
            /// <summary>
            /// wSuiteMask
            /// </summary>
            public ushort wSuiteMask;
            /// <summary>
            /// wProductType
            /// </summary>
            public byte wProductType;
            /// <summary>
            /// wReserved
            /// </summary>
            public byte wReserved;
        }
        #endregion

        #region OSVERSIONINFO struct
        /// <summary>
        /// OSVERSIONINFO struct contains operating system version information.
        /// The information includes major and minor version numbers, a build number, a platform identifier,
        /// and descriptive text about the operating system.
        /// This structure is used with the <c>GetVersionEx</c> function.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct OSVERSIONINFO
        {
            /// <summary>
            /// dwOSVersionInfoSize
            /// </summary>
            public int dwOSVersionInfoSize;
            /// <summary>
            /// dwMajorVersion
            /// </summary>
            public int dwMajorVersion;
            /// <summary>
            /// dwMinorVersion
            /// </summary>
            public int dwMinorVersion;
            /// <summary>
            /// dwBuildNumber
            /// </summary>
            public int dwBuildNumber;
            /// <summary>
            /// dwPlatformId
            /// </summary>
            public int dwPlatformId;
            /// <summary>
            /// szCSDVersion
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szCSDVersion;
        }
        #endregion

        #region SYSTEM_INFO struct
        /// <summary>
        /// SYSTEM_INFO struct contains information about the current computer system.
        /// This includes the architecture and type of the processor, the number of processors
        /// in the system, the page size, and other such information.
        /// This structure is used with the <c>GetSystemInfo</c> and <c>GetNativeSystemInfo</c> functions.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_INFO
        {
            /// <summary>
            /// System's processor architecture. This value can be one of the following values:
            /// PROCESSOR_ARCHITECTURE_UNKNOWN, PROCESSOR_ARCHITECTURE_INTEL, PROCESSOR_ARCHITECTURE_IA64, PROCESSOR_ARCHITECTURE_AMD64
            /// </summary>
            public short wProcessorArchitecture;
            /// <summary>
            /// Reserved for future use.
            /// </summary>
            public short wReserved;
            /// <summary>
            /// Page size and the granularity of page protection and commitment (VirtualAlloc).
            /// </summary>
            public int dwPageSize;
            /// <summary>
            /// Pointer to the lowest memory address accessible to applications and dynamic-link libraries (DLLs).
            /// </summary>
            public IntPtr lpMinimumApplicationAddress;
            /// <summary>
            /// Pointer to the highest memory address accessible to applications and DLLs.
            /// </summary>
            public IntPtr lpMaximumApplicationAddress;
            /// <summary>
            /// Mask representing the set of processors configured into the system. Bit 0 is processor 0; bit 31 is processor 31.
            /// </summary>
            public IntPtr dwActiveProcessorMask;
            /// <summary>
            /// Number of processors in the system
            /// </summary>
            public int dwNumberOfProcessors;
            /// <summary>
            /// An obsolete member that is retained for compatibility with Windows NT 3.5 and Windows Me/98/95.
            /// Use the wProcessorArchitecture, wProcessorLevel, and wProcessorRevision members to determine the type of processor.
            /// Specifies the type of processor in the system: 386, 486, 586
            /// </summary>
            public int dwProcessorType;
            /// <summary>
            /// Granularity for the starting address at which virtual memory can be allocated. For example, a VirtualAlloc request to allocate 1 byte will reserve an address space of dwAllocationGranularity bytes.
            /// </summary>
            public int dwAllocationGranularity;
            /// <summary>
            /// System's architecture-dependent processor level. It should be used only for display purposes.
            /// </summary>
            /// <remarks>
            /// If wProcessorArchitecture is PROCESSOR_ARCHITECTURE_INTEL
            ///     wProcessorLevel is defined by the CPU vendor
            ///     (following values: 3 - Intel 80386, 4 - Intel 80486, 5 - Intel Pentium, 6 - Intel Pentium Pro, Pentium II or Pentium III, 15 - Pentium 4)
            ///
            /// If wProcessorArchitecture is PROCESSOR_ARCHITECTURE_IA64
            ///     wProcessorLevel is set to 1.
            ///
            /// If wProcessorArchitecture is PROCESSOR_ARCHITECTURE_MIPS or PROCESSOR_ARCHITECTURE_ALPHA or PROCESSOR_ARCHITECTURE_PPC
            ///     wProcessorLevel is processor version number
            ///
            /// </remarks>
            public short wProcessorLevel;
            /// <summary>
            /// Architecture-dependent processor revision
            /// </summary>
            /// <remarks>
            /// The following table shows how the revision value is assembled for each type of processor architecture.
            ///
            /// Processor                | Value
            /// ----------------------------------------------------------------------------------------------
            /// Intel Pentium, Cyrix, or | The high byte is the model and the low byte is the stepping. For example,
            /// NextGen 586              | if the value is xxyy, the model number and stepping can be displayed
            ///                          } as follows: Model xx, Stepping yy
            /// ----------------------------------------------------------------------------------------------
            /// Intel 80386 or 80486     | A value of the form xxyz.
            ///                          | If xx is equal to 0xFF, y - 0xA is the model number, and z is the stepping
            ///                          | identifier. For example, an Intel 80486-D0 system returns 0xFFD0
            ///                          |
            ///                          | If xx is not equal to 0xFF, xx + 'A' is the stepping letter and yz is the
            ///                          | minor stepping.
            /// ----------------------------------------------------------------------------------------------
            /// </remarks>
            public ushort wProcessorRevision;
        }
        #endregion

        #region constants
        //wSuiteMask of OSVERSIONINFOEX struct consts
        private const ushort VER_SUITE_SMALLBUSINESS = 0x0001;   //Microsoft Small Business Server was once installed on the system, but may have been upgraded to another version of Windows. Refer to the Remarks section for more information about this bit flag.
        private const ushort VER_SUITE_ENTERPRISE = 0x0002;      //Windows Server 2008 Enterprise, Windows Server 2003, Enterprise Edition, or Windows 2000 Advanced Server is installed. Refer to the Remarks section for more information about this bit flag.
        private const ushort VER_SUITE_BACKOFFICE = 0x0004;      //Microsoft BackOffice components are installed.
        private const ushort VER_SUITE_TERMINAL = 0x0010;        //Terminal Services is installed. This value is always set.
        private const ushort VER_SUITE_SMALLBUSINESS_RESTRICTED = 0x0020;    //Microsoft Small Business Server is installed with the restrictive client license in force. Refer to the Remarks section for more information about this bit flag.
        private const ushort VER_SUITE_EMBEDDEDNT = 0x0040;      //Windows XP Embedded is installed.
        private const ushort VER_SUITE_DATACENTER = 0x0080;      //Windows Server 2008 Datacenter, Windows Server 2003, Datacenter Edition, or Windows 2000 Datacenter Server is installed.
        private const ushort VER_SUITE_SINGLEUSERTS = 0x0100;    //Remote Desktop is supported, but only one interactive session is supported. This value is set unless the system is running in application server mode.
        private const ushort VER_SUITE_PERSONAL = 0x0200;        //Windows Vista Home Premium, Windows Vista Home Basic, or Windows XP Home Edition is installed.
        private const ushort VER_SUITE_BLADE = 0x0400;           //Windows Server 2003, Web Edition is installed
        private const ushort VER_SUITE_STORAGE_SERVER = 0x2000;  //Windows Storage Server 2003 R2 or Windows Storage Server 2003is installed.
        private const ushort VER_SUITE_COMPUTE_SERVER = 0x4000;  //Windows Server 2003, Compute Cluster Edition is installed.
        private const ushort VER_SUITE_WH_SERVER = 0x8000;       //Windows Home Server is installed.

        //wProductType of OSVERSIONINFOEX struct consts
        private const byte VER_NT_WORKSTATION = 1;
        private const byte VER_NT_DOMAIN_CONTROLLER = 2;
        private const byte VER_NT_SERVER = 3;

        //GetProductInfo ReturnedProductType consts
        private const uint PRODUCT_UNDEFINED = 0x00000000;              //An unknown product
        private const uint PRODUCT_ULTIMATE = 0x00000001;               //Ultimate Edition
        private const uint PRODUCT_HOME_BASIC = 0x00000002;             //Home Basic Edition
        private const uint PRODUCT_HOME_PREMIUM = 0x00000003;           //Home Premium Edition
        private const uint PRODUCT_ENTERPRISE = 0x00000004;             //Enterprise Edition
        private const uint PRODUCT_HOME_BASIC_N = 0x00000005;           //Home Basic Edition
        private const uint PRODUCT_BUSINESS = 0x00000006;               //Business Edition
        private const uint PRODUCT_STANDARD_SERVER = 0x00000007;        //Server Standard Edition (full installation)
        private const uint PRODUCT_DATACENTER_SERVER = 0x00000008;      //Server Datacenter Edition (full installation)
        private const uint PRODUCT_SMALLBUSINESS_SERVER = 0x00000009;   //Small Business Server
        private const uint PRODUCT_ENTERPRISE_SERVER = 0x0000000A;      //Server Enterprise Edition (full installation)
        private const uint PRODUCT_STARTER = 0x0000000B;                //Starter Edition
        private const uint PRODUCT_DATACENTER_SERVER_CORE = 0x0000000C; //Server Datacenter Edition (core installation)
        private const uint PRODUCT_STANDARD_SERVER_CORE = 0x0000000D;   //Server Standard Edition (core installation)
        private const uint PRODUCT_ENTERPRISE_SERVER_CORE = 0x0000000E; //Server Enterprise Edition (core installation)
        private const uint PRODUCT_ENTERPRISE_SERVER_IA64 = 0x0000000F; //Server Enterprise Edition for Itanium-based Systems
        private const uint PRODUCT_BUSINESS_N = 0x00000010;             //Business Edition
        private const uint PRODUCT_WEB_SERVER = 0x00000011;             //Web Server Edition (full installation)
        private const uint PRODUCT_CLUSTER_SERVER = 0x00000012;         //Cluster Server Edition
        private const uint PRODUCT_HOME_SERVER = 0x00000013;            //Home Server Edition
        private const uint PRODUCT_STORAGE_EXPRESS_SERVER = 0x00000014;         //Storage Server Express Edition
        private const uint PRODUCT_STORAGE_STANDARD_SERVER = 0x00000015;        //Storage Server Standard Edition
        private const uint PRODUCT_STORAGE_WORKGROUP_SERVER = 0x00000016;       //Storage Server Workgroup Edition
        private const uint PRODUCT_STORAGE_ENTERPRISE_SERVER = 0x00000017;      //Storage Server Enterprise Edition
        private const uint PRODUCT_SERVER_FOR_SMALLBUSINESS = 0x00000018;       //Server for Small Business Edition
        private const uint PRODUCT_SMALLBUSINESS_SERVER_PREMIUM = 0x00000019;   //Small Business Server Premium Edition
        private const uint PRODUCT_HOME_PREMIUM_N = 0x0000001A;                 //Home Premium Edition
        private const uint PRODUCT_ENTERPRISE_N = 0x0000001B;                   //Enterprise Edition
        private const uint PRODUCT_ULTIMATE_N = 0x0000001C;                     //Ultimate Edition
        private const uint PRODUCT_WEB_SERVER_CORE = 0x0000001D;                //Web Server Edition (core installation)
        private const uint PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT = 0x0000001E;   //Windows Essential Business Server Management Server
        private const uint PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY = 0x0000001F;     //Windows Essential Business Server Security Server
        private const uint PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING = 0x00000020;    //Windows Essential Business Server Messaging Server
        private const uint PRODUCT_SERVER_FOUNDATION = 0x00000021;              //Server Foundation
        private const uint PRODUCT_HOME_PREMIUM_SERVER = 0x00000022;            //Windows Home Server 2011
        private const uint PRODUCT_SERVER_FOR_SMALLBUSINESS_V = 0x00000023;     //Windows Server 2008 without Hyper-V for Windows Essential Server Solutions
        private const uint PRODUCT_STANDARD_SERVER_V = 0x00000024;          //Server Standard Edition without Hyper-V (full installation)
        private const uint PRODUCT_DATACENTER_SERVER_V = 0x00000025;        //Server Datacenter Edition without Hyper-V (full installation)
        private const uint PRODUCT_ENTERPRISE_SERVER_V = 0x00000026;        //Server Enterprise Edition without Hyper-V (full installation)
        private const uint PRODUCT_DATACENTER_SERVER_CORE_V = 0x00000027;   //Server Datacenter Edition without Hyper-V (core installation)
        private const uint PRODUCT_STANDARD_SERVER_CORE_V = 0x00000028;     //Server Standard Edition without Hyper-V (core installation)
        private const uint PRODUCT_ENTERPRISE_SERVER_CORE_V = 0x00000029;   //Server Enterprise Edition without Hyper-V (core installation)
        private const uint PRODUCT_HYPERV = 0x0000002A;                     //Microsoft Hyper-V Server
        private const uint PRODUCT_STORAGE_EXPRESS_SERVER_CORE = 0x0000002B;     //Storage Server Express (core installation)
        private const uint PRODUCT_STORAGE_STANDARD_SERVER_CORE = 0x0000002C;    //Storage Server Standard (core installation)
        private const uint PRODUCT_STORAGE_WORKGROUP_SERVER_CORE = 0x0000002D;   //Storage Server Workgroup (core installation)
        private const uint PRODUCT_STORAGE_ENTERPRISE_SERVER_CORE = 0x0000002E;  //Storage Server Enterprise (core installation)
        private const uint PRODUCT_STARTER_N = 0x0000002F;                  //Starter N
        private const uint PRODUCT_PROFESSIONAL = 0x00000030;               //Professional
        private const uint PRODUCT_PROFESSIONAL_N = 0x00000031;             //Professional N
        private const uint PRODUCT_SB_SOLUTION_SERVER = 0x00000032;         //Windows Small Business Server 2011 Essentials
        private const uint PRODUCT_SERVER_FOR_SB_SOLUTIONS = 0x00000033;        //Server For SB Solutions
        private const uint PRODUCT_STANDARD_SERVER_SOLUTIONS = 0x00000034;      //Server Solutions Premium
        private const uint PRODUCT_STANDARD_SERVER_SOLUTIONS_CORE = 0x00000035; //Server Solutions Premium (core installation)
        private const uint PRODUCT_SB_SOLUTION_SERVER_EM = 0x00000036;          //Server For SB Solutions EM
        private const uint PRODUCT_SERVER_FOR_SB_SOLUTIONS_EM = 0x00000037;     //Server For SB Solutions EM
        private const uint PRODUCT_SOLUTION_EMBEDDEDSERVER = 0x00000038;    //Windows MultiPoint Server
        private const uint PRODUCT_ESSENTIALBUSINESS_SERVER_MGMT = 0x0000003B;      //Windows Essential Server Solution Management
        private const uint PRODUCT_ESSENTIALBUSINESS_SERVER_ADDL = 0x0000003C;      //Windows Essential Server Solution Additional
        private const uint PRODUCT_ESSENTIALBUSINESS_SERVER_MGMTSVC = 0x0000003D;   //Windows Essential Server Solution Management SVC
        private const uint PRODUCT_ESSENTIALBUSINESS_SERVER_ADDLSVC = 0x0000003E;   //Windows Essential Server Solution Additional SVC
        private const uint PRODUCT_SMALLBUSINESS_SERVER_PREMIUM_CORE = 0x0000003F;  //Small Business Server Premium (core installation)
        private const uint PRODUCT_CLUSTER_SERVER_V = 0x00000040;           //Server Hyper Core V
        private const uint PRODUCT_STARTER_E = 0x00000042;                  //Starter E
        private const uint PRODUCT_HOME_BASIC_E = 0x00000043;               //Home Basic E
        private const uint PRODUCT_HOME_PREMIUM_E = 0x00000044;             //Home Premium E
        private const uint PRODUCT_PROFESSIONAL_E = 0x00000045;             //Professional E
        private const uint PRODUCT_ENTERPRISE_E = 0x00000046;               //Enterprise E
        private const uint PRODUCT_ULTIMATE_E = 0x00000047;                 //Ultimate E
        private const uint PRODUCT_ENTERPRISE_EVALUATION = 0x00000048;          //Server Enterprise (evaluation installation)
        private const uint PRODUCT_DEVELOPER_PREVIEW = 0x0000004A;              //Windows 8 Developer Preview
        private const uint PRODUCT_MULTIPOINT_STANDARD_SERVER = 0x0000004C;     //Windows MultiPoint Server Standard (full installation)
        private const uint PRODUCT_MULTIPOINT_PREMIUM_SERVER = 0x0000004D;      //Windows MultiPoint Server Premium (full installation)
        private const uint PRODUCT_STANDARD_EVALUATION_SERVER = 0x0000004F;     //Server Standard (evaluation installation)
        private const uint PRODUCT_DATACENTER_EVALUATION_SERVER = 0x00000050;   //Server Datacenter (evaluation installation)
        private const uint PRODUCT_ENTERPRISE_N_EVALUATION = 0x00000054;        //Enterprise N (evaluation installation)
        private const uint PRODUCT_STORAGE_WORKGROUP_EVALUATION_SERVER = 0x0000005F;    //Storage Server Workgroup (evaluation installation)
        private const uint PRODUCT_STORAGE_STANDARD_EVALUATION_SERVER = 0x00000060;     //Storage Server Standard (evaluation installation)
        private const uint PRODUCT_CORE_N = 0x00000062;                     //Windows 8 N
        private const uint PRODUCT_CORE_COUNTRYSPECIFIC = 0x00000063;       //Windows 8 China
        private const uint PRODUCT_CORE_SINGLELANGUAGE = 0x00000064;        //Windows 8 Single Language
        private const uint PRODUCT_CORE = 0x00000065;                       //Windows 8
        private const uint PRODUCT_PROFESSIONAL_WMC = 0x00000067;           //Professional with Media Center
        private const uint PRODUCT_UNLICENSED = 0xABCDABCD; //Product has not been activated and is no longer in the grace period

        //GetSystemMetrics SO SpecialEdition info consts
        private const int SM_TABLETPC = 86;         //Windows XP Tablet PC edition
        private const int SM_MEDIACENTER = 87;      //Windows XP, Media Center Edition
        private const int SM_STARTER = 88;          //Windows Server 2003 R2 version
        private const int SM_SERVERR2 = 89;         //Windows XP Starter Edition or Windows Vista Starter Edition

        // ProcessorArchitecture of SYSTEM_INFO struct consts
        private const short PROCESSOR_ARCHITECTURE_UNKNOWN = -1;
        private const short PROCESSOR_ARCHITECTURE_INTEL = 0;
        private const short PROCESSOR_ARCHITECTURE_MIPS = 1;
        private const short PROCESSOR_ARCHITECTURE_ALPHA = 2;
        private const short PROCESSOR_ARCHITECTURE_PPC = 3;
        private const short PROCESSOR_ARCHITECTURE_SHX = 4;
        private const short PROCESSOR_ARCHITECTURE_ARM = 5;
        private const short PROCESSOR_ARCHITECTURE_IA64 = 6;
        private const short PROCESSOR_ARCHITECTURE_ALPHA64 = 7;
        private const short PROCESSOR_ARCHITECTURE_MSIL = 8;
        private const short PROCESSOR_ARCHITECTURE_AMD64 = 9;
        private const short PROCESSOR_ARCHITECTURE_IA32_ON_WIN64 = 10;
        private const short PROCESSOR_ARCHITECTURE_ARM64 = 12;
        #endregion

        #region SafeNativeMethods class
        [System.Security.SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods
        {
            [DllImport("ntdll.dll")]
            internal static extern int RtlGetVersion(ref OSVERSIONINFOEX lpVersionInformation);

            /// <summary>
            /// Retrieves information about the current operating system.
            /// </summary>
            [DllImport("kernel32", CharSet = CharSet.Auto)]
            internal static extern int GetVersionEx(ref OSVERSIONINFOEX osVersionInfo);

            /// <summary>
            /// Retrieves the product type for the operating system on the local computer, and maps the type to the product types supported by the specified operating system.
            /// </summary>
            [DllImport("Kernel32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetProductInfo(int dwOSMajorVersion, int dwOSMinorVersion, int dwSpMajorVersion, int dwSpMinorVersion, out uint pdwReturnedProductType);

            /// <summary>
            /// Retrieves information about the current system.
            /// </summary>
            [DllImport("kernel32.dll", EntryPoint = "GetSystemInfo")]
            internal static extern void GetSystemInfo(ref SYSTEM_INFO sysinfo);

            /// <summary>
            /// Retrieves information about the current system to an application running under WOW64. If the function is called from a 64-bit application, it is equivalent to the GetSystemInfo function.
            /// </summary>
            [DllImport("kernel32.dll", EntryPoint = "GetNativeSystemInfo")]
            internal static extern void GetNativeSystemInfo(ref SYSTEM_INFO sysinfo);

            /// <summary>
            /// Retrieves the specified system metric or system configuration setting.
            /// </summary>
            [DllImport("user32.dll", EntryPoint = ("GetSystemMetrics"))]
            internal static extern int GetSystemMetrics(int nIndex);

            /// <summary>
            /// Retrieves a module handle for the specified module. The module must have been loaded by the calling process.
            /// </summary>
            [DllImport("kernel32.dll", BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
            internal static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPStr)] string lpModuleName);

            /// <summary>
            /// Retrieves the address of an exported function or variable from the specified dynamic-link library (DLL).
            /// </summary>
            [DllImport("kernel32.dll", BestFitMapping = false, ThrowOnUnmappableChar = true)]
            internal static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);
        }
        #endregion
        #endregion

        #region member varible and default property initialization
        private short m_ProcessorArchitecture;
        private OSVERSIONINFOEX m_WindowsOSVersionInfo;
        private uint m_WindowsProductInfoProductType;
        private string m_WindowsServicePack;
        private int m_WindowsCSDBuildNumber;

        private OSPlatform m_OSPlatform;
        private string m_OSName;
        private string m_OSFullName;
        private OSWindowsID m_OSWindows;

        private string m_NETFrameworkVersionString;
        #endregion

        #region constructors and destructors
        /// <summary>
        /// OsVersionInfo constructor loads operation system informations.
        /// </summary>
        /// <exception cref="NotSupportedException">Cannot get system informations.</exception>
        public OsVersionInfo()
        {
            SetVersionInformation();
        }
        #endregion

        #region action methods
        /// <summary>
        /// Gets the concatenated string representation of the platform with edition, service pack and build number of operating system that are currently installed.
        /// </summary>
        /// <returns>concatenated string representation of the platform with edition, service pack and build number of operating system that are currently installed</returns>
        public override string ToString()
        {
            return this.VersionString;
        }

        /// <summary>
        /// Gets the concatenated string representation of the platform with edition, service pack and build number of operating system that are currently installed.
        /// </summary>
        /// <returns>concatenated string representation of the platform with edition, service pack and build number of operating system that are currently installed</returns>
        public static string GetVersionString()
        {
            return new OsVersionInfo().VersionString;
        }
        #endregion

        #region property getters/setters
        #region OS Information
        /// <summary>
        /// Operating system or platform currently installed.
        /// </summary>
        public OSPlatform OSPlatform
        {
            get { return m_OSPlatform; }
        }

        /// <summary>
        /// Gets the string representation of the platform of operating system that are currently installed.
        /// </summary>
        public string OSName
        {
            get { return m_OSName; }
        }

        /// <summary>
        /// Gets the string representation of the platform with edition of operating system that are currently installed.
        /// </summary>
        public string OSFullName
        {
            get { return m_OSFullName; }
        }

        /// <summary>
        /// Gets the platform identifier of windows operating system that are currently installed.
        /// </summary>
        public OSWindowsID OSWindows
        {
            get { return m_OSWindows; }
        }

        /// <summary>
        /// Version of operating system that are currently installed.
        /// </summary>
        /// <remarks>
        /// Revision number of version is not set, use only major, minor a build numbers
        /// </remarks>
        public Version OSVersion
        {
            get
            {
                if (this.OSPlatform == OSPlatform.Windows)
                {
                    return new Version(m_WindowsOSVersionInfo.dwMajorVersion, m_WindowsOSVersionInfo.dwMinorVersion, m_WindowsOSVersionInfo.dwBuildNumber, 0);
                }

                return new Version(0, 0, 0);
            }
        }

        /// <summary>
        /// Build number of operating system that are currently installed.
        /// </summary>
        public int OSBuild
        {
            get { return this.OSVersion.Build; }
        }

        /// <summary>
        /// Operating system architecture version.
        /// </summary>
        public Architecture OSArchitecture
        {
            get
            {
                switch (m_ProcessorArchitecture)
                {
                    case PROCESSOR_ARCHITECTURE_ARM64:
                        return Architecture.Arm64;
                    case PROCESSOR_ARCHITECTURE_ARM:
                        return Architecture.Arm;
                    case PROCESSOR_ARCHITECTURE_AMD64:
                    case PROCESSOR_ARCHITECTURE_IA64:
                        return Architecture.X64;
                    case PROCESSOR_ARCHITECTURE_INTEL:
                        return Architecture.X86;
                }

                return Architecture.X86;
            }
        }

        /// <summary>
        /// Product type of operating system that are currently installed.
        /// </summary>
        public ProductTypeID WindowsProductType
        {
            get
            {
                if (m_WindowsOSVersionInfo.wProductType == VER_NT_SERVER ||
                    m_WindowsOSVersionInfo.wProductType == VER_NT_DOMAIN_CONTROLLER)
                {
                    return ProductTypeID.Server;
                }

                return ProductTypeID.Workstation;
            }
        }

        /// <summary>
        /// Service pack that are currently installed on the operating system.
        /// </summary>
        public string WindowsServicePack
        {
            get { return m_WindowsServicePack; }
        }

        /// <summary>
        /// Version of service pack that are currently installed on the operating system.
        /// </summary>
        public Version WindowsServicePackVersion
        {
            get
            {
                return new Version(m_WindowsOSVersionInfo.wServicePackMajor, m_WindowsOSVersionInfo.wServicePackMinor);
            }
        }

        /// <summary>
        /// Service pack CSDBuildNumber on the operating system.
        /// </summary>
        public int WindowsCSDBuildNumber
        {
            get { return m_WindowsCSDBuildNumber; }
        }

        /// <summary>
        /// BuildLab version string of operating system that are currently installed.
        /// </summary>
        public string WindowsBuildLab
        {
            get
            {
                if (this.OSPlatform != OSPlatform.Windows)
                {
                    return null;
                }

                if (Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion") == null)
                {
                    return null;
                }

                string buildLab = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                            "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion").GetValue("BuildLabEx") as string;

                if (buildLab == null)
                {
                    buildLab = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                                "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion").GetValue("BuildLab") as string;
                }

                return buildLab;
            }
        }

        /// <summary>
        /// Gets the concatenated string representation of the platform with edition, service pack and build number of operating system that are currently installed
        /// </summary>
        public string VersionString
        {
            get
            {
                if (this.OSPlatform == OSPlatform.Windows)
                {
                    //Windows 2000, Windows XP, Windows Vista or later
                    if (string.IsNullOrEmpty(m_WindowsServicePack))
                    {
                        return m_OSFullName + " (Build " + m_WindowsOSVersionInfo.dwBuildNumber.ToString(System.Globalization.CultureInfo.CurrentCulture) + ")";
                    }

                    return m_OSFullName + " " + m_WindowsServicePack + " (Build " + m_WindowsOSVersionInfo.dwBuildNumber.ToString(System.Globalization.CultureInfo.CurrentCulture) + ")";
                }

                return m_OSFullName;
            }
        }

        /// <summary>
        /// Core installation of Windows Server 2008 or later operating system
        /// </summary>
        public bool WindowsCoreInstallation
        {
            get
            {
                switch (m_WindowsProductInfoProductType)
                {
                    case PRODUCT_WEB_SERVER_CORE:
                    case PRODUCT_STANDARD_SERVER_CORE:
                    case PRODUCT_DATACENTER_SERVER_CORE:
                    case PRODUCT_ENTERPRISE_SERVER_CORE:
                    case PRODUCT_STANDARD_SERVER_CORE_V:
                    case PRODUCT_DATACENTER_SERVER_CORE_V:
                    case PRODUCT_ENTERPRISE_SERVER_CORE_V:
                    case PRODUCT_HYPERV:
                    case PRODUCT_STORAGE_EXPRESS_SERVER_CORE:
                    case PRODUCT_STORAGE_STANDARD_SERVER_CORE:
                    case PRODUCT_STORAGE_WORKGROUP_SERVER_CORE:
                    case PRODUCT_STORAGE_ENTERPRISE_SERVER_CORE:
                    case PRODUCT_STANDARD_SERVER_SOLUTIONS_CORE:
                    case PRODUCT_SMALLBUSINESS_SERVER_PREMIUM_CORE:
                        return true;
                }

                return false;
            }
        }
        #endregion

        #region CLR information
        public string Runtime
        {
            get { return "CLR"; }
        }

        public string RuntimeDisplayName
        {
            get { return ".NET Framework"; }
        }

        /// <summary>
        /// Version (the major, minor, build, and revision numbers) of the common language runtime.
        /// </summary>
        public Version CLRVersion
        {
            get { return System.Environment.Version; }
        }

        public string RuntimeFramework
        {
            get
            {
                return ".NETFramework,Version=v" + NETFrameworkVersionString;
            }
        }

        /// <summary>
        /// Version of .NET framework that are currently installed, e.g.: "4.5".
        /// </summary>
        public string NETFrameworkVersionString
        {
            get
            {
                if (m_NETFrameworkVersionString == null)
                {
                    m_NETFrameworkVersionString = GetNETFrameworkVersionString();
                }

                return m_NETFrameworkVersionString;
            }
        }
        #endregion
        #endregion

        #region private member functions
        /// <summary>
        /// Set OS version information to m_OSName, m_OSFullName, m_OSWindows
        /// </summary>
        private void SetVersionInformation()
        {
            //Get ProcessorArchitecture from SYSTEM_INFO struct
            m_ProcessorArchitecture = GetProcessorArchitecture();

            m_OSPlatform = OSPlatform.Windows;

            //Get OSVERSIONINFOEX struct
            m_WindowsOSVersionInfo = GetWindowsOSVersionInfo();

            if (m_WindowsOSVersionInfo.dwMajorVersion >= 6)
            {
                //Get ProductInfo ProductType
                uint dwType;
                if (SafeNativeMethods.GetProductInfo(m_WindowsOSVersionInfo.dwMajorVersion, m_WindowsOSVersionInfo.dwMinorVersion, 0, 0, out dwType))
                {
                    m_WindowsProductInfoProductType = dwType;
                }
            }

            SetWindowsPlatformInformation();

            //Set m_WindowsServicePack and m_WindowsCSDBuildNumber
            SetWindowsServicePackString();
        }

        /// <summary>
        /// Get ProcessorArchitecture from SYSTEM_INFO struct
        /// </summary>
        /// <returns><c>short</c> ProcessorArchitecture</returns>
        private static short GetProcessorArchitecture()
        {
            var si = new SYSTEM_INFO();
            if (SafeNativeMethods.GetProcAddress(SafeNativeMethods.GetModuleHandle("KERNEL32.dll"), "GetNativeSystemInfo") != (IntPtr)0)
            {
                //On Windows XP or later
                SafeNativeMethods.GetNativeSystemInfo(ref si);
                return si.wProcessorArchitecture;
            }

            SafeNativeMethods.GetSystemInfo(ref si);
            return si.wProcessorArchitecture;
        }

        /// <summary>
        /// Get OSVERSIONINFOEX struct
        /// </summary>
        /// <returns>OSVERSIONINFOEX struct</returns>
        /// <exception cref="NotSupportedException">Cannot get OSVERSIONINFO struct.</exception>
        private static OSVERSIONINFOEX GetWindowsOSVersionInfo()
        {
            var osVersionInfo = new OSVERSIONINFOEX();

            //Get Win32 API extended OSVERSIONINFOEX struct
            osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(new OSVERSIONINFOEX());

            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6)
            {
                //Try use RtlGetVersion function because GetOSVersionEx API gives wrong result on Windows 8.1 and Windows 10 (without applications manifested compatibility keys)
                int rtlResult = SafeNativeMethods.RtlGetVersion(ref osVersionInfo);
                if (rtlResult == 0)
                {
                    return osVersionInfo;
                }
            }

            int result = SafeNativeMethods.GetVersionEx(ref osVersionInfo);
            if (result == 0)
            {
                //Get only OSVERSIONINFO struct
                osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(new OSVERSIONINFO());
                result = SafeNativeMethods.GetVersionEx(ref osVersionInfo);

                if (result == 0)
                {
                    throw new NotSupportedException();
                }
            }

            return osVersionInfo;
        }

        /// <summary>
        /// Set Windows Platform information to m_OSName, m_OSFullName, m_OSWindows
        /// </summary>
        private void SetWindowsPlatformInformation()
        {
            m_OSPlatform = OSPlatform.Windows;

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetWindowsInformation();
            }
            else if (Environment.OSVersion.Platform == PlatformID.WinCE) //Specifies the Windows CE OS.
            {
                m_OSName = "Windows CE";
                m_OSFullName = "Windows CE";
            }
            else
            {
                m_OSName = Environment.OSVersion.Platform.ToString();
                m_OSFullName = Environment.OSVersion.Platform.ToString();
            }
        }

        /// <summary>
        /// Windows Information - Windows 2000, Windows XP, Windows Vista, Windows 7 and later
        /// </summary>
        private void SetWindowsInformation()
        {
            switch (m_WindowsOSVersionInfo.dwMajorVersion)
            {
                case 5:
                    if (m_WindowsOSVersionInfo.dwMinorVersion == 0)
                    {
                        SetWin2KInformation(); //Windows 2000
                    }
                    else if (m_WindowsOSVersionInfo.dwMinorVersion == 1)
                    {
                        SetWinXPInformation(); //Windows XP
                    }
                    else
                    {
                        SetWin2K3Information(); //Windows Server 2003 or Windows XP Professional x64 Edition
                    }
                    break;
                case 6:
                    if (m_WindowsOSVersionInfo.dwMinorVersion == 0)
                    {
                        SetWinVistaWin2K8Information(); //Windows Vista or Windows Server 2008
                    }
                    else if (m_WindowsOSVersionInfo.dwMinorVersion == 1)
                    {
                        SetWin7Win2K8R2Information();   //Windows 7 or Windows Server 2008 R2
                    }
                    else if (m_WindowsOSVersionInfo.dwMinorVersion == 2 || m_WindowsOSVersionInfo.dwMinorVersion == 3)
                    {
                        SetWin8Win2012Information();    //Windows 8 or Windows Server 2012
                    }
                    else if (m_WindowsOSVersionInfo.dwMinorVersion == 4)
                    {
                        SetWin10Information();          //Windows 10, Windows Server 10
                    }
                    break;
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                    SetWin10Information();  //Windows 10, Windows Server 10 (Major 10, Minor 0)
                    break;
            }
        }

        #region Windows operating system informations medhods
        /// <summary>
        /// Windows 2000 operating system informations
        /// </summary>
        private void SetWin2KInformation()
        {
            m_OSName = "Windows 2000";
            m_OSFullName = "Windows 2000";
            m_OSWindows = OSWindowsID.Windows2K;

            if (m_WindowsOSVersionInfo.wProductType == VER_NT_WORKSTATION)
            {
                if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_PERSONAL) == VER_SUITE_PERSONAL)
                {
                    m_OSFullName += " Personal";
                }
                else
                {
                    m_OSFullName += " Professional";
                }
            }
            else if (m_WindowsOSVersionInfo.wProductType == VER_NT_SERVER ||
                     m_WindowsOSVersionInfo.wProductType == VER_NT_DOMAIN_CONTROLLER)
            {
                if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_DATACENTER) == VER_SUITE_DATACENTER)
                {
                    m_OSFullName += " Datacenter Server";
                }
                else if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
                {
                    m_OSFullName += " Advanced Server";
                }
                else if (((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_SMALLBUSINESS) == VER_SUITE_SMALLBUSINESS) ||
                        ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_SMALLBUSINESS_RESTRICTED) == VER_SUITE_SMALLBUSINESS_RESTRICTED))
                {
                    m_OSFullName += " Small Business Server";
                }
                else
                {
                    m_OSFullName += " Server";
                }
            }
        }

        /// <summary>
        /// Windows XP (32-bit) operating system informations
        /// </summary>
        private void SetWinXPInformation()
        {
            m_OSName = "Windows XP";
            m_OSFullName = "Windows XP";
            m_OSWindows = OSWindowsID.WindowsXP_2K3;

            if (SafeNativeMethods.GetSystemMetrics(SM_MEDIACENTER) != 0)
            {
                m_OSFullName += " Media Center Edition";
            }
            else if (SafeNativeMethods.GetSystemMetrics(SM_STARTER) != 0)
            {
                m_OSFullName += " Starter Edition";
            }
            else if (SafeNativeMethods.GetSystemMetrics(SM_TABLETPC) != 0)
            {
                m_OSFullName += " Tablet PC Edition";
            }
            if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_PERSONAL) == VER_SUITE_PERSONAL)
            {
                m_OSFullName += " Home Edition";
            }
            else
            {
                m_OSFullName += " Professional";
            }
        }

        /// <summary>
        /// Windows Server 2003 or Windows XP Professional x64 Edition operating system informations
        /// </summary>
        private void SetWin2K3Information()
        {
            m_OSWindows = OSWindowsID.WindowsXP_2K3;

            if (SafeNativeMethods.GetSystemMetrics(SM_SERVERR2) != 0)
            {
                m_OSName = "Windows Server 2003";
                m_OSFullName = "Windows Server 2003 R2";
            }
            else if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_STORAGE_SERVER) == VER_SUITE_STORAGE_SERVER)
            {
                m_OSName = "Windows Storage Server 2003";
                m_OSFullName = "Windows Storage Server 2003";
            }
            else if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_WH_SERVER) == VER_SUITE_WH_SERVER)
            {
                m_OSName = "Windows Home Server";
                m_OSFullName = "Windows Home Server";
            }
            else if (m_WindowsOSVersionInfo.wProductType == VER_NT_WORKSTATION && m_ProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64)
            {
                m_OSName = "Windows XP x64";
                m_OSFullName = "Windows XP Professional x64 Edition";
            }
            else
            {
                m_OSName = "Windows Server 2003";
                m_OSFullName = "Windows Server 2003";
            }

            if ((m_WindowsOSVersionInfo.wProductType == VER_NT_SERVER ||
                 m_WindowsOSVersionInfo.wProductType == VER_NT_DOMAIN_CONTROLLER) &&
                (m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_WH_SERVER) != VER_SUITE_WH_SERVER)
            {
                if (m_ProcessorArchitecture == PROCESSOR_ARCHITECTURE_IA64)
                {
                    if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_DATACENTER) == VER_SUITE_DATACENTER)
                    {
                        m_OSFullName += " Datacenter Edition for Itanium-based Systems";
                    }
                    else if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
                    {
                        m_OSFullName += " Enterprise Edition for Itanium-based Systems";
                    }
                }
                else if (m_ProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64)
                {
                    if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_DATACENTER) == VER_SUITE_DATACENTER)
                    {
                        m_OSFullName += " Datacenter x64 Edition";
                    }
                    else if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
                    {
                        m_OSFullName += " Enterprise x64 Edition";
                    }
                    else
                    {
                        m_OSFullName += " Standard x64 Edition";
                    }
                }
                else
                {
                    if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_COMPUTE_SERVER) == VER_SUITE_COMPUTE_SERVER)
                    {
                        m_OSFullName += " Compute Cluster Edition";
                    }
                    else if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_DATACENTER) == VER_SUITE_DATACENTER)
                    {
                        m_OSFullName += " Datacenter Edition";
                    }
                    else if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
                    {
                        m_OSFullName += " Enterprise Edition";
                    }
                    else if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_BLADE) == VER_SUITE_BLADE)
                    {
                        m_OSFullName += " Web Edition";
                    }
                    else if (((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_SMALLBUSINESS) == VER_SUITE_SMALLBUSINESS) ||
                            ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_SMALLBUSINESS_RESTRICTED) == VER_SUITE_SMALLBUSINESS_RESTRICTED))
                    {
                        m_OSFullName += " for Small Business Server";
                    }
                    else
                    {
                        m_OSFullName += " Standard Edition";
                    }
                }
            }
        }

        /// <summary>
        /// Windows Vista or Windows Server 2008 operating system informations
        /// </summary>
        private void SetWinVistaWin2K8Information()
        {
            if (m_WindowsOSVersionInfo.wProductType == VER_NT_WORKSTATION)
            {
                SetWinVistaInformation();
            }
            else
            {
                SetWin2K8Information();
            }
        }

        /// <summary>
        /// Windows Vista operating system informations
        /// </summary>
        private void SetWinVistaInformation()
        {
            m_OSName = "Windows Vista";
            m_OSFullName = "Windows Vista";
            m_OSWindows = OSWindowsID.WindowsVista_2K8;

            #region ProductInfoProductType switch
            switch (m_WindowsProductInfoProductType)
            {
                case PRODUCT_HOME_BASIC:
                    m_OSFullName += " Home Basic";
                    return;
                case PRODUCT_HOME_BASIC_N:
                    m_OSFullName += " Home Basic N";
                    return;
                case PRODUCT_HOME_PREMIUM:
                    m_OSFullName += " Home Premium";
                    return;
                case PRODUCT_HOME_PREMIUM_N:
                    m_OSFullName += " Home Premium N";
                    return;
                case PRODUCT_STARTER:
                    m_OSFullName += " Starter";
                    return;
                case PRODUCT_STARTER_N:
                    m_OSFullName += " Starter N";
                    return;
                case PRODUCT_STARTER_E:
                    m_OSFullName += " Starter E";
                    return;
                case PRODUCT_ENTERPRISE:
                    m_OSFullName += " Enterprise";
                    return;
                case PRODUCT_ENTERPRISE_N:
                    m_OSFullName += " Enterprise N";
                    return;
                case PRODUCT_BUSINESS:
                    m_OSFullName += " Business";
                    return;
                case PRODUCT_BUSINESS_N:
                    m_OSFullName += " Business N";
                    return;
                case PRODUCT_ULTIMATE:
                    m_OSFullName += " Ultimate";
                    return;
                case PRODUCT_ULTIMATE_N:
                    m_OSFullName += " Ultimate N";
                    return;
            }
            #endregion

            //Unlicensed or unknown
            if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_PERSONAL) == VER_SUITE_PERSONAL)
            {
                //Windows Vista Home Premium or Windows Vista Home Basic
                if (SafeNativeMethods.GetSystemMetrics(SM_MEDIACENTER) != 0)
                {
                    m_OSFullName += " Home Premium";
                }
                else
                {
                    m_OSFullName += " Home Basic";
                }
            }
            else
            {
                if (SafeNativeMethods.GetSystemMetrics(SM_STARTER) != 0)
                {
                    m_OSFullName += " Starter";
                }
                else
                {
                    if (SafeNativeMethods.GetSystemMetrics(SM_MEDIACENTER) != 0)
                    {
                        m_OSFullName += " Ultimate";
                    }
                    else
                    {
                        if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
                        {
                            m_OSFullName += " Enterprise";
                        }
                        else
                        {
                            m_OSFullName += " Business";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Windows Server 2008 operating system informations
        /// </summary>
        private void SetWin2K8Information()
        {
            m_OSName = "Windows Server 2008";
            m_OSFullName = "Windows Server 2008";
            m_OSWindows = OSWindowsID.WindowsVista_2K8;

            if (m_ProcessorArchitecture == PROCESSOR_ARCHITECTURE_IA64)
            {
                m_OSFullName = "Windows Server 2008 for Itanium-based Systems";
                return;
            }

            #region ProductInfoProductType switch
            switch (m_WindowsProductInfoProductType)
            {
                case PRODUCT_STANDARD_SERVER:
                    m_OSFullName = "Windows Server 2008 Standard";
                    return;
                case PRODUCT_STANDARD_SERVER_CORE:
                    m_OSFullName = "Windows Server 2008 Standard (core installation)";
                    return;
                case PRODUCT_STANDARD_SERVER_V:
                    m_OSFullName = "Windows Server 2008 Standard without Hyper-V";
                    return;
                case PRODUCT_STANDARD_SERVER_CORE_V:
                    m_OSFullName = "Windows Server 2008 Standard without Hyper-V (core installation)";
                    return;
                case PRODUCT_ENTERPRISE:
                case PRODUCT_ENTERPRISE_N:
                case PRODUCT_ENTERPRISE_SERVER:
                    m_OSFullName = "Windows Server 2008 Enterprise";
                    return;
                case PRODUCT_ENTERPRISE_SERVER_CORE:
                    m_OSFullName = "Windows Server 2008 Enterprise (core installation)";
                    return;
                case PRODUCT_ENTERPRISE_SERVER_V:
                    m_OSFullName = "Windows Server 2008 Enterprise without Hyper-V";
                    return;
                case PRODUCT_ENTERPRISE_SERVER_CORE_V:
                    m_OSFullName = "Windows Server 2008 Enterprise without Hyper-V (core installation)";
                    return;
                case PRODUCT_DATACENTER_SERVER:
                    m_OSFullName = "Windows Server 2008 Datacenter";
                    return;
                case PRODUCT_DATACENTER_SERVER_CORE:
                    m_OSFullName = "Windows Server 2008 Datacenter (core installation)";
                    return;
                case PRODUCT_DATACENTER_SERVER_V:
                    m_OSFullName = "Windows Server 2008 Datacenter without Hyper-V";
                    return;
                case PRODUCT_DATACENTER_SERVER_CORE_V:
                    m_OSFullName = "Windows Server 2008 Datacenter without Hyper-V (core installation)";
                    return;
                case PRODUCT_WEB_SERVER:
                    m_OSName = "Windows Web Server 2008";
                    m_OSFullName = "Windows Web Server 2008";
                    return;
                case PRODUCT_WEB_SERVER_CORE:
                    m_OSName = "Windows Web Server 2008";
                    m_OSFullName = "Windows Web Server 2008 (core installation)";
                    return;
                case PRODUCT_ENTERPRISE_SERVER_IA64:
                    m_OSFullName = "Windows Server 2008 for Itanium-based Systems";
                    return;
                case PRODUCT_SMALLBUSINESS_SERVER:
                case PRODUCT_SERVER_FOR_SMALLBUSINESS:
                    m_OSName = "Windows Small Business Server 2008";
                    m_OSFullName = "Windows Small Business Server 2008";
                    return;
                case PRODUCT_SERVER_FOR_SMALLBUSINESS_V:
                    m_OSName = "Windows Small Business Server 2008";
                    m_OSFullName = "Windows Small Business Server 2008 without Hyper-V";
                    return;
                case PRODUCT_SMALLBUSINESS_SERVER_PREMIUM:
                    m_OSName = "Windows Small Business Server 2008";
                    m_OSFullName = "Windows Small Business Server 2008 Premium";
                    return;
                case PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT:
                case PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY:
                case PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING:
                    m_OSName = "Windows Essential Business Server 2008";
                    m_OSFullName = "Windows Essential Business Server 2008";
                    return;
                case PRODUCT_STARTER:
                case PRODUCT_STARTER_N:
                case PRODUCT_STARTER_E:
                    m_OSFullName = "Windows Server 2008 Starter Edition";
                    return;
                case PRODUCT_CLUSTER_SERVER:
                    m_OSFullName = "Windows HPC Server 2008";
                    return;
                case PRODUCT_HOME_SERVER:
                    m_OSName = "Windows Home Server";
                    m_OSFullName = "Windows Home Server";
                    return;
                case PRODUCT_STORAGE_EXPRESS_SERVER:
                    m_OSName = "Windows Storage Server 2008";
                    m_OSFullName = "Windows Storage Server 2008 Express";
                    return;
                case PRODUCT_STORAGE_STANDARD_SERVER:
                    m_OSName = "Windows Storage Server 2008";
                    m_OSFullName = "Windows Storage Server 2008";
                    return;
                case PRODUCT_STORAGE_WORKGROUP_SERVER:
                    m_OSName = "Windows Storage Server 2008";
                    m_OSFullName = "Windows Storage Server 2008 Workgroup";
                    return;
                case PRODUCT_STORAGE_ENTERPRISE_SERVER:
                    m_OSName = "Windows Storage Server 2008";
                    m_OSFullName = "Windows Storage Server 2008 Enterprise";
                    return;
                case PRODUCT_HYPERV:
                    m_OSName = "Hyper-V Server 2008";
                    m_OSFullName = "Microsoft Hyper-V Server 2008";
                    return;
            }
            #endregion

            //Unlicensed or unknown
            if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_COMPUTE_SERVER) == VER_SUITE_COMPUTE_SERVER)
            {
                m_OSFullName = "Windows HPC Server 2008";
            }
            else if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_STORAGE_SERVER) == VER_SUITE_STORAGE_SERVER)
            {
                m_OSName = "Windows Storage Server 2008";
                m_OSFullName = "Windows Storage Server 2008";
            }
            else if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_WH_SERVER) == VER_SUITE_WH_SERVER)
            {
                m_OSName = "Windows Home Server";
                m_OSFullName = "Windows Home Server";
            }
            else if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_DATACENTER) == VER_SUITE_DATACENTER)
            {
                m_OSFullName = "Windows Server 2008 Datacenter";
            }
            else if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
            {
                m_OSFullName = "Windows Server 2008 Enterprise";
            }
            else if ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_BLADE) == VER_SUITE_BLADE)
            {
                m_OSName = "Windows Web Server 2008";
                m_OSFullName = "Windows Web Server 2008";
            }
            else if (((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_SMALLBUSINESS) == VER_SUITE_SMALLBUSINESS) ||
                    ((m_WindowsOSVersionInfo.wSuiteMask & VER_SUITE_SMALLBUSINESS_RESTRICTED) == VER_SUITE_SMALLBUSINESS_RESTRICTED))
            {
                m_OSName = "Windows Small Business Server 2008";
                m_OSFullName = "Windows Small Business Server 2008";
            }
            else
            {
                m_OSFullName = "Windows Server 2008 Standard";
            }
        }

        /// <summary>
        /// Windows 7 or Windows Server 2008 R2 or Windows Small Business Server 2011 operating system informations
        /// </summary>
        private void SetWin7Win2K8R2Information()
        {
            m_OSWindows = OSWindowsID.Windows7_2K8R2;

            if (m_WindowsOSVersionInfo.wProductType == VER_NT_WORKSTATION)
            {
                m_OSName = "Windows 7";
                m_OSFullName = "Windows 7";

                #region ProductInfoProductType switch
                switch (m_WindowsProductInfoProductType)
                {
                    case PRODUCT_HOME_BASIC:
                        m_OSFullName += " Home Basic";
                        return;
                    case PRODUCT_HOME_BASIC_N:
                        m_OSFullName += " Home Basic N";
                        return;
                    case PRODUCT_HOME_BASIC_E:
                        m_OSFullName += " Home Basic E";
                        return;
                    case PRODUCT_HOME_PREMIUM:
                        m_OSFullName += " Home Premium";
                        return;
                    case PRODUCT_HOME_PREMIUM_N:
                        m_OSFullName += " Home Premium N";
                        return;
                    case PRODUCT_HOME_PREMIUM_E:
                        m_OSFullName += " Home Premium E";
                        return;
                    case PRODUCT_STARTER:
                        m_OSFullName += " Starter Edition";
                        return;
                    case PRODUCT_STARTER_N:
                        m_OSFullName += " Starter N Edition";
                        return;
                    case PRODUCT_STARTER_E:
                        m_OSFullName += " Starter E Edition";
                        return;
                    case PRODUCT_ENTERPRISE:
                        m_OSFullName += " Enterprise";
                        return;
                    case PRODUCT_ENTERPRISE_N:
                        m_OSFullName += " Enterprise N";
                        return;
                    case PRODUCT_ENTERPRISE_E:
                        m_OSFullName += " Enterprise E";
                        return;
                    case PRODUCT_BUSINESS:
                    case PRODUCT_PROFESSIONAL:
                        m_OSFullName += " Professional";
                        return;
                    case PRODUCT_BUSINESS_N:
                    case PRODUCT_PROFESSIONAL_N:
                        m_OSFullName += " Professional N";
                        return;
                    case PRODUCT_PROFESSIONAL_E:
                        m_OSFullName += " Professional E";
                        return;
                    case PRODUCT_ULTIMATE:
                        m_OSFullName += " Ultimate";
                        return;
                    case PRODUCT_ULTIMATE_N:
                        m_OSFullName += " Ultimate N";
                        return;
                    case PRODUCT_ULTIMATE_E:
                        m_OSFullName += " Ultimate E";
                        return;
                }
                #endregion
            }
            else
            {
                m_OSName = "Windows Server 2008 R2";
                m_OSFullName = "Windows Server 2008 R2";

                if (m_ProcessorArchitecture == PROCESSOR_ARCHITECTURE_IA64)
                {
                    m_OSFullName = "Windows Server 2008 R2 for Itanium-based Systems";
                    return;
                }

                #region ProductInfoProductType switch
                switch (m_WindowsProductInfoProductType)
                {
                    case PRODUCT_STANDARD_SERVER:
                        m_OSFullName = "Windows Server 2008 R2 Standard";
                        return;
                    case PRODUCT_STANDARD_SERVER_CORE:
                        m_OSFullName = "Windows Server 2008 R2 Standard (core installation)";
                        return;
                    case PRODUCT_STANDARD_SERVER_V:
                        m_OSFullName = "Windows Server 2008 R2 Standard without Hyper-V";
                        return;
                    case PRODUCT_STANDARD_SERVER_CORE_V:
                        m_OSFullName = "Windows Server 2008 R2 Standard without Hyper-V (core installation)";
                        return;
                    case PRODUCT_ENTERPRISE:
                    case PRODUCT_ENTERPRISE_N:
                    case PRODUCT_ENTERPRISE_SERVER:
                        m_OSFullName = "Windows Server 2008 R2 Enterprise";
                        return;
                    case PRODUCT_ENTERPRISE_SERVER_CORE:
                        m_OSFullName = "Windows Server 2008 R2 Enterprise (core installation)";
                        return;
                    case PRODUCT_ENTERPRISE_SERVER_V:
                        m_OSFullName = "Windows Server 2008 R2 Enterprise without Hyper-V";
                        return;
                    case PRODUCT_ENTERPRISE_SERVER_CORE_V:
                        m_OSFullName = "Windows Server 2008 R2 Enterprise without Hyper-V (core installation)";
                        return;
                    case PRODUCT_DATACENTER_SERVER:
                        m_OSFullName = "Windows Server 2008 R2 Datacenter";
                        return;
                    case PRODUCT_DATACENTER_SERVER_CORE:
                        m_OSFullName = "Windows Server 2008 R2 Datacenter (core installation)";
                        return;
                    case PRODUCT_DATACENTER_SERVER_V:
                        m_OSFullName = "Windows Server 2008 R2 Datacenter without Hyper-V";
                        return;
                    case PRODUCT_DATACENTER_SERVER_CORE_V:
                        m_OSFullName = "Windows Server 2008 R2 Datacenter without Hyper-V (core installation)";
                        return;
                    case PRODUCT_WEB_SERVER:
                        m_OSName = "Windows Web Server 2008 R2";
                        m_OSFullName = "Windows Web Server 2008 R2";
                        return;
                    case PRODUCT_WEB_SERVER_CORE:
                        m_OSName = "Windows Web Server 2008 R2";
                        m_OSFullName = "Windows Web Server 2008 R2 (core installation)";
                        return;
                    case PRODUCT_ENTERPRISE_SERVER_IA64:
                        m_OSFullName = "Windows Server 2008 R2 for Itanium-based Systems";
                        return;
                    case PRODUCT_SMALLBUSINESS_SERVER:
                    case PRODUCT_SERVER_FOR_SMALLBUSINESS:
                    case PRODUCT_SERVER_FOR_SMALLBUSINESS_V:
                        m_OSName = "Windows Small Business Server 2011";
                        m_OSFullName = "Windows Small Business Server 2011 Standard";
                        return;
                    case PRODUCT_SB_SOLUTION_SERVER:
                        m_OSName = "Windows Small Business Server 2011";
                        m_OSFullName = "Windows Small Business Server 2011 Essentials";
                        return;
                    case PRODUCT_SMALLBUSINESS_SERVER_PREMIUM:
                        m_OSName = "Windows Small Business Server 2011";
                        m_OSFullName = "Windows Small Business Server 2011 Premium";
                        return;
                    case PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT:
                    case PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY:
                    case PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING:
                        m_OSName = "Windows Essential Business Server 2008 R2";
                        m_OSFullName = "Windows Essential Business Server 2008 R2";
                        return;
                    case PRODUCT_STARTER:
                    case PRODUCT_STARTER_N:
                    case PRODUCT_STARTER_E:
                        m_OSFullName = "Windows Server 2008 R2 Starter Edition";
                        return;
                    case PRODUCT_CLUSTER_SERVER:
                        m_OSFullName = "Windows HPC Server 2008 R2";
                        return;
                    case PRODUCT_HOME_SERVER:
                        m_OSName = "Windows Storage Server 2008 R2";
                        m_OSFullName = "Windows Storage Server 2008 R2 Essentials";
                        return;
                    case PRODUCT_HOME_PREMIUM_SERVER:
                        m_OSName = "Windows Home Server 2011";
                        m_OSFullName = "Windows Home Server 2011";
                        return;
                    case PRODUCT_STORAGE_EXPRESS_SERVER:
                        m_OSName = "Windows Storage Server 2008 R2";
                        m_OSFullName = "Windows Storage Server 2008 R2 Express";
                        return;
                    case PRODUCT_STORAGE_STANDARD_SERVER:
                        m_OSName = "Windows Storage Server 2008 R2";
                        m_OSFullName = "Windows Storage Server 2008 R2";
                        return;
                    case PRODUCT_STORAGE_WORKGROUP_SERVER:
                        m_OSName = "Windows Storage Server 2008 R2";
                        m_OSFullName = "Windows Storage Server 2008 R2 Workgroup";
                        return;
                    case PRODUCT_STORAGE_ENTERPRISE_SERVER:
                        m_OSName = "Windows Storage Server 2008 R2";
                        m_OSFullName = "Windows Storage Server 2008 R2 Enterprise";
                        return;
                    case PRODUCT_HYPERV:
                        m_OSName = "Hyper-V Server 2008 R2";
                        m_OSFullName = "Microsoft Hyper-V Server 2008 R2";
                        return;
                    case PRODUCT_SOLUTION_EMBEDDEDSERVER:
                        m_OSName = "Windows MultiPoint Server 2011";
                        m_OSFullName = "Windows MultiPoint Server 2011";
                        return;
                }
                #endregion
            }
        }

        /// <summary>
        /// Windows 8 or Windows Server 2012 operating system informations
        /// </summary>
        private void SetWin8Win2012Information()
        {
            m_OSWindows = OSWindowsID.Windows8_2K12;

            if (m_WindowsOSVersionInfo.wProductType == VER_NT_WORKSTATION)
            {
                m_OSName = "Windows 8";
                m_OSFullName = "Windows 8";

                #region ProductInfoProductType switch
                switch (m_WindowsProductInfoProductType)
                {
                    case PRODUCT_DEVELOPER_PREVIEW:
                        m_OSFullName += " Developer Preview";
                        return;
                    case PRODUCT_CORE_COUNTRYSPECIFIC:
                    case PRODUCT_CORE_SINGLELANGUAGE:
                    case PRODUCT_CORE:
                        //No name for edition
                        return;
                    case PRODUCT_CORE_N:
                        m_OSFullName += " N";
                        return;
                    case PRODUCT_BUSINESS:
                    case PRODUCT_PROFESSIONAL:
                    case PRODUCT_PROFESSIONAL_WMC:
                        m_OSFullName += " Pro";
                        return;
                    case PRODUCT_BUSINESS_N:
                    case PRODUCT_PROFESSIONAL_N:
                        m_OSFullName += " Pro N";
                        return;
                    case PRODUCT_ENTERPRISE:
                    case PRODUCT_ENTERPRISE_EVALUATION:
                    case PRODUCT_ULTIMATE:
                        m_OSFullName += " Enterprise";
                        return;
                    case PRODUCT_ENTERPRISE_N:
                    case PRODUCT_ENTERPRISE_N_EVALUATION:
                    case PRODUCT_ULTIMATE_N:
                        m_OSFullName += " Enterprise N";
                        return;
                }
                #endregion
            }
            else
            {
                m_OSName = "Windows Server 2012";
                m_OSFullName = "Windows Server 2012";

                #region ProductInfoProductType switch
                switch (m_WindowsProductInfoProductType)
                {
                    case PRODUCT_DEVELOPER_PREVIEW:
                        m_OSFullName += " Developer Preview";
                        return;
                    case PRODUCT_STANDARD_SERVER:
                    case PRODUCT_STANDARD_EVALUATION_SERVER:
                        m_OSFullName += " Standard";
                        return;
                    case PRODUCT_STANDARD_SERVER_CORE:
                        m_OSFullName += " Standard (core installation)";
                        return;
                    case PRODUCT_STANDARD_SERVER_V:
                        m_OSFullName += " Standard without Hyper-V";
                        return;
                    case PRODUCT_STANDARD_SERVER_CORE_V:
                        m_OSFullName += " Standard without Hyper-V (core installation)";
                        return;
                    case PRODUCT_DATACENTER_SERVER:
                    case PRODUCT_DATACENTER_EVALUATION_SERVER:
                        m_OSFullName += " Datacenter";
                        return;
                    case PRODUCT_DATACENTER_SERVER_CORE:
                        m_OSFullName += " Datacenter (core installation)";
                        return;
                    case PRODUCT_DATACENTER_SERVER_V:
                        m_OSFullName += " Datacenter without Hyper-V";
                        return;
                    case PRODUCT_DATACENTER_SERVER_CORE_V:
                        m_OSFullName += " Datacenter without Hyper-V (core installation)";
                        return;
                    case PRODUCT_SB_SOLUTION_SERVER:
                    case PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT:
                    case PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY:
                    case PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING:
                    case PRODUCT_ESSENTIALBUSINESS_SERVER_MGMT:
                    case PRODUCT_ESSENTIALBUSINESS_SERVER_ADDL:
                    case PRODUCT_ESSENTIALBUSINESS_SERVER_MGMTSVC:
                    case PRODUCT_ESSENTIALBUSINESS_SERVER_ADDLSVC:
                        m_OSFullName += " Essentials";
                        return;
                    case PRODUCT_SERVER_FOUNDATION:
                        m_OSFullName += " Foundation";
                        return;
                    case PRODUCT_STORAGE_EXPRESS_SERVER:
                    case PRODUCT_STORAGE_STANDARD_SERVER:
                    case PRODUCT_STORAGE_STANDARD_EVALUATION_SERVER:
                        m_OSName = "Windows Storage Server 2012";
                        m_OSFullName = "Windows Storage Server 2012 Standard";
                        return;
                    case PRODUCT_STORAGE_WORKGROUP_SERVER:
                    case PRODUCT_STORAGE_WORKGROUP_EVALUATION_SERVER:
                        m_OSName = "Windows Storage Server 2012";
                        m_OSFullName = "Windows Storage Server 2012 Workgroup";
                        return;
                    case PRODUCT_STORAGE_EXPRESS_SERVER_CORE:
                    case PRODUCT_STORAGE_STANDARD_SERVER_CORE:
                        m_OSName = "Windows Storage Server 2012";
                        m_OSFullName = "Windows Storage Server 2012 Standard (core installation)";
                        return;
                    case PRODUCT_STORAGE_WORKGROUP_SERVER_CORE:
                        m_OSName = "Windows Storage Server 2012";
                        m_OSFullName = "Windows Storage Server 2012 Workgroup (core installation)";
                        return;
                    case PRODUCT_HYPERV:
                        m_OSName = "Hyper-V Server 2012";
                        m_OSFullName = "Hyper-V Server 2012";
                        return;
                }
                #endregion
            }
        }

        /// <summary>
        /// Windows 10 or Windows Server 10 operating system informations
        /// </summary>
        private void SetWin10Information()
        {
            m_OSWindows = OSWindowsID.Windows10;

            if (m_WindowsOSVersionInfo.wProductType == VER_NT_WORKSTATION)
            {
                m_OSName = "Windows 10";
                m_OSFullName = "Windows 10";

                #region ProductInfoProductType switch
                switch (m_WindowsProductInfoProductType)
                {
                    case PRODUCT_DEVELOPER_PREVIEW:
                        m_OSFullName += " Developer Preview";
                        return;
                    case PRODUCT_CORE_COUNTRYSPECIFIC:
                    case PRODUCT_CORE_SINGLELANGUAGE:
                    case PRODUCT_CORE:
                        //No name for edition
                        return;
                    case PRODUCT_CORE_N:
                        m_OSFullName += " N";
                        return;
                    case PRODUCT_BUSINESS:
                    case PRODUCT_PROFESSIONAL:
                    case PRODUCT_PROFESSIONAL_WMC:
                        m_OSFullName += " Pro";
                        return;
                    case PRODUCT_BUSINESS_N:
                    case PRODUCT_PROFESSIONAL_N:
                        m_OSFullName += " Pro N";
                        return;
                    case PRODUCT_ENTERPRISE:
                    case PRODUCT_ENTERPRISE_EVALUATION:
                    case PRODUCT_ULTIMATE:
                        m_OSFullName += " Enterprise";
                        return;
                    case PRODUCT_ENTERPRISE_N:
                    case PRODUCT_ENTERPRISE_N_EVALUATION:
                    case PRODUCT_ULTIMATE_N:
                        m_OSFullName += " Enterprise N";
                        return;
                }
                #endregion
            }
            else
            {
                m_OSName = "Windows Server 10";
                m_OSFullName = "Windows Server 10";

                #region ProductInfoProductType switch
                switch (m_WindowsProductInfoProductType)
                {
                    case PRODUCT_DEVELOPER_PREVIEW:
                        m_OSFullName += " Developer Preview";
                        return;
                    case PRODUCT_STANDARD_SERVER:
                    case PRODUCT_STANDARD_EVALUATION_SERVER:
                        m_OSFullName += " Standard";
                        return;
                    case PRODUCT_STANDARD_SERVER_CORE:
                        m_OSFullName += " Standard (core installation)";
                        return;
                    case PRODUCT_STANDARD_SERVER_V:
                        m_OSFullName += " Standard without Hyper-V";
                        return;
                    case PRODUCT_STANDARD_SERVER_CORE_V:
                        m_OSFullName += " Standard without Hyper-V (core installation)";
                        return;
                    case PRODUCT_DATACENTER_SERVER:
                    case PRODUCT_DATACENTER_EVALUATION_SERVER:
                        m_OSFullName += " Datacenter";
                        return;
                    case PRODUCT_DATACENTER_SERVER_CORE:
                        m_OSFullName += " Datacenter (core installation)";
                        return;
                    case PRODUCT_DATACENTER_SERVER_V:
                        m_OSFullName += " Datacenter without Hyper-V";
                        return;
                    case PRODUCT_DATACENTER_SERVER_CORE_V:
                        m_OSFullName += " Datacenter without Hyper-V (core installation)";
                        return;
                    case PRODUCT_SB_SOLUTION_SERVER:
                    case PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT:
                    case PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY:
                    case PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING:
                    case PRODUCT_ESSENTIALBUSINESS_SERVER_MGMT:
                    case PRODUCT_ESSENTIALBUSINESS_SERVER_ADDL:
                    case PRODUCT_ESSENTIALBUSINESS_SERVER_MGMTSVC:
                    case PRODUCT_ESSENTIALBUSINESS_SERVER_ADDLSVC:
                        m_OSFullName += " Essentials";
                        return;
                    case PRODUCT_SERVER_FOUNDATION:
                        m_OSFullName += " Foundation";
                        return;
                    case PRODUCT_STORAGE_EXPRESS_SERVER:
                    case PRODUCT_STORAGE_STANDARD_SERVER:
                    case PRODUCT_STORAGE_STANDARD_EVALUATION_SERVER:
                        m_OSName = "Windows Storage Server 10";
                        m_OSFullName = "Windows Storage Server 10 Standard";
                        return;
                    case PRODUCT_STORAGE_WORKGROUP_SERVER:
                    case PRODUCT_STORAGE_WORKGROUP_EVALUATION_SERVER:
                        m_OSName = "Windows Storage Server 10";
                        m_OSFullName = "Windows Storage Server 10 Workgroup";
                        return;
                    case PRODUCT_STORAGE_EXPRESS_SERVER_CORE:
                    case PRODUCT_STORAGE_STANDARD_SERVER_CORE:
                        m_OSName = "Windows Storage Server 10";
                        m_OSFullName = "Windows Storage Server 10 Standard (core installation)";
                        return;
                    case PRODUCT_STORAGE_WORKGROUP_SERVER_CORE:
                        m_OSName = "Windows Storage Server 2012";
                        m_OSFullName = "Windows Storage Server 10 Workgroup (core installation)";
                        return;
                    case PRODUCT_HYPERV:
                        m_OSName = "Hyper-V Server 10";
                        m_OSFullName = "Hyper-V Server 10";
                        return;
                }
                #endregion
            }
        }
        #endregion

        /// <summary>
        /// Set m_ServicePack and m_CSDBuildNumber
        /// </summary>
        private void SetWindowsServicePackString()
        {
            if (Environment.OSVersion.Platform != System.PlatformID.Win32NT)
            {
                return;
            }

            string servicePack = m_WindowsOSVersionInfo.szCSDVersion.Trim();
            if (servicePack.Length > 2 && servicePack.Substring(servicePack.Length - 3) == " v.")   //In Windows Vista Build 5456 is "Service Pack 0 v." in szCSDVersion
            {
                servicePack = servicePack.Substring(0, servicePack.Length - 3);
            }

            m_WindowsServicePack = servicePack;

            if (Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion") != null)
            {
                string csdBuildNumberValue = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                            "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion").GetValue("CSDBuildNumber") as string;

                Int32.TryParse(csdBuildNumberValue, out m_WindowsCSDBuildNumber);
            }
        }

        private string GetNETFrameworkVersionString()
        {
            var clrVersion = Environment.Version;

            //http://deletionpedia.org/en/List_of_.NET_Framework_versions
            //https://jonathanparker.wordpress.com/2014/12/05/list-of-net-framework-versions/
            if (clrVersion.Major == 4 && clrVersion.Minor == 0)
            {
                //4.0.30319.237   - 4.0
                //4.0.30319.17020 - 4.5 (Microsoft .NET Framework 4.5 Developer Preview)
                //4.0.30319.17379 - 4.5 (Microsoft .NET Framework 4.5 Consumer Preview)
                //4.0.30319.17626 - 4.5 (Microsoft .NET Framework 4.5 RC)
                //4.0.30319.17929 - 4.5 (Microsoft .NET Framework 4.5 RTM)
                //4.0.30319.18408 - 4.5.1 (Microsoft .NET Framework 4.5.1 RTM - Windows Vista/7 - KB2858728)
                //4.0.30319.34003 - 4.5.1 (Microsoft .NET Framework 4.5.1 RTM - Windows 8.1)
                //4.0.30319.34209 - 4.5.2 (Microsoft .NET Framework 4.5.2 May 2014 Update)
                //4.0.30319.42000 - 4.6 - In .NET Framework 4.6, the Environment.Version property returns the fixed version string 4.0.30319.42000
                //                      - Release 393295 - 4.6 installed with Windows 10 or later
                //                      - Release 393297 - 4.6 installed on all other Windows OS versions or later
                //                      - Release 394254 - 4.6.1 installed on Windows 10 or later
                //                      - Release 394271 - 4.6.1 installed on all other Windows OS versions or later

                if (clrVersion >= new Version(4, 0, 30319, 42000))
                {
                    //Starting with 4.6 has Environment.Version hard-coded value 4.0.30319.42000 (file versioning scheme changed to match the product version).
                    //Get .NET Framework 4.6 or later Information From Registry
                    using (var ndpKey = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
                    {
                        int releaseKey = Convert.ToInt32(ndpKey.GetValue("Release"));
                        if (releaseKey >= 394254)
                        {
                            return "4.6.1";
                        }
                    }

                    return "4.6";
                }
                if (clrVersion >= new Version(4, 0, 30319, 34209))
                {
                    return "4.5.2";
                }
                if (clrVersion >= new Version(4, 0, 30319, 18408))
                {
                    return "4.5.1";
                }
                if (clrVersion >= new Version(4, 0, 30319, 17020))
                {
                    return "4.5";
                }

                return "4.0";   //4.0.30319.237
            }

            if (clrVersion.Major == 2 && clrVersion.Minor == 0)
            {
                #region FW 2.0, 3.0, 3.5, 3.5.1
                if (clrVersion >= new Version(2, 0, 50727, 3521))     //3.5.1
                {
                    //2.0.50727.3521 - 3.5.1 in Windows 7 Beta 2
                    //2.0.50727.4016 - 3.5 SP1 in Windows Vista SP2 or Windows Server 2008 SP2
                    //2.0.50727.4918 - 3.5.1 in Windows 7 RC or Windows Server 2008 R2

                    try
                    {
                        System.Reflection.Assembly.Load(new AssemblyName("System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));

                        if (Environment.OSVersion.Platform == PlatformID.Win32NT && m_WindowsOSVersionInfo.dwMajorVersion >= 6 && m_WindowsOSVersionInfo.dwMinorVersion >= 1)
                        {
                            return "3.5.1";
                        }

                        return "3.5 SP1";
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        //Ignore exception
                    }

                    return "2.0 SP2";
                }

                if (clrVersion >= new Version(2, 0, 50727, 3053))
                {
                    return "3.5 SP1";
                }

                if (clrVersion >= new Version(2, 0, 50727, 1433))
                {
                    //2.0 SP1 or 3.0 SP1 or 3.5
                    try
                    {
                        System.Reflection.Assembly.Load(new AssemblyName("System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
                        return "3.5";
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        //Ignore exception
                    }

                    try
                    {
                        System.Reflection.Assembly.Load(new AssemblyName("WindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
                        return "3.0 SP1";
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        //Ignore exception
                    }

                    return "2.0 SP1";
                }

                if (clrVersion == new Version(2, 0, 50727, 312))  //Vista RTM
                {
                    return "3.0";
                }

                //2.0.50727.42 RTM - 2.0 or 3.0
                try
                {
                    System.Reflection.Assembly.Load(new AssemblyName("WindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"));
                    return "3.0";
                }
                catch (System.IO.FileNotFoundException)
                {
                    //Ignore exception
                }

                return "2.0";
                #endregion
            }

            return clrVersion.Major.ToString(System.Globalization.CultureInfo.InvariantCulture) + "." + clrVersion.Minor.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        #endregion
    }
}