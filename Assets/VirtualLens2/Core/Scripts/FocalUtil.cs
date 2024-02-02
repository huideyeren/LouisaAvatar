using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualLens2
{

    public class FocalUtil
    {
        public static float AdjustFov(float fov)
        {
            const float HEIGHT_SCALE = 1152.0f / 1080.0f;
            return 2.0f * Mathf.Atan(HEIGHT_SCALE * Mathf.Tan(0.5f * fov * Mathf.Deg2Rad)) * Mathf.Rad2Deg;
        }

        public static float Fov2Focal(float fov)
        {
            return 10.125f / Mathf.Tan(0.5f * fov * Mathf.Deg2Rad);
        }

        public static float Fov2Zoom(float fov)
        {
            return Mathf.Tan(30.0f * Mathf.Deg2Rad) / Mathf.Tan(0.5f * fov * Mathf.Deg2Rad);
        }

        public static float Focal2Fov(float focal)
        {
            return 2.0f * Mathf.Atan(10.125f / focal) * Mathf.Rad2Deg;
        }

        public static float Focal2Zoom(float focal)
        {
            return Fov2Zoom(Focal2Fov(focal));
        }

        public static float Zoom2Fov(float zoom)
        {
            return 2.0f * Mathf.Atan(Mathf.Tan(30.0f * Mathf.Deg2Rad) / zoom) * Mathf.Rad2Deg;
        }

        public static float Zoom2Focal(float zoom)
        {
            return Fov2Focal(Zoom2Fov(zoom));
        }
    }

}
