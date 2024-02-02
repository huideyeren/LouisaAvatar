using System;
using UnityEngine;
using XWear.IO.XResource.Archive;

namespace XWear.IO.XResource.Component.Particle
{
    [Serializable]
    public class ExternalForcesModule
    {
        public bool enabled;
        public XResourceMinMaxCurve multiplierCurve;
        public float multiplier;
        public ParticleSystemGameObjectFilter influenceFilter;
        public LayerMask influenceMask;
        public int influenceCount;

        public XResourceParticleForceField[] influences =
            Array.Empty<XResourceParticleForceField>();

        public ExternalForcesModule() { }

        public ExternalForcesModule(
            ParticleSystem.ExternalForcesModule from,
            XResourceContainerUtil.XResourceArchiver archiver
        )
        {
            enabled = from.enabled;
            multiplier = from.multiplier;
            multiplierCurve = new XResourceMinMaxCurve(from.multiplierCurve);
            influenceFilter = from.influenceFilter;
            influenceMask = from.influenceMask;
            influenceCount = from.influenceCount;
            influences = new XResourceParticleForceField[influenceCount];
            for (int i = 0; i < influenceCount; i++)
            {
                var influence = from.GetInfluence(i);
                if (influences != null)
                {
                    influences[i] = new XResourceParticleForceField(influence, archiver);
                }
            }
        }

        public static void CopyUnityComponent(
            ParticleSystem.ExternalForcesModule from,
            ParticleSystem.ExternalForcesModule to
        )
        {
            to.enabled = from.enabled;
            to.multiplier = from.multiplier;
            to.multiplierCurve = new XResourceMinMaxCurve(from.multiplierCurve).ToUnity();
            to.influenceFilter = from.influenceFilter;
            to.influenceMask = from.influenceMask;
            for (int i = 0; i < from.influenceCount; i++)
            {
                var influence = from.GetInfluence(i);
                to.AddInfluence(influence);
            }
        }

        public void SetTo(
            ParticleSystem.ExternalForcesModule to,
            ParticleSystem root,
            XResourceContainerUtil.XResourceOpener opener
        )
        {
            to.enabled = enabled;
            to.multiplier = multiplier;
            to.multiplierCurve = multiplierCurve.ToUnity();
            to.influenceFilter = influenceFilter;
            to.influenceMask = influenceMask;
            for (int i = 0; i < influenceCount; i++)
            {
                if (influences[i] == null)
                {
                    continue;
                }

                var newInfluence = influences[i].AddParticleForceField(root, opener);
                to.AddInfluence(newInfluence);
            }
        }
    }
}
