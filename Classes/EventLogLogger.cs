using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace IMP.Shared
{
    #region public enum declarations
    /// <summary>
    /// Důležitost logované zprávy
    /// </summary>
    public enum VerbosityLevel
    {
        /// <summary>
        /// Vysoká (například zápis do eventlogu)
        /// </summary>
        HighImportance = 1,
        /// <summary>
        /// Standardní
        /// </summary>
        Normal,
        /// <summary>
        /// Trace (například zápis pouze do trace)
        /// </summary>
        Trace
    }

    /// <summary>
    /// Typ logované zprávy
    /// </summary>
    public enum EntryType
    {
        /// <summary>
        /// Informace
        /// </summary>
        Information = 1,
        /// <summary>
        /// Varování
        /// </summary>
        Warning,
        /// <summary>
        /// Chyba
        /// </summary>
        Error
    }
    #endregion

    /// <summary>
    /// Interface třídy implementující zápis do logu
    /// </summary>
    public interface ILogger
    {
        #region action methods
        /// <summary>
        /// Zápis zprávy do logu
        /// </summary>
        /// <param name="message">Text zprávy</param>
        /// <param name="level">Důležitost zprávy</param>
        /// <param name="type">Typ zprávy</param>
        void WriteLine(string message, VerbosityLevel level = VerbosityLevel.Normal, EntryType type = EntryType.Information);
        #endregion
    }

    /// <summary>
    /// Základní třída pro třídy implementující zápis do logu
    /// </summary>
    public abstract class AbstractLogger : ILogger
    {
        #region member varible and default property initialization
        /// <summary>
        /// Úrověň důležitosti logovaných zpráv
        /// </summary>
        protected VerbosityLevel Verbosity { get; set; }
        #endregion

        #region constructors and destructors
        /// <summary>
        /// Konstruktor třídy <see cref="AbstractLogger"/>
        /// </summary>
        protected AbstractLogger()
        {
            this.Verbosity = VerbosityLevel.Trace;
        }

        /// <summary>
        /// Konstruktor třídy <see cref="AbstractLogger"/>
        /// </summary>
        /// <param name="verbosity">Úrověň důležitosti logovaných zpráv</param>
        protected AbstractLogger(VerbosityLevel verbosity)
        {
            this.Verbosity = verbosity;
        }
        #endregion

        #region action methods
        /// <summary>
        /// Zápis zprávy do logu
        /// </summary>
        /// <param name="message">Text zprávy</param>
        /// <param name="level">Důležitost zprávy</param>
        /// <param name="type">Typ zprávy</param>
        public void WriteLine(string message, VerbosityLevel level = VerbosityLevel.Normal, EntryType type = EntryType.Information)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (this.IsVerbosityAtLeast(level))
            {
                this.OnWriteLine(message, level, type);
            }
        }
        #endregion

        #region private member functions
        /// <summary>
        /// Implementace zápisu zprávy
        /// </summary>
        /// <param name="message">Text zprávy</param>
        /// <param name="level">Důležitost zprávy</param>
        /// <param name="type">Typ zprávy</param>
        protected abstract void OnWriteLine(string message, VerbosityLevel level, EntryType type);

        private bool IsVerbosityAtLeast(VerbosityLevel checkVerbosity)
        {
            return this.Verbosity >= checkVerbosity;
        }
        #endregion
    }

    /// <summary>
    /// Zápis zpráv do event logu
    /// </summary>
    public class EventLogLogger : AbstractLogger
    {
        #region member varible and default property initialization
        /// <summary>
        /// Zdroj zpráv zapisovaných do event logu
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Hlavička zpráv zapisovaných do event logu
        /// </summary>
        public string EntryHeader { get; set; }
        #endregion

        #region constructors and destructors
        /// <summary>
        /// Konstruktor třídy <see cref="EventLogLogger"/>
        /// </summary>
        public EventLogLogger()
        {
            this.Source = GetDefaultSourceName();
            this.Verbosity = VerbosityLevel.HighImportance;
        }

        /// <summary>
        /// Konstruktor třídy <see cref="EventLogLogger"/>
        /// </summary>
        /// <param name="verbosity">Úrověň důležitosti logovaných zpráv</param>
        public EventLogLogger(VerbosityLevel verbosity)
        {
            this.Source = GetDefaultSourceName();
            this.Verbosity = verbosity;
        }
        #endregion

        #region action methods
        /// <summary>
        /// Implementace zápisu zprávy
        /// </summary>
        /// <param name="message">Text zprávy</param>
        /// <param name="level">Důležitost zprávy</param>
        /// <param name="type">Typ zprávy</param>
        protected override void OnWriteLine(string message, VerbosityLevel level, EntryType type)
        {
            EventLogEntryType entryType;

            switch (type)
            {
                case EntryType.Error:
                    entryType = EventLogEntryType.Error;
                    break;
                case EntryType.Warning:
                    entryType = EventLogEntryType.Warning;
                    break;
                default:
                    entryType = EventLogEntryType.Information;
                    break;
            }

            EventLog.WriteEntry(this.Source, TrimMessage((string.IsNullOrEmpty(this.EntryHeader) ? "" : this.EntryHeader + Environment.NewLine) + message, this.Source.Length), entryType);
        }
        #endregion

        #region private member functions
        private static string TrimMessage(string message, int sourceLength)
        {
            int maxLength = 31894 - sourceLength;
            const int endLines = 20;

            if (message.Length <= maxLength)
            {
                return message;
            }

            var lines = ReadLines(message).ToList();
            var sb = new System.Text.StringBuilder();

            sb.AppendLine();
            sb.AppendLine("----Část této zprávy je na tomto místě vynechána----");
            for (int i = lines.Count - endLines - 1; i < lines.Count; i++)
            {
                sb.AppendLine();
                sb.Append(lines[i]);
            }

            int index = 0;
            foreach (var line in lines)
            {
                if (sb.Length + line.Length > maxLength)
                {
                    break;
                }
                sb.Insert(index, line);
                index += line.Length;
                sb.Insert(index, Environment.NewLine);
                index += Environment.NewLine.Length;
            }

            return sb.ToString();
        }

        private static IEnumerable<string> ReadLines(string s)
        {
            using (var reader = new StringReader(s))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    yield return line;
                }
            }
        }

        private static string GetDefaultSourceName()
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly() ?? System.Reflection.Assembly.GetExecutingAssembly();

            return assembly.GetName().Name;
        }
        #endregion
    }
}