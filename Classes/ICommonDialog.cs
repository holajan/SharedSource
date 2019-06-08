using System;

namespace IMP.Windows
{
    /// <summary>
    /// Specifies identifiers to indicate the return value of a CommonFileDialog dialog.
    /// </summary>
    public enum CommonDialogResult
    {
        /// <summary>
        /// The dialog box return value is OK (usually sent from a button labeled OK or Save).
        /// </summary>
        OK = 1,
        /// <summary>
        /// The dialog box return value is Cancel (usually sent from a button labeled Cancel).
        /// </summary>
        Cancel = 2
    }

    interface ICommonDialog
    {
        /// <summary>
        /// Runs the dialog with a default owner.
        /// </summary>
        /// <returns>CommonFileDialogResult.OK if the user clicks OK in the dialog; otherwise, CommonFileDialogResult.Cancel.</returns>
        CommonDialogResult ShowDialog();

        /// <summary>
        /// Runs the dialog with the specified owner.
        /// </summary>
        /// <param name="ownerWindowHandle">Window handle of any top-level window that will own the modal dialog box.</param>
        /// <returns>CommonFileDialogResult.OK if the user clicks OK in the dialog; otherwise, CommonFileDialogResult.Cancel.</returns>
        CommonDialogResult ShowDialog(IntPtr ownerWindowHandle);
    }
}
