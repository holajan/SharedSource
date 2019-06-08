using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;

namespace IMP.Shared
{
    internal static class ProcessUtil
    {
        #region NativeMethods class
        private static class NativeMethods
        {
            [StructLayout(LayoutKind.Sequential)]
            public class STARTUPINFO
            {
                public int cb;
                public IntPtr lpReserved;
                public IntPtr lpDesktop;
                public IntPtr lpTitle;
                public int dwX;
                public int dwY;
                public int dwXSize;
                public int dwYSize;
                public int dwXCountChars;
                public int dwYCountChars;
                public int dwFillAttribute;
                public int dwFlags;
                public short wShowWindow;
                public short cbReserved2;
                public IntPtr lpReserved2;
                public IntPtr hStdInput;
                public IntPtr hStdOutput;
                public IntPtr hStdError;
            }

            [StructLayout(LayoutKind.Sequential)]
            public class PROCESS_INFORMATION
            {
                public IntPtr hProcess;
                public IntPtr hThread;
                public int dwProcessId;
                public int dwThreadId;
            }

            [Flags]
            internal enum LogonFlags
            {
                //Log on, but use the specified credentials on the network only. The new process uses the same token as the caller, but the system creates
                //a new logon session within LSA, and the process uses the specified credentials as the default credentials.
                //This value can be used to create a process that uses a different set of credentials locally than it does remotely.
                //This is useful in inter-domain scenarios where there is no trust relationship.
                //The system does not validate the specified credentials. Therefore, the process can start, but it may not have access to network resources.
                LOGON_NETCREDENTIALS_ONLY = 2,
                //Log on, then load the user profile in the HKEY_USERS registry key. The function returns after the profile is loaded.
                //Loading the profile can be time-consuming, so it is best to use this value only if you must access the information in the HKEY_CURRENT_USER registry key. 
                LOGON_WITH_PROFILE = 1
            }

            [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
            public static extern bool CreateProcessWithLogonW(string userName, string domain, IntPtr password, LogonFlags logonFlags, [MarshalAs(UnmanagedType.LPTStr)] string appName, System.Text.StringBuilder cmdLine, int creationFlags, IntPtr environmentBlock, [MarshalAs(UnmanagedType.LPTStr)] string lpCurrentDirectory, STARTUPINFO lpStartupInfo, PROCESS_INFORMATION lpProcessInformation);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);
        }
        #endregion

        #region action methods
        /// <summary>
        /// Run an command under another user credentials.
        /// </summary>
        /// <param name="command">Executable file with arguments</param>
        /// <param name="workingDirectory">Working directory</param>
        /// <param name="user">Username with domain of desired credentials or null for Elevated Start</param>
        /// <param name="password">Password of desired credentials</param>
        /// <param name="showOutput">Show Output window</param>
        /// <param name="netOnly">Indicates that the user information specified is for remote access only. (RunAs /NetOnly functionality)</param>
        /// <param name="windowhandle">windowhandle for UAC dilog or IntPtr.Zero</param>
        public static bool RunCommandAs(string command, string workingDirectory, string user, string password, bool netOnly, bool showOutput, IntPtr windowhandle)
        {
            string cmd = string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("COMSPEC")) ? "cmd.exe" : System.Environment.GetEnvironmentVariable("COMSPEC");
            var startInfo = new ProcessStartInfo(cmd, "/c " + EncodeParameterArgument(command + (showOutput ? " & pause" : "")))
            {
                WorkingDirectory = workingDirectory,
                LoadUserProfile = false,
                CreateNoWindow = !showOutput,
                WindowStyle = !showOutput ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
            };

            if (string.IsNullOrEmpty(user))
            {
                startInfo.UseShellExecute = true;
                startInfo.Verb = "runas";      //Start Elevated
            }
            else
            {
                string userName = ExtractLogin(user);
                string domainName = ExtractDomain(user);

                startInfo.UseShellExecute = false;
                startInfo.Domain = domainName;
                startInfo.UserName = userName;
                startInfo.Password = MakeSecureString(password);
            }

            if (windowhandle != IntPtr.Zero)
            {
                //Make the UAC dialog modal to Windowhandle
                startInfo.ErrorDialog = true;
                startInfo.ErrorDialogParentHandle = windowhandle;
            }

            try
            {
                if (string.IsNullOrEmpty(user) || !netOnly)
                {
                    //Run command normaly using Process class
                    var process = System.Diagnostics.Process.Start(startInfo);
                    //Wait for the process to end.
                    process.WaitForExit();
                }
                else
                {
                    //Run command with RunAs /NetOnly functionality (CreateProcessWithLogonW with LOGON_NETCREDENTIALS_ONLY LogonFlag) and wait for the process to end.
                    StartProcessWithLogonNetOnly(startInfo, true);
                }

                return true;
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                try
                {
                    //Provede publikaci vyjímky pomoci Exception Management Application Bloku
                    var additionalInfo = new System.Collections.Specialized.NameValueCollection();
                    additionalInfo.Add("Command", command);
                    additionalInfo.Add("WorkingDirectory", workingDirectory);
                    additionalInfo.Add("User", user);
                    additionalInfo.Add("Password", password);

                    Microsoft.ApplicationBlocks.ExceptionManagement.ExceptionManager.Publish(ex, additionalInfo);
                }
                catch (System.Exception publishex)
                {
                    System.Windows.Forms.MessageBox.Show(publishex.ToString(), "Publish Error",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }

                if (string.IsNullOrEmpty(user))
                {
                    //User cancelled UAC dialog
                    return false;
                }

                throw;
            }
        }

        /// <summary>
        /// Run an command under another user credentials.
        /// </summary>
        /// <param name="command">Executable file with arguments</param>
        /// <param name="workingDirectory">Working directory</param>
        /// <param name="user">Username with domain of desired credentials or null for Elevated Start</param>
        /// <param name="password">Password of desired credentials</param>
        /// <param name="netOnly">Indicates that the user information specified is for remote access only. (RunAs /NetOnly functionality)</param>
        /// <param name="showOutput">Show Output window</param>
        public static bool RunCommandAs(string command, string workingDirectory, string user, string password, bool netOnly, bool showOutput)
        {
            return RunCommandAs(command, workingDirectory, user, password, netOnly, showOutput, IntPtr.Zero);
        }

        /// <summary>
        /// Run an command under another user credentials.
        /// </summary>
        /// <param name="command">Executable file with arguments</param>
        /// <param name="workingDirectory">Working directory</param>
        /// <param name="user">Username with domain of desired credentials or null for Elevated Start</param>
        /// <param name="password">Password of desired credentials</param>
        /// <param name="netOnly">Indicates that the user information specified is for remote access only. (RunAs /NetOnly functionality)</param>
        public static bool RunCommandAs(string command, string workingDirectory, string user, string password, bool netOnly)
        {
            return RunCommandAs(command, workingDirectory, user, password, netOnly, false, IntPtr.Zero);
        }

        /// <summary>
        /// Run an command under another user credentials.
        /// </summary>
        /// <param name="command">Executable file with arguments</param>
        /// <param name="workingDirectory">Working directory</param>
        /// <param name="user">Username with domain of desired credentials or null for Elevated Start</param>
        /// <param name="password">Password of desired credentials</param>
        public static bool RunCommandAs(string command, string workingDirectory, string user, string password)
        {
            return RunCommandAs(command, workingDirectory, user, password, false, false, IntPtr.Zero);
        }

        /// <summary>
        /// Run an command under another user credentials.
        /// </summary>
        /// <param name="command">Executable file with arguments</param>
        /// <param name="workingDirectory">Working directory</param>
        public static bool RunCommandAs(string command, string workingDirectory)
        {
            return RunCommandAs(command, workingDirectory, null, null, false, false, IntPtr.Zero);
        }
        #endregion

        #region private member functions
        private static void StartProcessWithLogonNetOnly(ProcessStartInfo startInfo, bool waitForExit)
        {
            if (startInfo.UseShellExecute)
            {
                throw new InvalidOperationException("UseShellExecute must be false.");
            }

            if (startInfo.LoadUserProfile)
            {
                throw new InvalidOperationException("LoadUserProfile cannot be used.");
            }

            if (string.IsNullOrEmpty(startInfo.UserName))
            {
                throw new InvalidOperationException("UserName is empty.");
            }

            var cmdLine = BuildCommandLine(startInfo.FileName, startInfo.Arguments);
            var lpStartupInfo = new NativeMethods.STARTUPINFO();
            var lpProcessInformation = new NativeMethods.PROCESS_INFORMATION();

            int creationFlags = 0;
            if (startInfo.CreateNoWindow)
            {
                creationFlags |= 0x8000000;
            }

            IntPtr zero = IntPtr.Zero;

            string workingDirectory = startInfo.WorkingDirectory;
            if (string.IsNullOrEmpty(workingDirectory))
            {
                workingDirectory = Environment.CurrentDirectory;
            }

            NativeMethods.LogonFlags logonFlags = NativeMethods.LogonFlags.LOGON_NETCREDENTIALS_ONLY;   //NetOnly;

            IntPtr passwordPrt = IntPtr.Zero;
            try
            {
                if (startInfo.Password == null)
                {
                    passwordPrt = Marshal.StringToCoTaskMemUni(string.Empty);
                }
                else
                {
                    passwordPrt = Marshal.SecureStringToCoTaskMemUnicode(startInfo.Password);
                }

                int error = 0;
                bool flag = NativeMethods.CreateProcessWithLogonW(startInfo.UserName, startInfo.Domain, passwordPrt, logonFlags, null, cmdLine, creationFlags, zero, workingDirectory, lpStartupInfo, lpProcessInformation);
                if (!flag)
                {
                    error = Marshal.GetLastWin32Error();
                }

                if (!flag)
                {
                    if (error != 0xc1 && error != 0xd8)
                    {
                        throw new Win32Exception(error);
                    }
                    throw new Win32Exception(error, "Invalid Application");
                }
            }
            finally
            {
                if (passwordPrt != IntPtr.Zero)
                {
                    Marshal.ZeroFreeCoTaskMemUnicode(passwordPrt);
                }
            }

            if (waitForExit)
            {
                NativeMethods.WaitForSingleObject(lpProcessInformation.hProcess, 0xFFFFFFFF);
            }
        }

        private static StringBuilder BuildCommandLine(string executableFileName, string arguments)
        {
            StringBuilder builder = new StringBuilder();
            string str = executableFileName.Trim();
            bool flag = str.StartsWith("\"", StringComparison.Ordinal) && str.EndsWith("\"", StringComparison.Ordinal);
            if (!flag)
            {
                builder.Append("\"");
            }
            builder.Append(str);
            if (!flag)
            {
                builder.Append("\"");
            }
            if (!string.IsNullOrEmpty(arguments))
            {
                builder.Append(" ");
                builder.Append(arguments);
            }
            return builder;
        }

        private static SecureString MakeSecureString(string text)
        {
            SecureString secure = new SecureString();
            foreach (char c in text)
            {
                secure.AppendChar(c);
            }

            return secure;
        }

        /// <summary>
        /// Encodes an argument for passing into a program
        /// </summary>
        /// <param name="original">The value that should be received by the program</param>
        /// <returns>The value which needs to be passed to the program for the original value 
        /// to come through</returns>
        public static string EncodeParameterArgument(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            value = "\"" + System.Text.RegularExpressions.Regex.Replace(value, @"(\\)+$", @"$1$1") + "\"";

            return value;
        }

        /// <summary>
        /// Vrací samotné přihlašovací jméno uživatele
        /// </summary>
        /// <param name="loginName">Přihlašovací jméno ve tvaru "domain\login" nebo "login@domain.local", případně pouze login</param>
        /// <returns>Přihlašovací jméno uživatele</returns>
        private static string ExtractLogin(string loginName)
        {
            string strExp = loginName.Replace("/", "\\");

            int index = strExp.IndexOf('\\');
            if (index != -1)
            {
                strExp = strExp.Substring(index + 1);
            }
            else
            {
                index = strExp.IndexOf('@');
                if (index != -1)
                {
                    strExp = strExp.Substring(0, index);
                }
            }

            return strExp;
        }

        /// <summary>
        /// Vrací název domeny z přihlašovacího jména uživatele
        /// </summary>
        /// <param name="loginName">Přihlašovací jméno ve tvaru "domain\login" nebo "login@domain.local", případně pouze login</param>
        /// <returns>Název domeny uživatele</returns>
        private static string ExtractDomain(string loginName)
        {
            string strExp = loginName.Replace("/", "\\");

            int index = strExp.IndexOf('\\');
            if (index != -1)
            {
                strExp = strExp.Substring(0, index);
            }
            else
            {
                index = strExp.IndexOf('@');
                if (index != -1)
                {
                    strExp = strExp.Substring(index + 1);
                }
                else
                {
                    strExp = "";
                }
            }

            index = strExp.IndexOf('.');
            if (index != -1)
            {
                strExp = strExp.Substring(0, index);
            }

            return strExp.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
        }
        #endregion
    }
}
