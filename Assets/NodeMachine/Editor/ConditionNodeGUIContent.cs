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

        private string[] _comparisons;
        private Vector2 _curScroll = new Vector2();

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

        public override bool DrawContent(Event e)
        {

            bool modelNeedsSaving = false;
            ConditionNode node = _node as ConditionNode;
            string[] types = _editor._propTypesAvailable;
            GUIStyle wordWrap = new GUIStyle();
            wordWrap.wordWrap = true;

            if (node.collapsed)
            {
                text = node.ToPrettyString();
                return false;
            }

            text = "";

            Rect content = new Rect();
            content.size = Transform.size;
            content.width -= 20;
            content.height -= 50;
            content.position = Transform.position;
            content.x += 10;
            content.y += 25;

            GUILayout.BeginArea(content);

            string[] propNames = _editor._properties.GetPropNamesForType(node.condition._type);

            //_curScroll = EditorGUILayout.BeginScrollView(_curScroll);
            GUILayout.BeginVertical(wordWrap);

            int currentType = Array.IndexOf(types, node.condition._type.ToString());
            int newType = EditorGUILayout.Popup(currentType, types);
            if (newType != currentType)
            {
                modelNeedsSaving = true;
                node.condition.SetConditionType(Condition.ConditionTypeFromString(types[newType]));
            }

            int currentProp = Array.IndexOf(propNames, node.condition._propName);
            if (currentProp == -1)
            {
                node.condition._propName = propNames[0];
                currentProp = 0;
            }
            int newProp = EditorGUILayout.Popup(currentProp, propNames);
            if (newProp != currentProp)
            {
                modelNeedsSaving = true;
                node.condition._propName = propNames[newProp];
            }

            if (node.condition._type == Condition.ConditionType.BOOL)
            {
                EditorGUILayout.Popup(0, new string[] { "EQUAL" });
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
            if (node.condition._type == Condition.ConditionType.FLOAT)
            {
                float newValue = EditorGUILayout.FloatField(node.condition.GetComparisonValue());
                if (newValue != node.condition.GetComparisonValue())
                {
                    node.condition.SetComparisonValue(newValue);
                    modelNeedsSaving = true;
                }
            }
            else if (node.condition._type == Condition.ConditionType.INT)
            {
                int newValue = EditorGUILayout.IntField(node.condition.GetComparisonValue());
                if (newValue != node.condition.GetComparisonValue())
                {
                    node.condition.SetComparisonValue(newValue);
                    modelNeedsSaving = true;
                }
            }
            else if (node.condition._type == Condition.ConditionType.BOOL)
            {
                bool newValue = EditorGUILayout.Toggle(node.condition.GetComparisonValue(), GUILayout.ExpandWidth(false));
                if (newValue != node.condition.GetComparisonValue())
                {
                    node.condition.SetComparisonValue(newValue);
                    modelNeedsSaving = true;
                }
            }
            GUILayout.EndVertical();
            //EditorGUILayout.EndScrollView();

            GUILayout.EndArea();

            return modelNeedsSaving;
        }

    }

}