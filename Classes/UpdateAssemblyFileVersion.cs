using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using IMP.Shared;

namespace IMP.CustomBuildTasks
{
    /// <summary>
    /// MSBuild task to update File Version in the AssemblyInfo.cs file.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// <Project>
    ///   <PropertyGroup>
    ///     <TasksPath Condition="'$(TasksPath)'==''">c:\BuildBinaries</TasksPath>
    ///   </PropertyGroup>
    ///   <UsingTask TaskName="IMP.CustomBuildTasks.UpdateAssemblyFileVersion" AssemblyFile="$(TasksPath)\VersionBuildTask.dll"/>
    ///   <Target Name="BeforeBuild">
    ///     <IMP.CustomBuildTasks.UpdateAssemblyFileVersion FileName="$(ProjectDir)\Properties\AssemblyInfo.cs" />
    ///   </Target>
    /// </Project>
    /// ]]></code>    
    /// </example>
    public class UpdateAssemblyFileVersion : Task
    {
        #region member varible and default property initialization
        private string m_FileName;
        private string m_Version;
        #endregion

        #region action methods
        public override bool Execute()
        {
            try
            {
                if (string.IsNullOrEmpty(m_Version))
                {
                    m_Version = Environment.GetEnvironmentVariable("MSBuild_Version");
                    if (string.IsNullOrEmpty(m_Version))
                    {
                        m_Version = new Version(1, 0, 0, 0).GetCurrentBuildVersion().ToString();
                        Environment.SetEnvironmentVariable("MSBuild_Version", m_Version);
                        Log.LogMessage("Version set to {0}.", m_Version);
                    }
                }

                if (string.IsNullOrEmpty(m_FileName))
                {
                    string projfile = this.BuildEngine.ProjectFileOfTaskNode;
                    if (!string.IsNullOrEmpty(projfile))
                    {
                        //Get AssemblyInfo.cs file name from project file
                        m_FileName = AssemblyInfoHelper.GetAssemblyInfoFileName(projfile);
                    }
                }

                if (string.IsNullOrEmpty(m_FileName) || !System.IO.File.Exists(m_FileName))
                {
                    this.Log.LogError("The AssemblyInfo file not found.");
                    return false;
                }

                Version version = AssemblyInfoHelper.UpdateFileVersion(m_FileName, System.Version.Parse(m_Version));
                if (version == null)
                {
                    Log.LogError("Unable to replace assembly file version in file {0}.", m_FileName);
                    return false;
                }

                Log.LogMessage("Assembly file version in file {0} replaced to {1}.", m_FileName, version.ToString());
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }

            return true;
        }
        #endregion

        #region property getters/setters
        /// <summary>
        /// Optional full path and filename to the AssemblyInfo.cs file to update File Version.
        /// </summary>
        public string FileName
        {
            get { return m_FileName; }
            set { m_FileName = value; }
        }

        /// <summary>
        /// Optional version to get build and revision numbers from.
        /// </summary>
        public string Version
        {
            get { return m_Version; }
            set { m_Version = value; }
        }
        #endregion
    }
}
