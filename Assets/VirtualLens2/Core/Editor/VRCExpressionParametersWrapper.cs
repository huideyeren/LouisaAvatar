using System;
using System.Collections.Generic;
using System.Linq;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace VirtualLens2
{
    internal class VrcExpressionParametersWrapper
    {
        public static bool SupportsNotSynchronizedVariables()
        {
            var type = typeof(VRCExpressionParameters.Parameter);
            var field = type.GetField("networkSynced");
            return field != null;
        }

        public class ParameterWrapper
        {
            public VRCExpressionParameters.Parameter Underlying { get; }

            public ParameterWrapper() { Underlying = new VRCExpressionParameters.Parameter(); }

            public ParameterWrapper(VRCExpressionParameters.Parameter p) { Underlying = p; }

            public int Cost
            {
                get
                {
                    if (Name == "") { return 0; }
                    return TypeCost(ValueType);
                }
            }

            public string Name
            {
                get => Underlying.name;
                set => Underlying.name = value;
            }

            public VRCExpressionParameters.ValueType ValueType
            {
                get => Underlying.valueType;
                set => Underlying.valueType = value;
            }

            public bool Saved
            {
                get
                {
                    var type = typeof(VRCExpressionParameters.Parameter);
                    var field = type.GetField("saved");
                    if (field == null) { return true; }
                    return (bool)field.GetValue(Underlying);
                }
                set
                {
                    var type = typeof(VRCExpressionParameters.Parameter);
                    var field = type.GetField("saved");
                    if (field == null) { return; }
                    field.SetValue(Underlying, value);
                }
            }

            public float DefaultValue
            {
                get
                {
                    var type = typeof(VRCExpressionParameters.Parameter);
                    var field = type.GetField("defaultValue");
                    if (field == null) { return 0.0f; }
                    return (float)field.GetValue(Underlying);
                }
                set
                {
                    var type = typeof(VRCExpressionParameters.Parameter);
                    var field = type.GetField("defaultValue");
                    if (field == null) { return; }
                    field.SetValue(Underlying, value);
                }
            }

            public bool Synchronized
            {
                get
                {
                    var type = typeof(VRCExpressionParameters.Parameter);
                    var field = type.GetField("networkSynced");
                    if (field == null) { return true; }
                    return (bool)field.GetValue(Underlying);
                }
                set
                {
                    var type = typeof(VRCExpressionParameters.Parameter);
                    var field = type.GetField("networkSynced");
                    if (field == null) { return; }
                    field.SetValue(Underlying, value);
                }
            }
        }


        private static bool HasParameterCost()
        {
            // Parameter cost is introduced in VRCSDK3-AVATAR-2021.01.xx
            var type = typeof(VRCExpressionParameters);
            return type.GetField("MAX_PARAMETER_COST") != null;
        }

        public static int MaxParameterCost()
        {
            var type = typeof(VRCExpressionParameters);
            var field = type.GetField("MAX_PARAMETER_COST");
            if (field == null) { return 128; }
            return (int)field.GetValue(null);
        }

        public static int TypeCost(VRCExpressionParameters.ValueType t)
        {
            var type = typeof(VRCExpressionParameters);
            var method = type.GetMethod("TypeCost");
            if (method == null) { return 8; }
            return (int)method.Invoke(null, new object[] { t });
        }


        private VRCExpressionParameters Underlying { get; }

        public VrcExpressionParametersWrapper(VRCExpressionParameters parameters) { Underlying = parameters; }

        public IEnumerable<ParameterWrapper> Parameters
        {
            get
            {
                foreach (var raw in Underlying.parameters)
                {
                    yield return new ParameterWrapper(raw);
                }
            }
        }

        public bool AddParameter(
            string name, VRCExpressionParameters.ValueType type,
            bool saved = true, float defaultValue = 0.0f, bool synchronized = true)
        {
            if (!HasParameterCost())
            {
                foreach (var p in Underlying.parameters)
                {
                    var wrapper = new ParameterWrapper(p);
                    if (wrapper.Name != "") { continue; }
                    wrapper.Name = name;
                    wrapper.ValueType = type;
                    wrapper.Saved = saved;
                    wrapper.DefaultValue = defaultValue;
                    wrapper.Synchronized = synchronized;
                    return true;
                }
                return false;
            }
            else
            {
                var wrapper = new ParameterWrapper
                {
                    Name = name,
                    ValueType = type,
                    Saved = saved,
                    DefaultValue = defaultValue,
                    Synchronized = synchronized
                };
                var list = Underlying.parameters.ToList();
                list.Add(wrapper.Underlying);
                Underlying.parameters = list.ToArray();
                return true;
            }
        }

        public void RemoveParameters(Func<ParameterWrapper, bool> predicate)
        {
            if (!HasParameterCost())
            {
                foreach (var p in Underlying.parameters)
                {
                    var wrapper = new ParameterWrapper(p);
                    if (predicate(wrapper))
                    {
                        wrapper.Name = "";
                        wrapper.ValueType = VRCExpressionParameters.ValueType.Int;
                        wrapper.Saved = true;
                        wrapper.DefaultValue = 0.0f;
                    }
                }
            }
            else
            {
                Underlying.parameters = Underlying.parameters
                    .Where(p => !predicate(new ParameterWrapper(p)))
                    .ToArray();
            }
        }

        public void Cleanup()
        {
            if (!HasParameterCost()) { return; }
            RemoveParameters(p => p.Name == "");
        }
    }
}
