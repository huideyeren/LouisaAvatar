using System;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualLens2.AV3EditorLib
{
    /// <summary>
    /// Contains parameter mappings for game object templates.
    /// </summary>
    public class GameObjectTemplateParameters
    {
        private readonly IDictionary<string, GameObject> _dictionary;

        /// <summary>
        /// Initializes as an empty mapping.
        /// </summary>
        public GameObjectTemplateParameters()
        {
            _dictionary = new Dictionary<string, GameObject>();
        }

        /// <summary>
        /// Add mapping from a name to an object.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <exception cref="ArgumentNullException"><c>key</c> or <c>obj</c> is <c>null</c>.</exception>
        public GameObjectTemplateParameters Add(string key, GameObject value)
        {
            if (key == null) { throw new ArgumentNullException(nameof(key)); }
            if (value == null) { throw new ArgumentNullException(nameof(key)); }
            _dictionary.Add(key, value);
            return this;
        }

        /// <summary>
        /// Determines whether this mapping contains the specified key.
        /// </summary>
        /// <param name="key">The key of the element.</param>
        /// <returns><c>true</c> if this mappings contains an element with key <c>motion</c>, or <c>false</c> if otherwise.</returns>
        public bool ContainsKey(string key) => _dictionary.ContainsKey(key);
        
        /// <summary>
        /// Gets the game object associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the element.</param>
        /// <returns>The motion associated with the specified key.</returns>
        public GameObject this[string key] => _dictionary[key];
    }
}