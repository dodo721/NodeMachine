using NodeMachine.Nodes;
using UnityEngine;
using System.Collections.Generic;

namespace NodeMachine.Nodes {

    [NodeInfo("Functions/Function")]
    public class FunctionNode : Node, ISerializationCallbackReceiver {

        public string name = "function";
        public bool editingName = false;

        [SerializeField]
        private List<int> functionGroupIDs = new List<int>();

        public FunctionNode (NodeMachineModel model, Vector2 position) : base(model) {
            transform.position = position;
            transform.size = new Vector2(150, 75);
            background = "Assets/NodeMachine/Editor/Editor Resources/function-node.png";
        }

        public HashSet<Node> GetFunctionGroup () {
            functionGroupIDs = new List<int>();
            HashSet<Node> functionGroup = new HashSet<Node>();
            FollowFunctionGroup(this, functionGroup);
            foreach (Node node in functionGroup) {
                functionGroupIDs.Add(node.ID);
            }
            return functionGroup;
        }

        void FollowFunctionGroup (Node fromNode, HashSet<Node> functionGroup) {
            foreach (Link link in fromNode.GetLinksFrom()) {
                Node to = model.GetNodeFromID(link._to);
                if (functionGroup.Contains(to))
                    return;
                functionGroup.Add(to);
                FollowFunctionGroup(to, functionGroup);
            }
        }

        public override string BeforeAddLink (Link link) {
            GetFunctionGroup();
            if (link._to == ID) {
                if (functionGroupIDs.Contains(model.GetNodeFromID(link._from).ID)) {
                    return "Nodes stemming from a function cannot lead back to it.\nIf you need a loop to the function node, use a Goto Function node instead.";
                }
            }
            return null;
        }

        public override void OnMouseDown () {
            GetFunctionGroup();
        }

        public override void OnDrag (Vector2 drag, float zoom) {
            foreach (int node in functionGroupIDs) {
                model.GetNodeFromID(node).Drag(drag, zoom);
            }
        }

        public void OnBeforeSerialize () {
            GetFunctionGroup();
        }

        public void OnAfterDeserialize () {
        }

    }

}