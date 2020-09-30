using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

namespace NodeMachine.Nodes {

    [Serializable]
    [NodeInfo("Conditional/Condition")]
    public class ConditionNode : Node
    {

        public Condition condition;
        
        public bool collapsed;

        [NonSerialized]
        public bool conditionMet = false;
        private object prevOutput;

        public ConditionNode(NodeMachineModel model, Condition condition, Vector2 position) : base(model)
        {
            background = "Assets/NodeMachine/Editor/Editor Resources/condition-node.png";
            transform = new Rect(position.x, position.y, 160, 130);
            this.condition = condition;
        }

        public void Collapse(bool collapse)
        {
            collapsed = collapse;
            if (collapsed)
            {
                transform.size = new Vector2(160, 50);
            }
            else
            {
                transform.size = new Vector2(160, 130);
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

        public override void OnEncountered(Node prevNode, Machine machine, NodeFollower context)
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

        bool ConditionMet(Machine machine)
        {
            Condition.ConditionType type = condition._valueType;
            string propName = condition._propName;
            if (condition._compareMode == Condition.CompareTo.PROP) {
                Type compType = Condition.FromConditionType(condition._valueType);
                condition.SetComparisonValue(Convert.ChangeType(machine.GetProp(condition._compPropName), compType));
            }
            if (type == Condition.ConditionType.FLOAT)
            {
                return condition.Compare((float)machine.GetProp(propName));
            }
            else if (type == Condition.ConditionType.INT)
            {
                return condition.Compare((int)machine.GetProp(propName));
            }
            else if (type == Condition.ConditionType.BOOL)
            {
                return condition.Compare((bool)machine.GetProp(propName));
            }
            else if (type == Condition.ConditionType.STRING)
            {
                return condition.Compare((string)machine.GetProp(propName));
            }
            else if (type == Condition.ConditionType.ENUM)
            {
                return condition.Compare(machine.GetProp(propName));
            }
            return false;
        }

        public override void OnLoad () {
            Validate();
        }
        
        public void OnAfterDeserialize() {
            Valid = true;
        }

        public void Validate () {
            if (!model.machinePropsSchema.ContainsKey(condition._propName) ||
                (condition._compareMode == Condition.CompareTo.PROP && !model.machinePropsSchema.ContainsKey(condition._compPropName))) {
                Valid = false;
            } else if (!Condition.FromConditionType(condition._valueType).IsAssignableFrom(model.machinePropsSchema[condition._propName])) {
                Valid = false;
            } else
                Valid = true;
            if (!Valid) {
                model.PushError(condition._propName + " is invalid for condition!", "Condition " + ToPrettyString() + " has an invalid property. Check the field name and type are correct.", this);
            }
        }

    }

}