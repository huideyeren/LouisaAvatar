using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

// ver 0.0.1
// Copyright (c) 2021 TKSP

namespace VGC
{
    public static class WriteDefaultUtil
    {
        public static void WriteDefaultChangeRecursive(AnimatorStateMachine stateMachine, bool enabled)
        {
            foreach (var chiledState in stateMachine.states)
            {
                chiledState.state.writeDefaultValues = enabled;
            }

            foreach (var chiledStateMachine in stateMachine.stateMachines)
            {
                WriteDefaultChangeRecursive(chiledStateMachine.stateMachine, enabled);
            }
        }

        public static void WriteDefaultChangeByName(AnimatorController controller, string layerName, bool enabled)
        {
            foreach (var layer in controller.layers)
            {
                if (layer.name == layerName)
                {
                    WriteDefaultChangeRecursive(layer.stateMachine, enabled);
                    return;
                }
            }
        }

        public static void WriteDefaultChangeAll(AnimatorController controller, bool enabled)
        {
            foreach (var layer in controller.layers)
            {
                WriteDefaultChangeRecursive(layer.stateMachine, enabled);
            }
        }

    }
}