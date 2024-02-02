using System;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualLens2.AV3EditorLib
{
    /// <summary>
    /// Contains motion-to-motion mappings for animator controller templates.
    /// </summary>
    public class MotionTemplateParameters
    {
        private readonly IDictionary<Motion, Motion> _dictionary;

        /// <summary>
        /// Initializes as an empty mapping.
        /// </summary>
        public MotionTemplateParameters()
        {
            _dictionary = new Dictionary<Motion, Motion>(new ReferenceEqualityComparer<Motion>());
        }

        /// <summary>
        /// Add mapping from a placeholder motion to an actual motion.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <returns>The reference to <c>this</c>.</returns>
        /// <exception cref="ArgumentNullException"><c>key</c> or <c>value</c> is <c>null</c>.</exception>
        public MotionTemplateParameters Add(Motion key, Motion value)
        {
            if (key == null) { throw new ArgumentNullException(nameof(key)); }
            if (value == null) { throw new ArgumentNullException(nameof(value)); }
            _dictionary.Add(key, value);
            return this;
        }

        /// <summary>
        /// Determines whether this mapping contains the specified key.
        /// </summary>
        /// <param name="motion">The key of the element.</param>
        /// <returns><c>true</c> if this mappings contains an element with key <c>motion</c>, or <c>false</c> if otherwise.</returns>
        public bool ContainsKey(Motion motion) => _dictionary.ContainsKey(motion);

        /// <summary>
        /// Gets the motion associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the element.</param>
        /// <returns>The motion associated with the specified key.</returns>
        public Motion this[Motion key] => _dictionary[key];
    }
}