using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;

namespace VirtualLens2.AV3EditorLib
{
    /// <summary>
    /// Utility functions for manipulating <c>VRCExpressionParameters</c>.
    /// </summary>
    public static class VRCExpressionParametersEditor
    {
        /// <summary>
        /// Returns a deep copy of expression parameters.
        /// </summary>
        /// <param name="parameters">The expression parameters to be cloned.</param>
        /// <returns>The deep copy of <c>parameters</c>.</returns>
        public static VRCExpressionParameters Clone(VRCExpressionParameters parameters)
        {
            return Object.Instantiate(parameters);
        }


        /// <summary>
        /// Cleanup an expression parameters.
        /// </summary>
        /// <param name="parameters">The expression parameters to be modified.</param>
        public static void Cleanup(VRCExpressionParameters parameters)
        {
            Undo.RecordObject(parameters, "Cleanup expression parameters");
            if (parameters.parameters == null)
            {
                parameters.parameters = new VRCExpressionParameters.Parameter[] { };
            }
            else
            {
                // Assets created by legacy VRCSDK (-2021.01.19) contains fixed number of slots.
                // Remove empty slots to make it similar to newer data.
                parameters.parameters = parameters.parameters
                    .Where(p => !string.IsNullOrEmpty(p.name))
                    .ToArray();
            }
        }


        /// <summary>
        /// Removes all parameters satisfying a condition from an animator controller.
        /// </summary>
        /// <param name="parameters">The expression parameters to be modified.</param>
        /// <param name="pred">The function to test each parameter for a condition.</param>
        /// <exception cref="ArgumentNullException"><c>parameters</c> or <c>pred</c> is null.</exception>
        public static void RemoveParameters(
            VRCExpressionParameters parameters, Predicate<VRCExpressionParameters.Parameter> pred)
        {
            if (parameters == null) { throw new ArgumentNullException(nameof(parameters)); }
            if (pred == null) { throw new ArgumentNullException(nameof(pred)); }

            Undo.RecordObject(parameters, "Remove parameters");
            Cleanup(parameters);
            parameters.parameters = parameters.parameters
                .Where(p => !pred(p))
                .ToArray();
        }

        /// <summary>
        /// Removes all parameters satisfying a condition from an animator controller.
        /// </summary>
        /// <param name="parameters">The expression parameters to be modified.</param>
        /// <param name="re">The regex to test name of each parameter for a condition.</param>
        /// <exception cref="ArgumentNullException"><c>parameters</c> or <c>pred</c> is null.</exception>
        public static void RemoveParameters(VRCExpressionParameters parameters, Regex re)
        {
            if (parameters == null) { throw new ArgumentNullException(nameof(parameters)); }
            if (re == null) { throw new ArgumentNullException(nameof(re)); }
            RemoveParameters(parameters, p => re.IsMatch(p.name));
        }


        /// <summary>
        /// Merges two expression parameters assets.
        /// </summary>
        /// <param name="destination">The expression parameters to be modified.</param>
        /// <param name="source">The expression parameters to be merged with the <c>destination</c>.</param>
        /// <exception cref="ArgumentNullException"><c>destination</c> or <c>source</c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><c>source</c> cannot be merged into <c>destination</c>.</exception>
        public static void Merge(VRCExpressionParameters destination, VRCExpressionParameters source)
        {
            if (destination == null) { throw new ArgumentNullException(nameof(destination)); }
            if (source == null) { throw new ArgumentNullException(nameof(source)); }

            var destParameters = new Dictionary<string, VRCExpressionParameters.Parameter>();
            foreach (var p in destination.parameters) { destParameters.Add(p.name, p); }
            bool hasChanges = false;
            foreach (var src in source.parameters)
            {
                if (!destParameters.ContainsKey(src.name))
                {
                    hasChanges = true;
                    continue;
                }
                var dest = destParameters[src.name];
                if (src.valueType != dest.valueType ||
                    !src.defaultValue.Equals(dest.defaultValue) ||
                    src.saved != dest.saved)
                {
                    throw new ArgumentException("Parameters cannot be merged.");
                }
            }
            if (!hasChanges) { return; }

            Undo.RecordObject(destination, "Merge expression parameters");
            Cleanup(destination);
            destination.parameters = destination.parameters
                .Concat(source.parameters
                    .Where(p => !destParameters.ContainsKey(p.name))
                    .Select(p => new VRCExpressionParameters.Parameter
                    {
                        name = p.name, valueType = p.valueType, defaultValue = p.defaultValue, saved = p.saved
                    }))
                .ToArray();
        }


        /// <summary>
        /// Renders template strings in expression parameters.
        /// </summary>
        /// <seealso cref="StringTemplateEngine"/>
        /// <param name="expressionParameters">The expression parameters to be modified.</param>
        /// <param name="templateParameters">Key-value pairs used for rendering templates.</param>
        /// <exception cref="ArgumentNullException"><c>expressionParameters</c> or <c>templateParameters</c> is <c>null</c>.</exception>
        public static void ProcessStringTemplates(
            VRCExpressionParameters expressionParameters, IDictionary<string, string> templateParameters)
        {
            if (expressionParameters == null) { throw new ArgumentNullException(nameof(expressionParameters)); }
            if (templateParameters == null) { throw new ArgumentNullException(nameof(templateParameters)); }

            var engine = new StringTemplateEngine(templateParameters);
            var names = new HashSet<string>();
            foreach (var p in expressionParameters.parameters)
            {
                var name = engine.Render(p.name);
                if (names.Contains(name)) { throw new ArgumentException("Processed names are not distinct"); }
                names.Add(name);
            }

            Undo.RecordObject(expressionParameters, "Process string templates");
            Cleanup(expressionParameters);
            expressionParameters.parameters = expressionParameters.parameters
                .Select(p => new VRCExpressionParameters.Parameter
                {
                    name = engine.Render(p.name),
                    valueType = p.valueType,
                    defaultValue = p.defaultValue,
                    saved = p.saved
                })
                .ToArray();
        }
    }
}