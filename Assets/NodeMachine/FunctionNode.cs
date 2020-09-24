using NodeMachine.Nodes;
using UnityEngine;
using System.Collections.Generic;

namespace NodeMachine.Nodes {

    [NodeInfo("Functions/Function")]
    public class FunctionNode : Node {

        public string name = "function";
        public bool editingName = false;
        private HashSet<Node> functionGroup;

        public FunctionNode (NodeMachineModel model, Vector2 position) : base(model) {
            transform.position = position;
            transform.size = new Vector2(150, 75);
            background = "Assets/NodeMachine/Editor/Editor Resources/function-node.png";
        }

        public HashSet<Node> GetFunctionGroup () {
            functionGroup = new HashSet<Node>();
            FollowFunctionGroup(this);
            return functionGroup;
        }

        void FollowFunctionGroup (Node fromNode) {
            foreach (Link link in fromNode.GetLinksFrom()) {
                Node to = model.GetNodeFromID(link._to);
                if (functionGroup.Contains(to))
                    return;
                functionGroup.Add(to);
                FollowFunctionGroup(to);
            }
        }

        public override string BeforeAddLink (Link link) {
            GetFunctionGroup();
            if (link._to == ID) {
                if (functionGroup.Contains(model.GetNodeFromID(link._from))) {
                    return "Nodes stemming from a function cannot lead back to it.\nIf you need a loop to the function node, use a Goto Function node instead.";
                }
            }
            return null;
        }

        public override void OnMouseDown () {
            GetFunctionGroup();
        }

        public override void OnDrag (Vector2 drag, float zoom) {
            foreach (Node node in functionGroup) {
                node.Drag(drag, zoom);
            }
        }

    }

}