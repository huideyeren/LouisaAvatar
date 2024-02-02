using UnityEngine;

namespace VirtualLens2
{
    public static class TransformUtil
    {
        public static void ResetLocalTransform(Transform tf)
        {
            tf.localPosition = new Vector3();
            tf.localRotation = new Quaternion();
            if(tf.localScale.sqrMagnitude == 0.0f)
            {
                tf.localScale = new Vector3();
            }
            else
            {
                tf.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                var rcp = tf.lossyScale;
                tf.localScale = new Vector3(1.0f / rcp.x, 1.0f / rcp.y, 1.0f / rcp.z);
            }
        }
    }
}
