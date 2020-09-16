using System;
using System.Collections.Generic;
using UnityEngine;
using NodeMachine.Nodes;

namespace NodeMachine {

    [Serializable]
    public class Link : IDObject
    {

        public int _from;
        public int _to;
        private object passVal;

        public Rect _transform;

        public Link(Node from, Node to, int id) : base(id)
        {
            this._from = from.ID;
            this._to = to.ID;
        }

        public Link(int from, int to, int id) : base(id)
        {
            this._from = from;
            this._to = to;
        }

        /*
        public Rect RecalculateTransform () {
            Rect start = _from.transform;
            Rect end = _to.transform;
            Vector2 startPos = new Vector3(start.x + start.width / 2, start.y + start.height / 2);
            Vector2 endPos = new Vector3(end.x + end.width / 2, end.y + end.height / 2);
            Vector2 boxSize = new Vector2(25, 25);
            Vector2 boxPos = new Vector2((endPos.x - startPos.x) / 2 + startPos.x - (boxSize.x / 2), (endPos.y - startPos.y) / 2 + startPos.y - (boxSize.y / 2));
            _transform = new Rect(boxPos, boxSize);
            return _transform;
        }
        */

        public override string ToString()
        {
            return "[" + _from.ToString() + " -> " + _to.ToString() + "]";
        }

        public void ProcessEvents(Event e, Vector2 offset, Action<Link> ProcessContextMenu, Action<Link> Select)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 1 && _transform.Contains(e.mousePosition))
                    {
                        ProcessContextMenu(this);
                        e.Use();
                    }
                    else if (e.button == 0 && _transform.Contains(e.mousePosition))
                    {
                        Select(this);
                        e.Use();
                    }
                    break;
            }
        }
    }

}