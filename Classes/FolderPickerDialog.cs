using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using IMP.Windows.Interop;

namespace IMP.Windows
{
    /// <summary>
    /// Prompts the user to select a folder.
    /// </summary>
    /// <remarks>
    /// This class will use the Vista style Select Folder dialog if possible, or the regular FolderBrowserDialog if it is not.
    /// </remarks>
    public sealed class FolderPickerDialog : ICommonDialog
    {
        #region WindowHandleWrapper
        private class WindowHandleWrapper : System.Windows.Forms.IWin32Window
        {
            private IntPtr _handle;

            public WindowHandleWrapper(IntPtr handle)
            {
                _handle = handle;
            }

            #region IWin32Window Members

            public IntPtr Handle
            {
                get { return _handle; }
            }

            #endregion
        }
        #endregion

        #region member varible and default property initialization
        private string m_Title = string.Empty;
        private string m_FolderPath = string.Empty;
        #endregion

        #region constructors and destructors
        /// <summary>
        /// Creates a new instance of the <see cref="FolderPickerDialog" /> class.
        /// </summary>
        public FolderPickerDialog() { }
        #endregion

        #region action methods
        public CommonDialogResult ShowDialog()
        {
            return ShowDialog(IntPtr.Zero);
        }

        public CommonDialogResult ShowDialog(IntPtr hwndOwner)
        {
            if (hwndOwner == IntPtr.Zero)
            {
                hwndOwner = CommonDialogNativeMethods.GetActiveWindow();
            }

            if (hwndOwner == IntPtr.Zero)
            {
                throw new InvalidOperationException("Owner handler is not set and cannot be determined!");
            }

            if (IsVistaOrLater)
            {
                return RunVistaNativeDialog(hwndOwner);
            }
            else
            {
                return RunLegacyDialog(hwndOwner);
            }
        }
        #endregion

        #region property getters/setters
        /// <summary>
        /// Gets or sets the dialog title.
        /// </summary>
        /// <value>Dialog title.</value>
        [Localizable(true), DefaultValue(""), Browsable(true), Description("Dialog title."), Category("Appearance")]
        public string Title
        {
            get { return m_Title; }
            set
            {
                m_Title = (value == null) ? string.Empty : value;
            }
        }

        /// <summary>
        /// Gets or sets the folder path selected in dialog.
        /// </summary>
        /// <value>Selected folder path.</value>
        [DefaultValue(""), Browsable(true), Description("Folder path selected in dialog."), Category("Folder Browsing")]
        public string FolderPath
        {
            get { return m_FolderPath; }
            set
            {
                m_FolderPath = (value == null) ? string.Empty : value;
            }
        }

        private static bool IsVistaOrLater
        {
            get { return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major > 5; }
        }
        #endregion

        #region private member functions
        private CommonDialogResult RunVistaNativeDialog(IntPtr hwndOwner)
        {
            var nativeFileOpenDialog = new NativeFileOpenDialog();
            SetNativeDialogProperties(nativeFileOpenDialog);
            try
            {
                int hresult = nativeFileOpenDialog.Show(hwndOwner);

                //Create return information.
                if (hresult < 0)
                {
                    if ((uint)hresult == (uint)HRESULT.E_ERROR_CANCELLED)
                    {
                        return CommonDialogResult.Cancel;
                    }

                    throw Marshal.GetExceptionForHR(hresult);
                }

                GetNativeDialogResult(nativeFileOpenDialog);
                return CommonDialogResult.OK;
            }
            finally
            {
                if (nativeFileOpenDialog != null)
                {
                    Marshal.FinalReleaseComObject(nativeFileOpenDialog);
                }
            }
        }

        private void SetNativeDialogProperties(NativeFileOpenDialog nativeFileOpenDialog)
        {
            //Set Title
            if (!string.IsNullOrEmpty(m_Title))
            {
                nativeFileOpenDialog.SetTitle(m_Title);
            }

            //Set Folder
            if (!string.IsNullOrEmpty(m_FolderPath))
            {
                if (System.IO.Directory.Exists(m_FolderPath))
                {
                    nativeFileOpenDialog.SetFolder(CreateShellItemFromParsingName(m_FolderPath));
                }
                else
                {
                    string parent = System.IO.Path.GetDirectoryName(m_FolderPath);
                    if (parent != null && System.IO.Directory.Exists(parent))
                    {
                        string folder = System.IO.Path.GetFileName(m_FolderPath);
                        nativeFileOpenDialog.SetFolder(CreateShellItemFromParsingName(parent));
                        nativeFileOpenDialog.SetFileName(folder);
                    }
                }
            }

            //Apply option bitflags
            nativeFileOpenDialog.SetOptions(GetNativeDialogFlags());
        }

        private CommonDialogNativeMethods.FOS GetNativeDialogFlags()
        {
            var flags = CommonDialogNativeMethods.FOS.FOS_NOTESTFILECREATE | CommonDialogNativeMethods.FOS.FOS_FORCEFILESYSTEM    //Only File System
                    | CommonDialogNativeMethods.FOS.FOS_FILEMUSTEXIST | CommonDialogNativeMethods.FOS.FOS_PATHMUSTEXIST
                    | CommonDialogNativeMethods.FOS.FOS_PICKFOLDERS;   // Folder Picker

            return flags;
        }

        private void GetNativeDialogResult(NativeFileOpenDialog nativeFileOpenDialog)
        {
            IShellItem item;
            nativeFileOpenDialog.GetResult(out item);
            item.GetDisplayName(CommonDialogNativeMethods.SIGDN.SIGDN_FILESYSPATH, out m_FolderPath);
        }

        private static IShellItem CreateShellItemFromParsingName(string path)
        {
            object nativeShellItem;
            Guid guid = new Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"); //IID_IShellItem

            int hr = CommonDialogNativeMethods.SHCreateItemFromParsingName(path, IntPtr.Zero, ref guid, out nativeShellItem);

            if (nativeShellItem == null || hr < 0)
            {
                throw new ExternalException("Shell item could not be created.", Marshal.GetExceptionForHR(hr));
            }

            return (IShellItem)nativeShellItem;
        }

        private CommonDialogResult RunLegacyDialog(IntPtr hwndOwner)
        {
            var fbd = new Ionic.Utils.FolderBrowserDialogEx()
            {
                ShowNewFolderButton = true,
                Description = m_Title,
                SelectedPath = m_FolderPath
            };

            var result = fbd.ShowDialog(new WindowHandleWrapper(hwndOwner));
            if (result != System.Windows.Forms.DialogResult.OK)
            {
                return CommonDialogResult.Cancel;
            }

            m_FolderPath = fbd.SelectedPath;

            return CommonDialogResult.OK;
        }
        #endregion
    }
}
