using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Reflection;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.VisualStudio.TeamFoundation.VersionControl;
using Microsoft.TeamFoundation;

namespace IMP.TFSSCExplorerExtension.TFS
{
    //VersionControlServer.SupportedFeatures is an indicator of what server version you are talking to
    //                                          42684268421 - Microsoft.TeamFoundation.VersionControl.Common.SupportedFeatures Flags
    // 7     Team Foundation Server 2008 RTM  -         111 - GetLatestOnCheckout, OneLevelMapping, Destroy
    // 31    Team Foundation Server 2008 SP1  -       11111 - +CreateBranch, GetChangesForChangeset
    // 895   Team Foundation Server 2010 RTM  -  1101111111 - +ProxySuite, LocalVersions, BatchedCheckins, WorkspacePermissions
    // 1919  Team Foundation Server 2010 SP1  - 11101111111 - +CheckinDates

    //Microsoft.TeamFoundation.VersionControl.Common.SupportedFeatures Flags
    // None = 0,
    // GetLatestOnCheckout = 1,
    // OneLevelMapping = 2,
    // Destroy = 4,
    // CreateBranch = 8,
    // GetChangesForChangeset = 16,
    // ProxySuite = 32,
    // LocalVersions = 64,
    // BatchedCheckins = 256,
    // WorkspacePermissions = 512,
    // CheckinDates = 1024,

    internal static class SourceControlHelper
    {
        #region member varible and default property initialization
        private static VersionControlServer s_HasRepositoryExtensionsVCS;
        private static bool s_HasRepositoryExtensions;
        #endregion

        #region action methods
        #region Source control actions
        public static void Merge(Workspace workspace, IEnumerable<Tuple<string, string>> itemsToMerge)
        {
            foreach (var itemToMerge in itemsToMerge)
            {
                TrackMessage(string.Format("Merging {0} --> {1}", itemToMerge.Item1, itemToMerge.Item2));
                try
                {
                    var status = workspace.Merge(itemToMerge.Item1, itemToMerge.Item2, null, null, LockLevel.None, RecursionType.None, MergeOptions.None);
                    if (status.NumConflicts != 0)
                    {
                        TrackWarning("Merge operation encountered a conflict.");
                    }
                    else if (status.NoActionNeeded)
                    {
                        TrackMessage(string.Format("File {0} has no changes to merge.", itemToMerge.Item1));
                    }
                }
                catch (Microsoft.TeamFoundation.VersionControl.Client.NoMergeRelationshipException)
                {
                    //The item ... is not a branch of ...
                    TrackMessage(string.Format("A merge relationship does not exists, a baseless merge will be performed.", itemToMerge.Item1));

                    var status = workspace.Merge(itemToMerge.Item1, itemToMerge.Item2, null, null, LockLevel.None, RecursionType.None, MergeOptionsEx.Baseless);
                    if (status.NumConflicts != 0)
                    {
                        bool resolved = false;

                        var conflicts = workspace.QueryConflicts(new string[] { itemToMerge.Item2 }, false);
                        if (conflicts.Length == 1)
                        {
                            var conflict = conflicts[0];

                            //Take source version
                            if (!conflict.IsResolved && !conflict.NameChanged && !conflict.RequiresExplicitAcceptMerge && conflict.YourChangeType == ChangeType.None)
                            {
                                conflict.Resolution = Resolution.AcceptTheirs;
                                workspace.ResolveConflict(conflict);

                                if (conflict.IsResolved)
                                {
                                    resolved = true;

                                    if (UndoUnchangedItem(workspace, itemToMerge.Item2))
                                    {
                                        TrackMessage(string.Format("File {0} has no changes to merge.", itemToMerge.Item1));
                                    }
                                }
                            }
                        }

                        if (!resolved)
                        {
                            TrackWarning("Merge operation encountered a conflict.");
                        }
                    }
                    else if (status.NoActionNeeded)
                    {
                        TrackMessage(string.Format("File {0} has no changes to merge.", itemToMerge.Item1));
                    }
                    else
                    {
                        if (UndoUnchangedItem(workspace, itemToMerge.Item2))
                        {
                            TrackMessage(string.Format("File {0} has no changes to merge.", itemToMerge.Item1));
                        }
                    }
                }
                catch (Microsoft.TeamFoundation.VersionControl.Client.MappingException ex)
                {
                    TrackWarning(ex.Message);
                }
            }
        }

        public static void BranchToFolder(Workspace workspace, IEnumerable<string> items, string folder)
        {
            BranchToFolder(workspace, items, folder, VersionSpec.Latest);
        }

        public static void BranchToFolder(Workspace workspace, IEnumerable<string> items, string folder, VersionSpec versionSpec)
        {
            foreach (var item in items)
            {
                string fileName = VersionControlPath.GetFileName(item);
                string folderPath = VersionControlPath.Combine(folder, fileName);

                workspace.PendBranch(item, folderPath, versionSpec);
            }
        }

        public static void MoveToFolder(Workspace workspace, IEnumerable<string> items, string folder)
        {
            try
            {
                foreach (var item in items)
                {
                    string fileName = VersionControlPath.GetFileName(item);
                    string folderPath = VersionControlPath.Combine(folder, fileName);

                    workspace.PendRename(item, folderPath);
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                //Access is denied
                //This file is currently not available for use on this computer
                TrackWarning(ex.Message);
            }
        }

        public static void CopyToFolder(Workspace workspace, IEnumerable<VersionControlExplorerItem> items, string folder)
        {
            string folderLocalPath;
            try
            {
                folderLocalPath = workspace.GetLocalItemForServerItem(folder);
            }
            catch (Microsoft.TeamFoundation.VersionControl.Client.MappingException ex)
            {
                TrackWarning(ex.Message);
                return;
            }

            if (!System.IO.Directory.Exists(folderLocalPath))
            {
                TrackMessage(string.Format("Folder {0} not found", folderLocalPath));
                return;
            }

            foreach (var item in items)
            {
                string localPath = null;
                try
                {
                    localPath = item.LocalPath;
                    if (string.IsNullOrEmpty(localPath) && item.TargetServerPath != null)
                    {
                        localPath = workspace.GetLocalItemForServerItem(item.TargetServerPath);
                    }
                }
                catch (Microsoft.TeamFoundation.VersionControl.Client.MappingException ex)
                {
                    TrackWarning(ex.Message);
                    continue;
                }

                if (string.IsNullOrEmpty(localPath))
                {
                    TrackMessage(string.Format("Local item path cannot be loaded", localPath));
                    continue;
                }

                if (item.IsFolder)
                {
                    if (!System.IO.Directory.Exists(localPath))
                    {
                        TrackMessage(string.Format("Folder {0} not found", localPath));
                        continue;
                    }

                    string targetFolder = System.IO.Path.Combine(folderLocalPath, System.IO.Path.GetFileName(localPath));
                    CopyFiles(localPath, targetFolder, true, true);

                    workspace.PendAdd(targetFolder, true);
                    continue;
                }

                if (!System.IO.File.Exists(localPath))
                {
                    TrackMessage(string.Format("File {0} not found", localPath));
                    continue;
                }

                string targetFile = System.IO.Path.Combine(folderLocalPath, System.IO.Path.GetFileName(localPath));
                try
                {
                    System.IO.File.Copy(localPath, targetFile, true);
                }
                catch (System.UnauthorizedAccessException ex)
                {
                    //Access to the path ... is denied - Ignore
                    TrackWarning(ex.Message);
                    continue;
                }

                //Ensure that the file is writeable
                var fileAttributes = System.IO.File.GetAttributes(targetFile);
                System.IO.File.SetAttributes(targetFile, fileAttributes & ~System.IO.FileAttributes.ReadOnly);

                workspace.PendAdd(targetFile);
            }
        }

        public static void Destroy(VersionControlServer vcs, IEnumerable<string> items, bool keepHistory)
        {
            try
            {
                foreach (var item in items)
                {
                    var flags = DestroyFlags.Silent | DestroyFlags.StartCleanup;
                    if (keepHistory)
                    {
                        flags |= DestroyFlags.KeepHistory;
                    }

                    vcs.Destroy(new ItemSpec(item, RecursionType.Full), VersionSpec.Latest, null, flags);
                }
            }
            catch (Exception ex)
            {
                TrackWarning(ex.Message);
            }
        }

        /// <summary>
        /// Undo changes to unchanged files in the workspace
        /// </summary>
        /// <param name="workspace">Workspace</param>
        public static void UndoUnchanged(Workspace workspace)
        {
            //Get pending changes
            PendingChange[] pendingChanges = workspace.GetPendingChanges();
            if (pendingChanges.Length == 0)
            {
                return;
            }

            var spec = new ChangesetVersionSpec(workspace.VersionControlServer.GetLatestChangesetId());

            var sourceArray = ItemSpec.FromPendingChanges(pendingChanges);
            var pendingChangesItems = new ItemSet[0];
            if (sourceArray.Length > 0)
            {
                pendingChangesItems = workspace.VersionControlServer.GetItems(sourceArray, spec, DeletedState.Any, ItemType.Any);
            }

            //Build redundant change list
            var itemsToUndo = new List<ItemSpec>();
            var itemsToGet = new List<string>();
            for (int j = 0; j < pendingChanges.Length; j++)
            {
                var pendingChange = pendingChanges[j];
                if (pendingChange.IsAdd || pendingChange.IsDelete || pendingChange.IsMerge || (pendingChange.ChangeType & ~ChangeType.Lock) == ChangeType.Edit)
                {
                    Item item = null;
                    foreach (Item currentItem in pendingChangesItems[j].Items)
                    {
                        if (pendingChange.ItemType == currentItem.ItemType && (pendingChange.IsDelete || currentItem.DeletionId == 0))
                        {
                            if (pendingChange.IsDelete && currentItem.DeletionId == 0)
                            {
                                item = null;
                                break;
                            }
                            if (!pendingChange.IsDelete || item == null || currentItem.DeletionId >= item.DeletionId)
                            {
                                item = currentItem;
                            }
                        }
                    }

                    if (item != null)
                    {
                        bool additemToUndo = false;
                        bool addItemToGet = false;
                        if (pendingChange.ItemType == ItemType.File)
                        {
                            if (pendingChange.IsDelete)
                            {
                                additemToUndo = true;
                                addItemToGet = true;
                                TrackMessage(string.Format("{0}: {1}", pendingChange.ChangeTypeName, pendingChange.LocalItem));
                            }
                            else if (IsMatchesContent(pendingChange.LocalItem, item.HashValue))
                            {
                                additemToUndo = true;
                                if (pendingChange.IsAdd || pendingChange.IsDelete)
                                {
                                    addItemToGet = true;
                                }
                                TrackMessage(string.Format("{0} (contents match): {1}", pendingChange.ChangeTypeName, pendingChange.LocalItem));
                            }
                        }
                        else if (pendingChange.IsAdd || pendingChange.IsDelete)
                        {
                            additemToUndo = true;
                            addItemToGet = true;
                            TrackMessage(string.Format("{0}: {1}", pendingChange.ChangeTypeName, pendingChange.LocalItem));
                        }

                        if (additemToUndo)
                        {
                            itemsToUndo.Add(new ItemSpec(pendingChange));
                        }
                        if (addItemToGet)
                        {
                            itemsToGet.Add(pendingChange.LocalItem);
                        }
                    }
                }
            }

            if (itemsToUndo.Count == 0)
            {
                //No redundant pending changes
                TrackMessage("There are no redundant pending changes.");
                return;
            }

            //Undo redundant pending changes
            TrackMessage("Undoing redundant changes...");
            workspace.Undo(itemsToUndo.ToArray());

            if (itemsToGet.Count > 0)
            {
                //Force get undone adds
                TrackMessage("Forcing a get for undone adds and deletes...");
                workspace.Get(itemsToGet.ToArray(), spec, RecursionType.None, GetOptions.GetAll | GetOptions.Overwrite);
            }
        }
        #endregion

        #region Branch functions
        public static ExtendedItem GetExtendedItem(VersionControlServer vcs, string path)
        {
            var itemSpecs = new ItemSpec[] { new ItemSpec(path, RecursionType.None) };

            var getItemsOptions = GetBranchObjectSupported(vcs) ? GetItemsOptions.IncludeBranchInfo : GetItemsOptions.None;
            getItemsOptions |= GetItemsOptions.IncludeSourceRenames;

            try
            {
                var extendedItems = vcs.GetExtendedItems(itemSpecs, DeletedState.Any, ItemType.Any, getItemsOptions, null)[0];  //[podle itemSpecs.Length prvků]
                if (extendedItems.Length > 0)
                {
                    return extendedItems[0];    //[podle RecursionType]
                }
            }
            catch (System.Xml.XmlException)
            {
                //Unexpected end of file.
                //The data at the root level is invalid. Line 1, position 1.
            }
            catch (System.IndexOutOfRangeException)
            {
                //Index was outside the bounds of the array.
            }
            catch (System.NotSupportedException)
            {
                //Stream does not support reading.
            }
            catch (System.IO.IOException)
            {
                //Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host.
            }

            return null;
        }

        public static HashSet<string> GetAllBranchesList(VersionControlServer vcs, string path)
        {
            var list = new HashSet<string>();
            FillAllBranchesList(vcs, new ItemIdentifier(path), list);

            return list;
        }

        public static BranchHistoryTreeItem GetBranchHistory(VersionControlServer vcs, string path)
        {
            try
            {
                return GetBranchHistoryRequestedItem(GetBranchHistoryTree(vcs, path));
            }
            catch (System.NotSupportedException)
            {
                //Stream does not support reading.
            }
            catch (System.IO.InvalidDataException)
            {
                //Unknown block type. Stream might be corrupted.
            }
            catch (System.IO.IOException)
            {
                //Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host.
            }

            return null;
        }
        #endregion

        #region Others functions
        public static bool IsFolderCloaked(Workspace workspace, string serverItem)
        {
            if (workspace != null)
            {
                WorkingFolder folder = workspace.TryGetWorkingFolderForServerItem(serverItem);
                if (folder == null)
                {
                    return false;
                }
                if (folder.IsCloaked)
                {
                    return true;
                }
            }

            return false;
        }

        public static void Cloak(Workspace workspace, string serverItem)
        {
            workspace.Cloak(serverItem);
            workspace.Get(new string[] { serverItem }, VersionSpec.Latest, RecursionType.Full, GetOptions.None);
        }

        public static void UnCloak(Workspace workspace, string serverItem)
        {
            string localItem = null;
            WorkingFolder mapping = null;
            foreach (WorkingFolder workingFolder in workspace.Folders)
            {
                if (workingFolder.Type == WorkingFolderType.Cloak)
                {
                    if (serverItem != null && (VersionControlPath.Equals(workingFolder.ServerItem, serverItem) || VersionControlPath.Equals(workingFolder.DisplayServerItem, serverItem)))
                    {
                        localItem = workingFolder.LocalItem;
                        mapping = workingFolder;
                        break;
                    }
                    if (localItem != null && Microsoft.TeamFoundation.Common.FileSpec.Equals(workingFolder.LocalItem, localItem))
                    {
                        serverItem = workingFolder.ServerItem;
                        mapping = workingFolder;
                        break;
                    }
                }
            }

            if (mapping != null)
            {
                workspace.DeleteMapping(mapping);
            }
        }

        public static bool HasRepositoryExtensions(VersionControlServer vcs)
        {
            if (vcs == null)
            {
                s_HasRepositoryExtensionsVCS = null;
                s_HasRepositoryExtensions = false;
                return false;
            }

            if (vcs != s_HasRepositoryExtensionsVCS)
            {
                try
                {
#if VS2010
                    //vcs.Repository.Extensions != null;
                    var versionControlServerType = vcs.GetType();

                    var repositoryProperty = versionControlServerType.GetProperty("Repository", BindingFlags.NonPublic | BindingFlags.Instance);
                    object repository = repositoryProperty.GetValue(vcs, new object[0]);

                    var extensionsProperty = repository.GetType().GetProperty("Extensions", BindingFlags.NonPublic | BindingFlags.Instance);
                    object extensions = extensionsProperty.GetValue(repository, new object[0]);

                    s_HasRepositoryExtensionsVCS = vcs;
                    s_HasRepositoryExtensions = extensions != null;
#else
                    s_HasRepositoryExtensionsVCS = vcs;
                    s_HasRepositoryExtensions = vcs.WebServiceLevel >= WebServiceLevel.Tfs2010;
#endif
                }
                catch (Exception)
                {
                    s_HasRepositoryExtensionsVCS = vcs;
                    s_HasRepositoryExtensions = false;
                }
            }

            return s_HasRepositoryExtensions;
        }

        public static bool PendingChangeItemsDirtyCheck(Workspace workspace)
        {
            try
            {
                //Get pending changes
                PendingChange[] pendingChanges = workspace.GetPendingChanges();
                if (pendingChanges.Length == 0)
                {
                    return false;
                }

                for (int j = 0; j < pendingChanges.Length; j++)
                {
                    var pendingChange = pendingChanges[j];

                    if (pendingChange.IsEdit && !string.IsNullOrEmpty(pendingChange.LocalItem) && IsItemDirty(pendingChange.LocalItem))
                    {
                        return true;
                    }
                }
            }
            catch (System.IO.IOException)
            {
                //Unable to read data from the transport connection: The connection was closed.
            }

            return false;
        }
        #endregion
        #endregion

        #region private member functions
        private static string GetParentPath(string serverItem)
        {
            int index = serverItem.LastIndexOf('/');
            if (index != -1)
            {
                if (index == 1)
                {
                    return "$/";
                }

                return serverItem.Substring(0, index);
            }

            return null;
        }

        /// <summary>
        /// Undo changes in item
        /// </summary>
        /// <param name="workspace">Workspace</param>
        private static bool UndoUnchangedItem(Workspace workspace, string sourceItem)
        {
            var spec = new ChangesetVersionSpec(workspace.VersionControlServer.GetLatestChangesetId());
            var itemSet = workspace.VersionControlServer.GetItems(new ItemSpec[] { new ItemSpec(sourceItem, RecursionType.None) }, spec, DeletedState.Any, ItemType.Any)[0];
            if (itemSet.Items.Length == 0)
            {
                return false;
            }

            Item item = itemSet.Items[0];
            string localItem = workspace.GetLocalItemForServerItem(item.ServerItem);

            if (item.ItemType == ItemType.File && IsMatchesContent(localItem, item.HashValue))
            {
                //Undo item
                workspace.Undo(item.ServerItem);
                return true;
            }

            return false;
        }

        private static void CopyFiles(string sourcePath, string destPath, bool overwrite, bool noReadOnly)
        {
            var dir = new System.IO.DirectoryInfo(sourcePath);
            if (!System.IO.Directory.Exists(destPath))
            {
                System.IO.Directory.CreateDirectory(destPath);
            }

            foreach (var file in dir.GetFiles())
            {
                string targetFile = System.IO.Path.Combine(destPath, file.Name);

                file.CopyTo(targetFile, overwrite);

                if (noReadOnly)
                {
                    //Ensure that the file is writeable
                    var fileAttributes = System.IO.File.GetAttributes(targetFile);
                    System.IO.File.SetAttributes(targetFile, fileAttributes & ~System.IO.FileAttributes.ReadOnly);
                }
            }

            foreach (var subDir in dir.GetDirectories())
            {
                string targetDirPath = System.IO.Path.Combine(destPath, subDir.Name);
                CopyFiles(System.IO.Path.Combine(sourcePath, subDir.Name), targetDirPath, overwrite, noReadOnly);
            }
        }

        private static bool IsMatchesContent(string path, byte[] hash)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && hash.Length > 0 && File.Exists(path))
                {
                    byte[] buffer = ComputeMD5Hash(path);

                    return BitConverter.ToString(hash).Equals(BitConverter.ToString(buffer));
                }
            }
            catch (IOException)
            {
            }
            return false;
        }

        private static byte[] ComputeMD5Hash(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                try
                {
                    using (var md = new MD5CryptoServiceProvider())
                    {
                        return md.ComputeHash(stream);
                    }
                }
                catch (InvalidOperationException)
                {
                    return new byte[0];
                }
            }
        }

        private static void TrackMessage(string message)
        {
            TFSSCExplorerExtensionPackage.ReportFailure(String.Format("TFSSCExplorerExtension Message: {0}", message));
        }

        private static void TrackWarning(string message)
        {
            TFSSCExplorerExtensionPackage.ReportFailure(String.Format("TFSSCExplorerExtension Warning: {0}", message));
        }

        private static bool IsItemDirty(string item)
        {
            Assembly visualStudioVersionControlsAssembly = typeof(VersionControlExt).Assembly;

            //Microsoft.VisualStudio.TeamFoundation.VersionControl.ClientHelperVS.IsItemDirty(item);
            var clientHelperVSType = visualStudioVersionControlsAssembly.GetType("Microsoft.VisualStudio.TeamFoundation.VersionControl.ClientHelperVS");
            return (bool)clientHelperVSType.InvokeMember("IsItemDirty", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static, null, null, new object[] { item });
        }

        private static void FillAllBranchesList(VersionControlServer vcs, ItemIdentifier item, HashSet<string> list)
        {
            var branches = vcs.QueryBranchObjects(item, RecursionType.Full);

            foreach (var branche in branches)
            {
                if (branche.Properties.ParentBranch != null && !list.Contains(branche.Properties.ParentBranch.Item))
                {
                    FillAllBranchesList(vcs, branche.Properties.ParentBranch, list);
                }

                list.Add(branche.Properties.RootItem.Item);

                foreach (var child in branche.ChildBranchesNoClone)
                {
                    list.Add(child.Item);
                }
            }
        }

        private static BranchHistoryTreeItem[] GetBranchHistoryTree(VersionControlServer vcs, string path)
        {
            var item = GetExtendedItem(vcs, path);
            if (item == null)
            {
                return null;
            }

            var version = new ChangesetVersionSpec(item.VersionLatest);

            BranchHistoryTreeItem[][] branchHistory = vcs.GetBranchHistory(new ItemSpec[] { new ItemSpec(path, RecursionType.None) }, version);

            if (((branchHistory.Length > 0) && (branchHistory[0].GetLength(0) > 0)) && (branchHistory[0][0].Children.Count > 0))
            {
                return branchHistory[0];
            }

            return null;
        }

        private static BranchHistoryTreeItem GetBranchHistoryRequestedItem(System.Collections.IEnumerable branches)
        {
            if (branches == null)
            {
                return null;
            }

            foreach (BranchHistoryTreeItem item in branches)
            {
                if (item.Relative.IsRequestedItem)
                {
                    return item;
                }

                var requestedItem = GetBranchHistoryRequestedItem(item.Children);
                if (requestedItem != null)
                {
                    return requestedItem;
                }
            }

            return null;
        }

        private static bool GetBranchObjectSupported(VersionControlServer vcs)
        {
            if (vcs == null)
            {
                return false;
            }
            return (vcs.WebServiceLevel >= WebServiceLevel.Tfs2010);
        }
        #endregion
    }
}