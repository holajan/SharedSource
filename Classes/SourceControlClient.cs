using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace IMP.SourceControl
{
    /// <summary>
    /// Source Control helper class working on TFS current local workspace
    /// </summary>
    internal static class SourceControlClient
    {
        #region member varible and default property initialization
        private static Workspace Workspace;
        #endregion

        #region private member functions
        static SourceControlClient()
        {
            var workspaceInfo = GetCurrentWorkspace();
            Workspace = GetWorkspace(workspaceInfo);
        }
        #endregion

        #region action methods
        /// <summary>
        /// Update workspace with the latest version of specified item if item is not currently checked out.
        /// </summary>
        /// <param name="item">The path to the file to update.</param>
        /// <param name="recursive">Recurse to the last child.</param>
        /// <returns><c>false</c> if file is currently checked out.</returns>
        public static bool GetLatestVersion(string item, bool recursive = false)
        {
            var checkinChanges = Workspace.GetPendingChangesEnumerable(item, recursive ? RecursionType.Full : RecursionType.None);
            if (!PendingChange.IsIEnumerableEmpty(checkinChanges))
            {
                //File is checked out.
                return false;
            }

            var s = Workspace.Get(new string[] { item }, VersionSpec.Latest, recursive ? RecursionType.Full : RecursionType.None, GetOptions.Overwrite | GetOptions.GetAll);
            return true;
        }

        /// <summary>
        /// Performs a check-out of the specified item.
        /// </summary>
        /// <param name="item">The path to the file to check out.</param>
        /// <param name="lockLevel">The lock level to apply to each file checked out.</param>
        /// <param name="recursive">Recurse to the last child.</param>
        /// <returns><c>false</c> if file was already checked out.</returns>
        public static bool CheckOut(string item, LockLevel lockLevel = LockLevel.Unchanged, bool recursive = false)
        {
            var checkinChanges = Workspace.GetPendingChangesEnumerable(item, recursive ? RecursionType.Full : RecursionType.None);
            if (!PendingChange.IsIEnumerableEmpty(checkinChanges))
            {
                //File is already checked out.
                return false;
            }

            Workspace.PendEdit(new string[] { item }, recursive ? RecursionType.Full : RecursionType.None, null, lockLevel, true,
                        Microsoft.TeamFoundation.VersionControl.Common.PendChangesOptions.GetLatestOnCheckout);

            return true;
        }

        /// <summary>
        /// Performs a check-in of the specified item.
        /// </summary>
        /// <param name="item">The path of the file to check in.</param>
        /// <param name="comment">Check-in comment.</param>
        /// <param name="checkinNotes">Check-in notes.</param>
        /// <param name="recursive">Recurse to the last child.</param>
        /// <returns>The result of the evaluation.</returns>
        public static CheckinEvaluationResult CheckIn(string item, string comment = null, CheckinNote checkinNotes = null, bool recursive = false, bool overridePolicyFailures = true)
        {
            var checkinChanges = Workspace.GetPendingChangesEnumerable(item, recursive ? RecursionType.Full : RecursionType.None);
            if (PendingChange.IsIEnumerableEmpty(checkinChanges))
            {
                throw new InvalidOperationException("There are no pending changes!");
            }

            var checkinOptions = CheckinEvaluationOptions.Notes | CheckinEvaluationOptions.Policies;
            var checkedWorkItems = new WorkItemCheckinInfo[0];

            var result = Workspace.EvaluateCheckin2(checkinOptions, null, checkinChanges, comment, checkinNotes, checkedWorkItems);
            if (result.Conflicts.Length > 0 || result.NoteFailures.Length > 0 || result.PolicyEvaluationException != null ||
               (result.PolicyFailures.Length > 0 && !overridePolicyFailures))
            {
                return result;
            }

            PolicyOverrideInfo policyOverrideInfo = null;
            if (result.PolicyFailures.Length > 0)
            {
                policyOverrideInfo = new PolicyOverrideInfo("PolicyFailures override!", result.PolicyFailures);
            }

            var checkInParameters = new WorkspaceCheckInParameters(checkinChanges, comment);
            checkInParameters.CheckinNotes = checkinNotes;
            checkInParameters.AssociatedWorkItems = checkedWorkItems;
            checkInParameters.PolicyOverride = policyOverrideInfo;
            Workspace.CheckIn(checkInParameters);

            return result;
        }

        /// <summary>
        /// Undo the pending changes for the specified item.
        /// </summary>
        /// <param name="item">The path to the file to check out.</param>
        /// <param name="recursive">Recurse to the last child.</param>
        public static void UndoCheckOut(string item, bool recursive = false)
        {
            Workspace.Undo(item, recursive ? RecursionType.Full : RecursionType.None);
        }
        #endregion

        #region private member functions
        private static WorkspaceInfo GetCurrentWorkspace()
        {
            WorkspaceInfo[] allLocalWorkspaceInfo = Workstation.Current.GetAllLocalWorkspaceInfo();
            if (allLocalWorkspaceInfo.Length == 1)
            {
                return allLocalWorkspaceInfo[0];
            }

            string currentDirectory = CurrentDirectory;
            WorkspaceInfo localWorkspaceInfo = Workstation.Current.GetLocalWorkspaceInfo(currentDirectory);
            if (localWorkspaceInfo != null)
            {
                return localWorkspaceInfo;
            }

            WorkspaceInfo[] localWorkspaceInfoRecursively = Workstation.Current.GetLocalWorkspaceInfoRecursively(currentDirectory);
            if (localWorkspaceInfoRecursively.Length != 1)
            {
                throw new InvalidOperationException("Unable to determine Workspace!");
            }

            return localWorkspaceInfoRecursively[0];
        }

        private static string CurrentDirectory
        {
            get
            {
                string currentDirectory = Environment.CurrentDirectory;
                if (currentDirectory.IndexOf('~') >= 0)
                {
                    currentDirectory = System.IO.Path.GetFullPath(currentDirectory);
                }
                return currentDirectory;
            }
        }

        private static Workspace GetWorkspace(WorkspaceInfo workspaceInfo)
        {
            string tfsName = workspaceInfo.ServerUri.AbsoluteUri;
            var credentials = System.Net.CredentialCache.DefaultCredentials; //new System.Net.NetworkCredential(userName, password, domain);
            var projects = new TfsTeamProjectCollection(TfsTeamProjectCollection.GetFullyQualifiedUriForName(tfsName), credentials, new UICredentialsProvider());

            return workspaceInfo.GetWorkspace(projects);
        }
        #endregion
    }
}
