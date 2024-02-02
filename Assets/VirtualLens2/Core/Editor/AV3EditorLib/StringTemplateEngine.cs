using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VirtualLens2.AV3EditorLib
{
    /// <summary>
    /// Tiny template engine for string replacement.
    /// </summary>
    public class StringTemplateEngine
    {
        private readonly IDictionary<string, string> _parameters;
        
        /// <summary>
        /// Initializes with parameter set.
        /// </summary>
        /// <param name="parameters">Parameters to be used for rendering templates.</param>
        /// <exception cref="ArgumentNullException"><c>parameters</c> is <c>null</c>.</exception>
        public StringTemplateEngine(IDictionary<string, string> parameters)
        {
            if (parameters == null) { throw new ArgumentNullException(nameof(parameters)); }
            _parameters = new Dictionary<string, string>();
            foreach (var kv in parameters) { _parameters.Add(kv); }
        }

        /// <summary>
        /// Renders the given template.
        /// </summary>
        /// <param name="template">The template string</param>
        /// <returns>The result of template rendering.</returns>
        public string Render(string template)
        {
            if (template == null) { return null; }

            string EvaluateMatch(Match m)
            {
                var key = m.Groups[1].Value;
                return _parameters.ContainsKey(key) ? _parameters[key] : m.Groups[0].Value;
            }

            return Regex.Replace(template, @"\{\{([\w_]+)\}\}", EvaluateMatch);
        }
    }
}