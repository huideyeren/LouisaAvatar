using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VirtualLens2.AV3EditorLib
{

    /// <summary>
    /// Contains validation messages.
    /// </summary>
    public class ValidationReport
    {
        /// <summary>
        /// Represents severity of validation messages.
        /// </summary>
        public enum Severity
        {
            Error,
            Warning,
            Info
        }

        /// <summary>
        /// Represents a validation message.
        /// </summary>
        public class Message
        {
            public Severity Severity { get; set; }
            public string Summary { get; set; }
            public string Details { get; set; }
            public Object Selectable { get; set; }
        }

        private LocalizationTable _localizationTable = null;
        private List<Message> _messages = new List<Message>();

        /// <summary>
        /// Get all reported messages.
        /// </summary>
        public IEnumerable<Message> Messages => _messages;

        /// <summary>
        /// Initialize with localization table.
        /// </summary>
        /// <param name="localizationTable">The string table for localization.</param>
        public ValidationReport(LocalizationTable localizationTable)
        {
            _localizationTable = localizationTable;
        }

        /// <summary>
        /// Report an error.
        /// </summary>
        /// <remarks>
        /// This method reads strings <c>{key}/Summary</c> and <c>{key}/Details</c> from the provided table.
        /// </remarks>
        /// <param name="key">The key for format strings that will be recorded.</param>
        /// <param name="args">The arguments to render format strings.</param>
        public void Error(string key, params object[] args)
        {
            ErrorSelectable(null, key, args);
        }

        /// <summary>
        /// Report an error.
        /// </summary>
        /// <remarks>
        /// This method reads strings <c>{key}/Summary</c> and <c>{key}/Details</c> from the provided table.
        /// </remarks>
        /// <param name="selectable">The object for <c>Selectable</c>.</param>
        /// <param name="key">The key for format strings that will be recorded.</param>
        /// <param name="args">The arguments to render format strings.</param>
        public void ErrorSelectable(Object selectable, string key, params object[] args)
        {
            var summary = _localizationTable[key + "/Summary"] ?? "";
            var details = _localizationTable[key + "/Details"] ?? "";
            _messages.Add(new Message
            {
                Severity = Severity.Error,
                Summary = string.Format(summary, args),
                Details = string.Format(details, args),
                Selectable = selectable
            });
        }

        /// <summary>
        /// Report a warning.
        /// </summary>
        /// <remarks>
        /// This method reads strings <c>{key}/Summary</c> and <c>{key}/Details</c> from the provided table.
        /// </remarks>
        /// <param name="key">The key for format strings that will be recorded.</param>
        /// <param name="args">The arguments to render format strings.</param>
        public void Warning(string key, params object[] args)
        {
            WarningSelectable(null, key, args);
        }

        /// <summary>
        /// Report a warning.
        /// </summary>
        /// <remarks>
        /// This method reads strings <c>{key}/Summary</c> and <c>{key}/Details</c> from the provided table.
        /// </remarks>
        /// <param name="selectable">The object for <c>Selectable</c>.</param>
        /// <param name="key">The key for format strings that will be recorded.</param>
        /// <param name="args">The arguments to render format strings.</param>
        public void WarningSelectable(Object selectable, string key, params object[] args)
        {
            var summary = _localizationTable[key + "/Summary"] ?? "";
            var details = _localizationTable[key + "/Details"] ?? "";
            _messages.Add(new Message
            {
                Severity = Severity.Warning,
                Summary = string.Format(summary, args),
                Details = string.Format(details, args),
                Selectable = selectable
            });
        }

        /// <summary>
        /// Report an information.
        /// </summary>
        /// <remarks>
        /// This method reads strings <c>{key}/Summary</c> and <c>{key}/Details</c> from the provided table.
        /// </remarks>
        /// <param name="key">The key for format strings that will be recorded.</param>
        /// <param name="args">The arguments to render format strings.</param>
        public void Info(string key, params object[] args)
        {
            InfoSelectable(null, key, args);
        }

        /// <summary>
        /// Report an information.
        /// </summary>
        /// <remarks>
        /// This method reads strings <c>{key}/Summary</c> and <c>{key}/Details</c> from the provided table.
        /// </remarks>
        /// <param name="selectable">The object for <c>Selectable</c>.</param>
        /// <param name="key">The key for format strings that will be recorded.</param>
        /// <param name="args">The arguments to render format strings.</param>
        public void InfoSelectable(Object selectable, string key, params object[] args)
        {
            var summary = _localizationTable[key + "/Summary"] ?? "";
            var details = _localizationTable[key + "/Details"] ?? "";
            _messages.Add(new Message
            {
                Severity = Severity.Info,
                Summary = string.Format(summary, args),
                Details = string.Format(details, args),
                Selectable = selectable
            });
        }


        /// <summary>
        /// Get whether this report has any errors or not.
        /// </summary>
        public bool HasError
        {
            get { return _messages.Any(message => message.Severity == Severity.Error); }
        }
    }
}
