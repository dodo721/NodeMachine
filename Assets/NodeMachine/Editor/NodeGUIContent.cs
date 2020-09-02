using UnityEngine;

namespace NodeMachine.Nodes {

    public abstract class NodeGUIContent
    {

        protected NodeMachineEditor _editor;
        protected Node _node;
        public string text;
        public bool shrinkTextWithZoom = false;

        public NodeGUIContent(Node node, NodeMachineEditor editor)
        {
            this._editor = editor;
            this._node = node;
        }

        protected Rect Transform
        {
            get
            {
                Rect transform = _node.drawnTransform;
                transform.position -= _editor._nodeEditor.position;
                return transform;
            }
        }

        /// <summary>
        ///  Draws custom content for the node. If true is returned, the model is marked as needing saved.
        /// </summary>
        public virtual bool DrawContent(Event e)
        {
            return false;
        }

    }

}