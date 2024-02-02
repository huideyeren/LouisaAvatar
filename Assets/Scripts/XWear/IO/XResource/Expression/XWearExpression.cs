using System.Collections.Generic;
using UnityEngine;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource.Expression
{
    public class XWearExpression
    {
        public int ExpressionIndex;
        public List<ExpressionParametersContainer> ParameterContainers;
    }

    public class ExpressionParametersContainer
    {
        public string GameObjectGuid;
        public List<ExpressionParameter> Parameters = new List<ExpressionParameter>();
    }

    public class ExpressionParameter
    {
        public int BlendShapeIndex;
        public float Weight;
    }

    public class ExpressionCollector
    {
        public class ExpressionCollectSourceContainer
        {
            public int ExpressionIndex;
            public List<ExpressionCollectSourceContainer> CollectSourceContainers =
                new List<ExpressionCollectSourceContainer>();
        }

        public class ExpressionCollectSourceParameterContainer
        {
            public SkinnedMeshRenderer SkinnedMeshRenderer;
            public List<ExpressionParameter> Parameters = new List<ExpressionParameter>();
        }

        // public Dictionary<SkinnedMeshRenderer,>


        public void Collect(GameObjectWithTransformCollector gameObjectCollector)
        {
            // gameObjectCollector.GuidToClothItemGameObjectMemo
        }
    }
}
