using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.ComponentModel;
using System.Windows.Media;

namespace IMP.Windows.Controls
{
    /// <summary>
    /// Window for modal dialogs
    /// </summary>
    public class DialogWindow : Window
    {
        #region constants
        private const string cFakeIcon = @"AAABAAEAEBACAAAAAACwAAAAFgAAACgAAAAQAAAAIAAAAAEAAQAAAAAAgAAAAAAAAAAAAAAAAgAAAAAAAAAAAAAA////AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/////////////////////////////////////////////////////////////////////////////////////";
        #endregion

        #region Win32 API
        private const Int32 GWL_STYLE = -16;
        private const Int32 GWL_EXSTYLE = -20;
        private const UInt32 WS_MAXIMIZEBOX = 0x10000;
        private const UInt32 WS_MINIMIZEBOX = 0x20000;
        private const UInt32 WS_SYSMENU = 0x80000;
        private const UInt32 WS_EX_DLGMODALFRAME = 0x1;
        private const UInt32 WS_EX_WINDOWEDGE = 0x100;

        private static readonly IntPtr HWND_TOP = new IntPtr(0);

        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 SWP_NOZORDER = 0x0004;
        private const UInt32 SWP_NOACTIVATE = 0x0010;
        private const UInt32 SWP_FRAMECHANGED = 0x0020; /* The frame changed: send WM_NCCALCSIZE */

        [DllImport("user32.dll")]
        private extern static UInt32 SetWindowLong(IntPtr hWnd, Int32 nIndex, UInt32 dwNewLong);

        [DllImport("user32.dll")]
        private extern static UInt32 GetWindowLong(IntPtr hWnd, Int32 nIndex);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam);

        private const int WM_NCHITTEST = 0x0084;
        #endregion

        #region member varible and default property initialization
        private ResizeMode originalResizeMode = ResizeMode.NoResize;

        private MessageBoxResult m_DialogResult;
        #endregion

        #region constructors and destructors
        /// <summary>
        /// DialogWindow constructor
        /// </summary>
        public DialogWindow()
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.ShowInTaskbar = false;
            this.ResizeMode = ResizeMode.NoResize;

            SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);
            this.UseLayoutRounding = true;

            this.Loaded += new RoutedEventHandler(DialogWindow_Loaded);
        }
        #endregion

        #region action methods
        /// <summary>
        /// Opens the dialog window and returns <see cref="DialogResult"/> after dialog is closed.
        /// </summary>
        /// <returns>A MessageBoxResult value that specifies which dialog box button is clicked by the user.</returns>
        public new MessageBoxResult ShowDialog()
        {
            return ShowDialog(null);
        }

        /// <summary>
        /// Opens the dialog window and returns <see cref="DialogResult"/> after dialog is closed.
        /// </summary>
        /// <param name="Owner">The System.Windows.Window that owns this DialogWindow.</param>
        /// <returns>A MessageBoxResult value that specifies which dialog box button is clicked by the user.</returns>
        public MessageBoxResult ShowDialog(Window Owner)
        {
            if (Owner != null)
            {
                this.Owner = Owner;
            }

            if (this.Owner == null)
            {
                if (Application.Current != null)
                {
                    this.Owner = Application.Current.MainWindow;
                }
            }

            if (double.IsNaN(this.Height))
            {
                if (double.IsNaN(this.Width))
                {
                    this.SizeToContent = SizeToContent.WidthAndHeight;
                }
                else
                {
                    this.SizeToContent = SizeToContent.Height;
                }
            }
            else if (double.IsNaN(this.Width))
            {
                this.SizeToContent = SizeToContent.Width;
            }

            base.ShowDialog();

            return this.DialogResult;
        }

        /// <summary>
        /// Opens the dialog window and returns <see cref="DialogResult"/> after dialog is closed.
        /// </summary>
        /// <param name="OwnerHandle">The IntPtr Handle of window that owns this DialogWindow.</param>
        /// <returns>A MessageBoxResult value that specifies which dialog box button is clicked by the user.</returns>
        public MessageBoxResult ShowDialog(IntPtr OwnerHandle)
        {
            if (OwnerHandle != IntPtr.Zero)
            {
                var helper = new WindowInteropHelper(this);
                helper.Owner = OwnerHandle;
            }

            if (double.IsNaN(this.Height))
            {
                if (double.IsNaN(this.Width))
                {
                    this.SizeToContent = SizeToContent.WidthAndHeight;
                }
                else
                {
                    this.SizeToContent = SizeToContent.Height;
                }
            }
            else if (double.IsNaN(this.Width))
            {
                this.SizeToContent = SizeToContent.Width;
            }

            base.ShowDialog();

            return this.DialogResult;
        }
        #endregion

        #region property getters/setters
        /// <summary>
        /// A MessageBoxResult value that specifies which dialog box button is clicked by the user.
        /// </summary>
        public new MessageBoxResult DialogResult
        {
            get { return m_DialogResult; }
            set
            {
                m_DialogResult = value;

                //Close the window and return from ShowDialog() call 
                base.DialogResult = true;
            }
        }

        /// <summary>
        /// ControlBox dependency property
        /// </summary>
        public static readonly DependencyProperty ControlBoxProperty = DependencyProperty.Register("ControlBox", typeof(bool), typeof(DialogWindow), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value indicating whether a control box is displayed in the caption bar of the window.
        /// </summary>
        /// <remarks>
        /// Hides dialog Icon, Minimize, Maximize and Close buttons in the caption bar.
        /// </remarks>
        [DefaultValue(true)]
        public bool ControlBox
        {
            get { return (bool)GetValue(ControlBoxProperty); }
            set { SetValue(ControlBoxProperty, value); }
        }

        /// <summary>
        /// ShowIcon dependency property
        /// </summary>
        public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register("ShowIcon", typeof(bool), typeof(DialogWindow), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value indicating whether an icon is displayed in the caption bar of the window.
        /// </summary>
        [DefaultValue(false)]
        public bool ShowIcon
        {
            get { return (bool)GetValue(ShowIconProperty); }
            set { SetValue(ShowIconProperty, value); }
        }
        #endregion

        #region private member functions
        private void DialogWindow_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr hWnd = new WindowInteropHelper(this).Handle;

            var source = HwndSource.FromHwnd(hWnd);
            source.AddHook(new HwndSourceHook(DialogWindow_WndProc));

            originalResizeMode = this.ResizeMode;
            if (originalResizeMode == System.Windows.ResizeMode.NoResize || originalResizeMode == System.Windows.ResizeMode.CanMinimize)
            {
                this.ResizeMode = ResizeMode.CanResize;
            }

            UInt32 windowStyleex = GetWindowLong(hWnd, GWL_EXSTYLE);
            windowStyleex |= WS_EX_DLGMODALFRAME;   //Hide dialog icon if icon is not set (Fixed Dialog style)
            SetWindowLong(hWnd, GWL_EXSTYLE, windowStyleex);

            if (this.ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip || !this.ControlBox)
            {
                UInt32 windowStyle = GetWindowLong(hWnd, GWL_STYLE);

                //Disable Minimize and Maximize buttons
                if (this.ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip)
                {
                    if (originalResizeMode == System.Windows.ResizeMode.CanMinimize)
                    {
                        windowStyle = windowStyle & ~WS_MAXIMIZEBOX;
                    }
                    else
                    {
                        windowStyle = windowStyle & ~WS_MINIMIZEBOX & ~WS_MAXIMIZEBOX;
                    }
                }
                //Set ControlBox (hides dialog Icon, Minimize, Maximize and Close buttons)
                if (!this.ControlBox)
                {
                    windowStyle = windowStyle & ~WS_SYSMENU;
                }

                SetWindowLong(hWnd, GWL_STYLE, windowStyle);
            }

            SetWindowPos(hWnd, HWND_TOP, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED);

            if (!this.ShowIcon)
            {
                if (this.Icon == null)
                {
                    //Fix hide dialog icon - Set dummy icon and then clear icon
                    var fakeIconData = new System.IO.MemoryStream(Convert.FromBase64String(cFakeIcon));
                    this.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(fakeIconData);
                    this.Icon = null;
                }

                //Hide icon
                SendMessage(hWnd, 0x80, 0, 0);
                SendMessage(hWnd, 0x80, 1, 0);
            }
        }

        private IntPtr DialogWindow_WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int HTCLIENT = 0x0001;
            const int HTSIZE = 4;   //Size box (same as HTGROWBOX)
            const int HTLEFT = 10;
            const int HTRIGHT = 11;
            const int HTTOP = 12;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOM = 15;
            const int HTBOTTOMLEFT = 16;
            const int HTBOTTOMRIGHT = 17;
            const int HTBORDER = 18;

            if (msg == WM_NCHITTEST && originalResizeMode != ResizeMode.CanResize && originalResizeMode != ResizeMode.CanResizeWithGrip)
            {
                //Disable sizing
                int result = DefWindowProc(hwnd, WM_NCHITTEST, wParam, lParam).ToInt32();
                if (result == HTSIZE || result == HTLEFT || result == HTRIGHT || result == HTTOP || result == HTTOPLEFT ||
                    result == HTTOPRIGHT || result == HTBOTTOM || result == HTBOTTOMLEFT || result == HTBOTTOMRIGHT || result == HTBORDER)
                {
                    handled = true;
                    return new IntPtr(HTCLIENT);    //Use this result to tell Windows to handle that point of your form like client Area
                }
            }

            return IntPtr.Zero;
        }
        #endregion
    }
}