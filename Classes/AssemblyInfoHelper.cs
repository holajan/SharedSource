using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using IMP.Shared;

namespace IMP.CustomBuildTasks
{
    internal static class AssemblyInfoHelper
    {
        #region action methods
        public static string GetAssemblyInfoFileName(string projectFileName)
        {
            if (projectFileName == null)
            {
                throw new ArgumentNullException("projectFileName");
            }
            if (projectFileName.Length == 0)
            {
                throw new ArgumentException("projectFileName is empty.", "projectFileName");
            }

            //Get AssemblyInfo.cs file name from project file
            var fileInfo = new System.IO.FileInfo(projectFileName);
            string fileName = System.IO.Path.Combine(fileInfo.DirectoryName, @"Properties\AssemblyInfo.cs");
            if (!System.IO.File.Exists(fileName))
            {
                fileName = System.IO.Path.Combine(fileInfo.DirectoryName, @"AssemblyInfo.cs");
            }

            return fileName;
        }

        public static Version UpdateFileVersion(string assemblyInfoFileName, Version buildAndRevision = null)
        {
            //Load the file
            Encoding fileEncoding;
            string content;
            using (StreamReader reader = new StreamReader(assemblyInfoFileName, Encoding.Default))
            {
                content = reader.ReadToEnd();
                fileEncoding = reader.CurrentEncoding;
            }

            Version version = ReplaceAssemblyFileVersion(ref content, buildAndRevision);
            if (version == null)
            {
                return null;
            }

            //Ensure that the file is writeable
            FileAttributes fileAttributes = File.GetAttributes(assemblyInfoFileName);
            File.SetAttributes(assemblyInfoFileName, fileAttributes & ~FileAttributes.ReadOnly);

            //Save to file
            File.WriteAllText(assemblyInfoFileName, content, fileEncoding);

            //Restore the file's original attributes
            File.SetAttributes(assemblyInfoFileName, fileAttributes);

            return version;
        }
        #endregion

        #region private member functions
        private static Version ReplaceAssemblyFileVersion(ref string content, Version buildAndRevision)
        {
            //Find AssemblyFileVersion
            int index = content.IndexOf("AssemblyFileVersion(\"");
            if (index == -1)
            {
                return null;
            }

            index += "AssemblyFileVersion(\"".Length;
            int endIndex = content.IndexOf("\")", index);

            Version version = new Version(1, 0, 0, 0);
            string versionString = content.Substring(index, endIndex - index);
            if (!string.IsNullOrEmpty(versionString))
            {
                version = new Version(versionString);
            }

            if (buildAndRevision == null)
            {
                //Get major and minor from AssemblyFileVersion and generate build and revision numbers.
                version = version.GetCurrentBuildVersion();
            }
            else
            {
                //Get major and minor from AssemblyFileVersion and build and revision numbers from buildAndRevision.
                version = new Version(version.Major, version.Minor, buildAndRevision.Build, buildAndRevision.Revision);
            }

            //Perform version replacement
            content = content.Substring(0, index) + version.ToString() + content.Substring(endIndex);

            return version;
        }
        #endregion
    }
}