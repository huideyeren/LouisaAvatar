namespace VirtualLens2
{
    public static class Constants
    {
        public const int Version = 21001;
        
        public const string ParameterPrefix = "VirtualLens2 ";

        public const int NumPins = 4;
        public const int NumQuickCalls = 8;
        public const int NumCustomGrids = 4;

        public const float MaxScaling = 1024.0f;
    }

    public enum MenuTrigger
    {
        // Root
        Enable = 100,
        // Transform Control
        Pickup = 102,
        Reposition = 103,
        // Transform Control / Auto Leveler
        AutoLevelerDisable = 104,
        AutoLevelerHorizontal = 105,
        AutoLevelerVertical = 106,
        AutoLevelerAuto = 107,
        // Transform Control / More / Pins
        Pin0 = 110,
        Pin1 = 111,
        Pin2 = 112,
        Pin3 = 113,
        // Transform Control / More / Reposition Scale
        RepositionScale1X = 114,
        RepositionScale3X = 115,
        RepositionScale10X = 116,
        RepositionScale30X = 117,
        // Image Control / AF Mode
        PointAutoFocus = 118,
        FaceAutoFocus = 119,
        SelfieAutoFocus = 120,
        // Image Control / Tracking Speed
        TrackingSpeedImmediate = 122,
        TrackingSpeedFast = 123,
        TrackingSpeedMedium = 124,
        TrackingSpeedSlow = 125,
        // Image Control / Focusing Speed
        FocusingSpeedImmediate = 126,
        FocusingSpeedFast = 127,
        FocusingSpeedMedium = 128,
        FocusingSpeedSlow = 129,
        // Advanced Settings
        MeshVisibility = 130,
        // Advanced Settings / Display Settings
        DisplayInformation = 132,
        DisplayLeveler = 133,
        // Advanced Settings / Display Settings / Grid
        GridNone = 134,
        Grid3X3= 135,
        Grid3X3Diag = 136,
        Grid6X4 = 137,
        // Advanced Settings / Display Settings / Peaking
        PeakingNone = 138,
        PeakingManualOnly = 139,
        PeakingAlways = 140,
        // Advanced Settings / Far Plane
        FarPlaneDefault = 142,
        FarPlane10X = 143,
        FarPlane100X = 144,
        // Advanced Settings / Transfer Mode (removed in v2.9.3)
        //   TransferPermissive = 146,
        //   TransferStrict = 147,
        // Advanced Settings / Depth Enabler
        DepthEnablerDisable = 148,
        DepthEnablerEnable = 149,
        // Quick Calls
        QuickCall0 = 150,
        QuickCall1 = 151,
        QuickCall2 = 152,
        QuickCall3 = 153,
        QuickCall4 = 154,
        QuickCall5 = 155,
        QuickCall6 = 156,
        QuickCall7 = 157,
        // Transform Control / More / Stabilizer
        StabilizerDisable = 158,
        StabilizerWeak = 159,
        StabilizerMedium = 160,
        StabilizerStrong = 161,
        // Advanced Settings / Display Settings / Grid (cont.)
        GridCustom0 = 162,
        GridCustom1 = 163,
        GridCustom2 = 164,
        GridCustom3 = 165,
    }

    public enum PositionMode
    {
        Neutral = 0,
        Drop = 1,
        Reposition = 2,
        Drone = 3
    }
}
