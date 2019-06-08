using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace IMP.CustomBuildTasks
{
    internal enum MessageType
    {
        Message = 0,
        Warning,
        Error,
        Custom
    }

    internal class LogMessageEventArgs : EventArgs
    {
        public string Message { get; private set; }
        public MessageType Type { get; private set; }

        public LogMessageEventArgs(string message)
        {
            this.Message = message;
            this.Type = MessageType.Message;
        }

        public LogMessageEventArgs(string message, MessageType type)
        {
            this.Message = message;
            this.Type = type;
        }
    }

    /// <summary>
    /// Logger BuildEngine for testing 
    /// </summary>
    public class LoggerBuildEngine : IBuildEngine
    {
        #region delegates and events
        internal event EventHandler<LogMessageEventArgs> LogMessage;
        #endregion

        #region action methods
        public void LogCustomEvent(CustomBuildEventArgs e)
        {
            OnLogMessage(e.Message, MessageType.Custom);
        }

        public void LogErrorEvent(BuildErrorEventArgs e)
        {
            OnLogMessage(e.Message, MessageType.Error);
        }

        public void LogWarningEvent(BuildWarningEventArgs e)
        {
            OnLogMessage(e.Message, MessageType.Warning);
        }

        public void LogMessageEvent(BuildMessageEventArgs e)
        {
            OnLogMessage(e.Message, MessageType.Message);
        }

        public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
        {
            return false;
        }
        #endregion

        #region property getters/setters
        public bool ContinueOnError
        {
            get { return false; }
        }

        public string ProjectFileOfTaskNode
        {
            get { return string.Empty; }
        }

        public int LineNumberOfTaskNode
        {
            get { return 0; }
        }

        public int ColumnNumberOfTaskNode
        {
            get { return 0; }
        }
        #endregion

        #region private member functions
        private void OnLogMessage(string message, MessageType type)
        {
            if (LogMessage != null)
            {
                LogMessage(this, new LogMessageEventArgs(message, type));
            }
        }
        #endregion
    }
}


