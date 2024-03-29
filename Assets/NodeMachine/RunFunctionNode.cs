using NodeMachine.Nodes;
using UnityEngine;
using System.Collections.Generic;

namespace NodeMachine.Nodes {

    [NodeInfo("Functions/Run Function")]
    public class RunFunctionNode : RunnableNode {

        public string function = "function";
        public bool continuous = false;
        Dictionary<NodeFollower, NodeFollower> followers = new Dictionary<NodeFollower, NodeFollower>();

        public RunFunctionNode (NodeMachineModel model, Vector2 position) : base(model) {
            transform.position = position;
            transform.size = new Vector2(150, 100);
            background = "builtin skins/darkskin/images/node2.png";
        }

        public override void OnRunStart (Machine machine, NodeFollower context) {
            if (followers == null)
                followers = new Dictionary<NodeFollower, NodeFollower>();
                
            if (!followers.ContainsKey(context)) {
                FunctionNode funcNode = machine._model.GetFunction(function);
                if (funcNode != null) {
                    followers.Add(context, new NodeFollower(machine, funcNode, context, continuous));
                } else {
                    Debug.LogError("Attempted to run non-existent function " + function + "!");
                }
            }
        }

        public override void OnRunEnd (Machine machine, NodeFollower context) {
            if (followers == null)
                followers = new Dictionary<NodeFollower, NodeFollower>();
                
            followers.Remove(context);
        }

        public override void Checkin (Machine machine, NodeFollower context) {
            if (followers[context].Active)
                machine.UpdateCurrents(followers[context].Checkin());
        }

        public override string ToString () {
            return "RUN " + function;
        }

    }

}