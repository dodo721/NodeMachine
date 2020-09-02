
namespace NodeMachine.Nodes {
    [NodeGUI(typeof(ElseNode))]
    public class ElseNodeGUIContent : NodeGUIContent
    {

        public ElseNodeGUIContent(ElseNode node, NodeMachineEditor editor) : base(node, editor)
        {
            this.text = "else";
            this.shrinkTextWithZoom = true;
        }

    }
}