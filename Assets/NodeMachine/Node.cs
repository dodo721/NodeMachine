using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using SimpleJSON;

namespace NodeMachine.Nodes {

    /// <summary>
    ///  Represents a node on a state machine.
    /// </summary>
    /// <remarks>
    ///  This class can be extended to create new node types.
    ///  Nodes by default are static. To create a runnable node, extend <c>RunnableNode</c> instead.
    /// </remarks>
    [Serializable]
    [NodeInfo(false)]
    public class Node : IDObject
    {
        
        public Rect transform;

        [NonSerialized]
        public Rect drawnTransform;
        public string background;
        private bool _dragTarget = false;
        public bool Valid {
            get;
            protected set;
        } = true;

        [NonSerialized]
        public NodeMachineModel model;

        // Store links by ID: links serialize their nodes, thus serializing links within nodes too would be recursive
        public List<int> linkIDs = new List<int>();

        public Node(NodeMachineModel model, Vector2 position) : base(model.GetFreeNodeID())
        {
            this.transform.position = position;
            this.model = model;
        }

        public Node(NodeMachineModel model) : base(model.GetFreeNodeID())
        {
            transform = new Rect();
            this.model = model;
        }

    #if UNITY_EDITOR
        public bool ProcessEvents(Event e, float zoom, Action<Node> Select, Action<Node> ProcessContextMenu)
        {
            bool hasMouse = drawnTransform.Contains(e.mousePosition);
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0 && hasMouse)
                    {
                        Select(this);
                        _dragTarget = true;
                        GUI.changed = true;
                        e.Use();
                    }
                    if (e.button == 1 && hasMouse)
                    {
                        // Select and menu
                        Select(this);
                        ProcessContextMenu(this);
                        GUI.changed = true;
                        e.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (_dragTarget)
                    {
                        _dragTarget = false;
                        return true;
                    }
                    break;
                case EventType.MouseDrag:
                    if (e.button == 0 && _dragTarget)
                    {
                        Drag(e.delta, zoom);
                        e.Use();
                        return false;
                    }
                    break;
            }
            return false;
        }
    #endif

        void Drag(Vector2 delta, float zoom)
        {
            transform.position += delta * zoom;
        }

        public void AddLink(Link link)
        {
            linkIDs.Add(link.ID);
            OnAddLink(link);
        }

        public List<Link> GetLinksFrom()
        {
            List<Link> fromLinks = new List<Link>();
            foreach (int linkID in linkIDs)
            {
                Link link = model.GetLinkFromID(linkID);
                if (link._from == this.ID)
                    fromLinks.Add(link);
            }
            return fromLinks;
        }

        public List<Link> GetLinksTo()
        {
            List<Link> toLinks = new List<Link>();
            foreach (int linkID in linkIDs)
            {
                Link link = model.GetLinkFromID(linkID);
                if (link._to == this.ID)
                    toLinks.Add(link);
            }
            return toLinks;
        }

        public override string ToString () {
            return this.GetType().ToString().Replace("NodeMachine.Nodes.", "");
        }

        /// <summary>
        ///  Tells the editor if this node can support a new link from it.
        /// </summary>
        public virtual bool CanCreateLinkFrom()
        {
            return true;
        }

        /// <summary>
        ///  Tells the editor if this node can support a new link to it.
        /// </summary>
        public virtual bool CanCreateLinkTo()
        {
            return true;
        }

        /// <summary>
        ///  Tells the editor if this node can be removed from the model.
        /// </summary>
        public virtual bool CanBeRemoved()
        {
            return true;
        }

        /// <summary>
        ///  If true, the machine will stop following links at this point.
        /// </summary>
        /// <remarks>
        ///  Note: if this node is runnable, the machine will run this node.
        /// </remarks>
        public virtual bool IsBlocking()
        {
            return false;
        }

        /// <summary>
        ///  Returns the next links to be tested by the machine.
        /// </summary>
        /// <remarks>
        ///  If <c>null</c> is returned, the machine will default to testing all connected links.<br/>
        ///  Note: returning an empty <c>Link</c> array has the same effect as if <c>IsBlocking</c> returned <c>true</c>.
        /// </remarks>
        public virtual Link[] NextLinks()
        {
            return null;
        }

        /// <summary>
        ///  Triggered just before a link is created. If false, the link is not created.
        /// </summary>
        public virtual bool BeforeAddLink(Link link)
        {
            return true;
        }

        /// <summary>
        ///  Triggered when a link is added to this node.
        /// </summary>
        public virtual void OnAddLink(Link link) { }

        /// <summary>
        ///  Triggered when the machine wakes up.
        /// </summary>
        public virtual void OnGameStart(Machine machine) { }

        /// <summary>
        ///  Triggered when this node is encountered while the machine is searching for the next node.
        /// </summary>
        public virtual void OnEncountered(Node prevNode, Machine machine) { }

        /// <summary>
        ///  Triggered when this node is passed while the machine is searching for the next node.
        /// </summary>
        /// <remarks>
        ///  Note: if the model does not support parallel flow, then only the first element of <c>nextNodes</c> will be followed.
        /// </remarks>
        public virtual void OnPassed(HashSet<Node> nextNodes, Machine machine) { }

        /// <summary>
        ///  Called when this node is loaded to the model.
        /// </summary>
        public virtual void OnLoad () { }

    }

}