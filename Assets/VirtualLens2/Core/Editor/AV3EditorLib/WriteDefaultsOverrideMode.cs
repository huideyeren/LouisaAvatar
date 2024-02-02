namespace VirtualLens2.AV3EditorLib
{
    /// <summary>
    /// Override mode for write defaults flag.
    /// </summary>
    public enum WriteDefaultsOverrideMode
    {
        /// <summary>
        /// Do not override write defaults flag.
        /// </summary>
        None,
        
        /// <summary>
        /// Disables write defaults flag for new animator states.
        /// </summary>
        ForceDisable,
        
        /// <summary>
        /// Enables write defaults flag for new animator states.
        /// </summary>
        ForceEnable
    }
}
