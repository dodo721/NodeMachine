using System.Collections;
using System.Collections.Generic;
using System;
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

        public bool running = false;
        protected Machine machine;
        protected NodeMachineProperties properties;
        private StateNode node;

        void Awake()
        {
            machine = GetComponent<Machine>();
            properties = machine.properties;
            node = StateNode.GetStateNodeFromType(machine.GetModel(), GetType());
        }

        public virtual void Checkin() {}

        protected void ActivateTrigger (string name) {
            node.ActivateTrigger(name);
        }

    }

}