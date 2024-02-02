using System;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualLens2.AV3EditorLib
{
    /// <summary>
    /// Contains parameter mappings for animation clip templates.
    /// </summary>
    public class AnimationTemplateParameters
    {
        private readonly IDictionary<string, List<GameObject>> _dictionary;

        /// <summary>
        /// Initializes as an empty mapping.
        /// </summary>
        public AnimationTemplateParameters()
        {
            _dictionary = new Dictionary<string, List<GameObject>>();
        }

        /// <summary>
        /// Add mapping from a name to an object.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="obj">The value of the element to add.</param>
        /// <exception cref="ArgumentNullException"><c>key</c> or <c>obj</c> is <c>null</c>.</exception>
        public void Add(string key, GameObject obj)
        {
            if (key == null) { throw new ArgumentNullException(nameof(key)); }
            if (obj == null) { throw new ArgumentNullException(nameof(obj)); }
            
            if (_dictionary.ContainsKey(key))
            {
                _dictionary[key].Add(obj);
            }
            else
            {
                _dictionary.Add(key, new List<GameObject>() {obj});
            }
        }

        /// <summary>
        /// Gets the objects associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the objects to get.</param>
        /// <exception cref="ArgumentNullException"><c>key</c> is <c>null</c>.</exception>
        public IEnumerable<GameObject> this[string key]
        {
            get
            {
                if (key == null) { throw new ArgumentNullException(nameof(key)); }
                if (_dictionary.ContainsKey(key)) { return _dictionary[key]; }
                return new GameObject[] { };
            }
        }
    }
}