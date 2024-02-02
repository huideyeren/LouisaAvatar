using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualLens2
{
    public enum BuildMode
    {
        Destructive = 0,
        NonDestructive = 1,
    }
    
    public enum GridType
    {
        None = 0,
        Grid3x3 = 1,
        Grid3x3Diag = 2,
        Grid6x4 = 3,
    }

    public enum PeakingMode
    {
        None = 0,
        MFOnly = 1,
        Always = 2,
    }

    public enum AutoFocusMode
    {
        Point = 0,
        Face = 1,
        Selfie = 2,
    }

    public enum AutoFocusTrackingSpeed
    {
        Immediate = 0,
        Fast = 1,
        Medium = 2,
        Slow = 3,
    }

    public enum FocusingSpeed
    {
        Immediate = 0,
        Fast = 1,
        Medium = 2,
        Slow = 3,
    }

    public enum PostAntialiasingMode
    {
        None = 0,
        FXAA = 1,
        SMAA = 2,
    }
    
    public enum WriteDefaultsOverrideMode
    {
        ForceDisable = 0,
        ForceEnable = 1,
        Auto = 2,
    }

    public enum RemoteOnlyMode
    {
        ForceDisable = 0,
        ForceEnable = 1,
        MobileOnly = 2,
    }
}
