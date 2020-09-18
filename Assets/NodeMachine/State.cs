using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;
using UnityEngine;
using NodeMachine.Nodes;

namespace NodeMachine.States {

    /// <summary>
    ///  The base class from which all States must be derived.
    /// </summary>
    [Serializable]
    [RequireComponent(typeof(Machine))]
    [AddComponentMenu("")]
    public abstract class State : MonoBehaviour
    {
        protected Machine machine;
        private Dictionary<string, StateNode> nodes = new Dictionary<string, StateNode>();

        void Awake()
        {
            machine = GetComponent<Machine>();
            foreach (MethodInfo method in GetType().GetMethods().Where(method => method.GetCustomAttribute<StateAttribute>() != null)) {
                nodes.Add(method.Name, StateNode.GetStateNodeFromMethod(machine._model, GetType(), method.Name));
            }
        }

        public virtual void Checkin() {}

        protected void ActivateTrigger (string name, [CallerMemberName] string caller = null) {
            nodes[caller]?.ActivateTrigger(name);
        }

    }

}