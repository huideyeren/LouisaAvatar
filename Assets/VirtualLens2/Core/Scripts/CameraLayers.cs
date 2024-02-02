using System;

namespace VirtualLens2
{

    [Flags]
    public enum CameraLayers : int
    {
        Default             = (1 <<  0),
        TransparentFX       = (1 <<  1),
        IgnoreRaycast       = (1 <<  2),
        Water               = (1 <<  4),
        UI                  = (1 <<  5) | (1 <<  6) | (1 <<  7),
        Interactive         = (1 <<  8),
        Player              = (1 <<  9),
        PlayerLocal         = (1 << 10),
        Environment         = (1 << 11),
        UIMenu              = (1 << 12),
        Pickup              = (1 << 13),
        PickupNoEnvironment = (1 << 14),
        StereoLeft          = (1 << 15),
        StereoRight         = (1 << 16),
        Walkthrough         = (1 << 17),
        MirrorReflection    = (1 << 18),
        InternalUI          = (1 << 19),
        Reserved3           = (1 << 20),
        Reserved4           = (1 << 21),
        All                 = -1
    }

}
