using NodeMachine.Nodes;
using UnityEngine;
using System.Collections.Generic;

namespace NodeMachine.Nodes {

    [NodeInfo("Functions/Event Function")]
    public class EventFunctionNode : Node {

        public string function = "function";
        Dictionary<NodeFollower, NodeFollower> followers = new Dictionary<NodeFollower, NodeFollower>();

        public EventFunctionNode (NodeMachineModel model, Vector2 position) : base(model) {
            transform.position = position;
            transform.size = new Vector2(150, 100);
            background = "builtin skins/darkskin/images/node1.png";
        }

        public override void OnEncountered (Node prevNode, Machine machine, NodeFollower context) {
            if (followers == null)
                followers = new Dictionary<NodeFollower, NodeFollower>();
                
            if (!followers.ContainsKey(context)) {
                FunctionNode funcNode = machine._model.GetFunction(function);
                if (funcNode != null) {
                    followers.Add(context, new NodeFollower(machine, funcNode, context, true));
                } else {
                    Debug.LogError("Attempted to run non-existent function " + function + "!");
                }
            }
            if (followers[context].Active)
                machine.UpdateCurrents(followers[context].Checkin());
        }

        public override string ToString () {
            return "EVENT " + function;
        }

    }

}