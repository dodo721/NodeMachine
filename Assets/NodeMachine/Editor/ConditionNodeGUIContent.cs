using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using NodeMachine;

namespace NodeMachine.Nodes {

    [NodeGUI(typeof(ConditionNode))]
    public class ConditionNodeGUIContent : NodeGUIContent
    {

        private Dictionary<string, Condition.ConditionType> fieldNames = null;
        private string[] _comparisons;

        public ConditionNodeGUIContent(ConditionNode node, NodeMachineEditor editor) : base(node, editor)
        {

            List<Condition.Comparison> comparisonEnums = new List<Condition.Comparison>();
            foreach (Condition.Comparison comparison in (Condition.Comparison[])Enum.GetValues(typeof(Condition.Comparison)))
            {
                comparisonEnums.Add(comparison);
            }
            _comparisons = new string[comparisonEnums.Count];
            for (int i = 0; i < _comparisons.Length; i++)
            {
                _comparisons[i] = comparisonEnums[i].ToString();
            }
            shrinkTextWithZoom = true;

        }

        void CacheFieldNames () {
            fieldNames = new Dictionary<string, Condition.ConditionType>();
            if (_editor._model.machinePropsSchema.Count == 0)
                return;
            Dictionary<string, Type> template = _editor._model.machinePropsSchema;
            foreach (string fieldName in template.Keys) {
                Condition.ConditionType? conType = Condition.ParseConditionType(template[fieldName]);
                if (conType == null)
                    continue;
                fieldNames.Add(fieldName, (Condition.ConditionType)conType);
            }
        }

        public override bool DrawContent(Event e)
        {
            // TODO : CONDITIONS??
            
            bool modelNeedsSaving = false;
            ConditionNode node = _node as ConditionNode;

            if (fieldNames == null)
                CacheFieldNames();

            GUIStyle wordWrap = new GUIStyle();
            wordWrap.wordWrap = true;

            if (node.collapsed)
            {
                text = (!node.Valid ? "Invalid property!! " : "") + node.ToPrettyString();
                return false;
            }

            text = "";

            Rect content = new Rect();
            content.size = Transform.size;
            content.width -= 20;
            content.height -= 20;
            content.position = Transform.position;
            content.x += 10;
            content.y += 10;

            GUILayout.BeginArea(content);

            string[] propNames = fieldNames.Keys.ToArray();

            //_curScroll = EditorGUILayout.BeginScrollView(_curScroll);
            GUILayout.BeginVertical(wordWrap);

            GUILayout.FlexibleSpace();

            if (!node.Valid) {
                GUILayout.Label("Invalid property!!");
            }

            // Prop compare 1
            int currentProp = Array.IndexOf(propNames, node.condition._propName);
            if (currentProp == -1)
            {
                node.condition._propName = propNames[0];
                node.condition.SetConditionType(fieldNames[propNames[0]]);
                currentProp = 0;
            }
            int newProp = EditorGUILayout.Popup(currentProp, propNames);
            if (newProp != currentProp || !node.Valid)
            {
                modelNeedsSaving = true;
                node.condition._propName = propNames[newProp];
                node.condition.SetConditionType(fieldNames[propNames[newProp]]);
            }

            // Comparison type
            if (node.condition._valueType == Condition.ConditionType.BOOL || node.condition._valueType == Condition.ConditionType.STRING)
            {
                string[] comparisons = { "EQUAL", "NOT_EQUAL" };
                int currentComparison = Array.IndexOf(comparisons, node.condition._comparison.ToString());
                currentComparison = currentComparison == -1 ? 0 : currentComparison;
                int newComparison = EditorGUILayout.Popup(currentComparison, comparisons);
                if (newComparison != currentComparison)
                {
                    node.condition._comparison = Condition.ComparisonFromString(_comparisons[newComparison]);
                    modelNeedsSaving = true;
                }
            }
            else
            {
                int currentComparison = Array.IndexOf(_comparisons, node.condition._comparison.ToString());
                int newComparison = EditorGUILayout.Popup(currentComparison, _comparisons);
                if (newComparison != currentComparison)
                {
                    node.condition._comparison = Condition.ComparisonFromString(_comparisons[newComparison]);
                    modelNeedsSaving = true;
                }
            }

            // Prop compare 2
            HashSet<string> propsCompToList = new HashSet<string>();
            propsCompToList.Add("-constant-");
            foreach (string prop in propNames) {
                if (_editor._model.machinePropsSchema[prop] == Condition.FromConditionType(node.condition._valueType) && prop != node.condition._propName) {
                    propsCompToList.Add(prop);
                }
            }

            string[] propsCompTo = propsCompToList.ToArray();
            int curPropComp = Array.IndexOf(propsCompTo, node.condition._compPropName);
            curPropComp = curPropComp == -1 ? 0 : curPropComp; // If prop isnt found default to -constant-
            int selPropComp = EditorGUILayout.Popup(curPropComp, propsCompTo);
            if (selPropComp != curPropComp || !node.Valid) {
                if (selPropComp == 0) {
                    node.condition._compareMode = Condition.CompareTo.CONSTANT;
                    node.condition._compPropName = "";
                } else {
                    node.condition._compareMode = Condition.CompareTo.PROP;
                    node.condition._compPropName = propsCompTo[selPropComp];
                }
            }

            if (node.condition._compareMode == Condition.CompareTo.CONSTANT) {

                // Compare value input (for constant)
                if (node.condition._valueType == Condition.ConditionType.FLOAT)
                {
                    float newValue = EditorGUILayout.FloatField(node.condition.GetComparisonValue());
                    if (newValue != node.condition.GetComparisonValue())
                    {
                        node.condition.SetComparisonValue(newValue);
                        modelNeedsSaving = true;
                    }
                }
                else if (node.condition._valueType == Condition.ConditionType.INT)
                {
                    int newValue = EditorGUILayout.IntField(node.condition.GetComparisonValue());
                    if (newValue != node.condition.GetComparisonValue())
                    {
                        node.condition.SetComparisonValue(newValue);
                        modelNeedsSaving = true;
                    }
                }
                else if (node.condition._valueType == Condition.ConditionType.BOOL)
                {
                    bool newValue = EditorGUILayout.Toggle(node.condition.GetComparisonValue(), GUILayout.ExpandWidth(false));
                    if (newValue != node.condition.GetComparisonValue())
                    {
                        node.condition.SetComparisonValue(newValue);
                        modelNeedsSaving = true;
                    }
                }
                else if (node.condition._valueType == Condition.ConditionType.STRING)
                {
                    string newValue = EditorGUILayout.TextField(node.condition.GetComparisonValue());
                    if (newValue != node.condition.GetComparisonValue())
                    {
                        node.condition.SetComparisonValue(newValue);
                        modelNeedsSaving = true;
                    }
                }

            }

            GUILayout.FlexibleSpace();

            GUILayout.EndVertical();
            //EditorGUILayout.EndScrollView();

            GUILayout.EndArea();

            if (modelNeedsSaving)
                node.Validate();

            return modelNeedsSaving;
        }

    }

}