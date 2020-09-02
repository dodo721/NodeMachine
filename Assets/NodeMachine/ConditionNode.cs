using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

namespace NodeMachine.Nodes {

    [Serializable]
    [NodeInfo("Conditions/Condition")]
    public class ConditionNode : Node
    {

        public Condition condition;
        
        public bool collapsed;

        [NonSerialized]
        public bool conditionMet = false;

        public ConditionNode(NodeMachineModel model, Condition condition, Vector2 position) : base(model)
        {
            background = "Assets/NodeMachine/Editor/Editor Resources/condition-node.png";
            transform = new Rect(position.x, position.y, 150, 130);
            this.condition = condition;
        }

        public void Collapse(bool collapse)
        {
            collapsed = collapse;
            if (collapsed)
            {
                transform.size = new Vector2(150, 50);
            }
            else
            {
                transform.size = new Vector2(150, 130);
            }
        }

        public override string ToString()
        {
            return condition.ToString();
        }

        public string ToPrettyString()
        {
            return condition.ToPrettyString();
        }

        public override void OnEncountered(Node prevNode, Machine machine)
        {
            conditionMet = ConditionMet(machine);
        }

        public override Link[] NextLinks()
        {
            List<Link> links = new List<Link>();
            foreach (Link link in GetLinksFrom())
            {
                if (model.GetNodeFromID(link._to) is ElseNode == !conditionMet)
                    links.Add(link);
            }
            return links.ToArray();
        }

        public override bool CanCreateLinkFrom()
        {
            return true;
        }

        bool ConditionMet(Machine machine)
        {
            Condition.ConditionType type = condition._type;
            string propName = condition._propName;
            if (type == Condition.ConditionType.FLOAT)
            {
                return condition.Compare((float)machine.properties.GetProp(propName));
            }
            else if (type == Condition.ConditionType.INT)
            {
                return condition.Compare((int)machine.properties.GetProp(propName));
            }
            else if (type == Condition.ConditionType.BOOL)
            {
                return condition.Compare((bool)machine.properties.GetProp(propName));
            }
            return false;
        }

    }

}