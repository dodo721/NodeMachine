using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;
using NodeMachine.Nodes;

namespace NodeMachine.Util {

    public static class NodeMachineGUIUtils
    {

        private static GUIStyle _stateNodeStyle = null;
        private static GUIStyle _linkBoxStyle = null;
        private static GUIStyle _linkBoxStyleClicked = null;
        private static GUIStyle _linkBoxStyle_t = null;
        private static GUIStyle _linkBoxStyleClicked_t = null;
        private static GUIStyle _nothingOpenStyle = null;
        private static Dictionary<string, Texture2D> cachedBackgrounds = new Dictionary<string, Texture2D>();
        private static Dictionary<Node, NodeGUIContent> cachedNodeGUIContents = new Dictionary<Node, NodeGUIContent>();

        private static Texture2D arrowCapTex = null;
        private static Texture2D arrowCapTex_t = null;
        private static Texture2D whiteLineTex_t = null;
        private static Texture2D greyBoxTex = null;
        private static Texture2D greyBoxTex_t = null;
        private static Texture2D greyBoxInvertedTex = null;
        private static Texture2D greyBoxInvertedTex_t = null;
        private static Texture2D runningIconTex = null;
        private static Texture2D nodeActive = null;

        /// <summary>
        ///  Initial set up method.
        /// </summary>
        /// <remarks>
        ///  Must be called before any useage.
        /// </remarks>
        public static void Init()
        {
            arrowCapTex = EditorGUIUtility.Load("Assets/NodeMachine/Editor/Editor Resources/arrow-icon-14-16.png") as Texture2D;
            arrowCapTex_t = EditorGUIUtility.Load("Assets/NodeMachine/Editor/Editor Resources/arrow-icon-14-16_t.png") as Texture2D;
            whiteLineTex_t = EditorGUIUtility.Load("Assets/NodeMachine/Editor/Editor Resources/white_line_t.png") as Texture2D;
            greyBoxTex = EditorGUIUtility.Load("Assets/NodeMachine/Editor/Editor Resources/small-grey-box.png") as Texture2D;
            greyBoxTex_t = EditorGUIUtility.Load("Assets/NodeMachine/Editor/Editor Resources/small-grey-box.png_t") as Texture2D;
            greyBoxInvertedTex = EditorGUIUtility.Load("Assets/NodeMachine/Editor/Editor Resources/small-grey-box-inverted.png") as Texture2D;
            greyBoxInvertedTex_t = EditorGUIUtility.Load("Assets/NodeMachine/Editor/Editor Resources/small-grey-box-inverted_t.png") as Texture2D;
            runningIconTex = EditorGUIUtility.Load("Assets/NodeMachine/Editor/Editor Resources/running.png") as Texture2D;
            nodeActive = EditorGUIUtility.Load("Assets/Editor Resource/node-active.png") as Texture2D;

            _stateNodeStyle = new GUIStyle();
            _stateNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
            _stateNodeStyle.border = new RectOffset(12, 12, 12, 12);
            _stateNodeStyle.padding = new RectOffset(12, 12, 12, 12);
            _stateNodeStyle.alignment = TextAnchor.MiddleCenter;
            _stateNodeStyle.wordWrap = true;

            _nothingOpenStyle = new GUIStyle();
            _nothingOpenStyle.border = new RectOffset(12, 12, 12, 12);
            _nothingOpenStyle.padding = new RectOffset(12, 12, 12, 12);
            _nothingOpenStyle.alignment = TextAnchor.MiddleCenter;
            _nothingOpenStyle.fontSize = 20;

            _linkBoxStyle = new GUIStyle();
            _linkBoxStyle.normal.background = greyBoxTex;
            _linkBoxStyle.border = new RectOffset(1, 1, 1, 1);
            _linkBoxStyle.padding = new RectOffset(1, 1, 1, 1);
            _linkBoxStyle.alignment = TextAnchor.MiddleCenter;

            _linkBoxStyleClicked = new GUIStyle();
            _linkBoxStyleClicked.normal.background = greyBoxInvertedTex;
            _linkBoxStyleClicked.border = new RectOffset(1, 1, 1, 1);
            _linkBoxStyleClicked.padding = new RectOffset(1, 1, 1, 1);
            _linkBoxStyleClicked.alignment = TextAnchor.MiddleCenter;

            _linkBoxStyle_t = new GUIStyle();
            _linkBoxStyle_t.normal.background = greyBoxTex_t;
            _linkBoxStyle_t.border = new RectOffset(1, 1, 1, 1);
            _linkBoxStyle_t.padding = new RectOffset(1, 1, 1, 1);
            _linkBoxStyle_t.alignment = TextAnchor.MiddleCenter;

            _linkBoxStyleClicked_t = new GUIStyle();
            _linkBoxStyleClicked_t.normal.background = greyBoxInvertedTex_t;
            _linkBoxStyleClicked_t.border = new RectOffset(1, 1, 1, 1);
            _linkBoxStyleClicked_t.padding = new RectOffset(1, 1, 1, 1);
            _linkBoxStyleClicked_t.alignment = TextAnchor.MiddleCenter;
        }

        /// <summary>
        ///  Draws a node to the editor.
        /// </summary>
        /// <param name="node">The node to draw.</param>
        /// <param name="editor">The <c>NodeMachineEditor</c> to draw to.</param>
        public static bool DrawNode(Node node, NodeMachineEditor editor, Event e)
        {
            return DrawNode(node, null, editor, e);
        }

        /// <summary>
        ///  Draws a node to the editor.
        /// </summary>
        /// <param name="node">The node to draw.</param>
        /// <param name="overrideBackground">If set, the node will be drawn with the given texture path instead of the one given by the node.</param>
        /// <param name="editor">The <c>NodeMachineEditor</c> to draw to.</param>
        public static bool DrawNode(Node node, string overrideBackground, NodeMachineEditor editor, Event e)
        {
            if (!node.visible)
                if (!editor._showInivisibleNodes)
                    return false;
            Vector2 drawnNodePos = (node.transform.position / editor._zoom) + editor._offset;

            Rect nodeRect = new Rect(
                drawnNodePos.x,
                drawnNodePos.y,
                node.transform.width / editor._zoom,
                node.transform.height / editor._zoom);
            nodeRect.position -= new Vector2(nodeRect.width / 2, nodeRect.height / 2);
            GUIStyle style = _stateNodeStyle;

            string background = overrideBackground == null || overrideBackground == "" ? node.background : overrideBackground;

            if (!cachedBackgrounds.ContainsKey(background))
            {
                cachedBackgrounds.Add(background, EditorGUIUtility.Load(background) as Texture2D);
            }
            style.normal.background = cachedBackgrounds[background];

            /*
            string title = "";
            if (node is StateNode) {
                title = (node as StateNode).ToString();
            }
            */

            NodeGUIContent nodeGUIContent = null;
            if (cachedNodeGUIContents.ContainsKey(node))
                nodeGUIContent = cachedNodeGUIContents[node];
            else
            {
                Type nodeType = node.GetType();
                Assembly assembly = Assembly.Load("Assembly-CSharp-Editor");

                Type guiContentType = assembly.GetTypes().Where(t =>
                    t.GetCustomAttribute<NodeGUIAttribute>()?.NodeType == nodeType
                    && typeof(NodeGUIContent).IsAssignableFrom(t)
                ).LastOrDefault();

                if (guiContentType != null)
                {
                    nodeGUIContent = System.Activator.CreateInstance(guiContentType, new object[] { node, editor }) as NodeGUIContent;
                }
                cachedNodeGUIContents.Add(node, nodeGUIContent);
            }
            
            if (node is RunnableNode && EditorApplication.isPlaying && editor._selectedMachine != null) {
                if (editor._selectedMachine.CurrentRunnables.Contains(node)) {
                    GUI.Box(nodeRect, nodeActive);
                }
            }

            int fontSize = 12;
            if (nodeGUIContent?.shrinkTextWithZoom == true)
            {
                style.fontSize = (int)Mathf.Round(fontSize / editor._zoom);
            }
            GUI.Box(nodeRect, (nodeGUIContent != null ? nodeGUIContent.text : node.ToString()), style);
            style.fontSize = fontSize;

            Rect drawnTransform = new Rect();
            drawnTransform.position = nodeRect.position + editor._nodeEditor.position;
            drawnTransform.size = nodeRect.size;
            node.drawnTransform = drawnTransform;

            bool needsSaving = nodeGUIContent?.DrawContent(e) == true;

            if (node is RunnableNode && EditorApplication.isPlaying && editor._selectedMachine != null) {
                if (editor._selectedMachine.CurrentRunnables.Contains(node)) {
                    GUI.DrawTexture(new Rect(nodeRect.position + new Vector2(12.5f, 12.5f), new Vector2(25,25)), runningIconTex);
                }
            }

            return needsSaving;
        }

        /// <summary>
        ///  Draws a link between 2 nodes.
        /// </summary>
        /// <param name="link">The link to draw.</param>
        /// <param name="active">For live preview: true if the link is currently in use.</param>
        /// <param name="editor">The <c>NodeMachineEditor</c> to draw to.</param>
        public static void DrawLink(Link link, bool active, NodeMachineEditor editor)
        {
            if (!editor._model.GetNodeFromID(link._from).visible || !editor._model.GetNodeFromID(link._to).visible)
                if (!editor._showInivisibleNodes)
                    return;
            
            Rect start = editor._model.GetNodeFromID(link._from).drawnTransform;
            Rect end = editor._model.GetNodeFromID(link._to).drawnTransform;
            start.position -= editor._nodeEditor.position;
            end.position -= editor._nodeEditor.position;
            float dirOffset = start.y > end.y ? -15 / editor._zoom : 15 / editor._zoom;
            Vector3 startPos = new Vector3(start.x + start.width / 2 + dirOffset, start.y + start.height / 2, 0);
            Vector3 endPos = new Vector3(end.x + end.width / 2 + dirOffset, end.y + end.height / 2, 0);
            Vector2 arrowPos = new Vector2((endPos.x - startPos.x) / 2 + startPos.x, (endPos.y - startPos.y) / 2 + startPos.y);
            Handles.color = Color.white;

            Handles.DrawAAPolyLine(5f, startPos, endPos);

            Rect transform = new Rect(arrowPos.x - (25 / 2), arrowPos.y - (25 / 2), 25f, 25f);
            link._transform = new Rect(transform.position + editor._nodeEditor.position, transform.size);

            GUIStyle style = _linkBoxStyle;
            if (active)
                style = _linkBoxStyleClicked;

            GUI.Box(transform, "", style);

            float xDis = endPos.x - startPos.x;
            float yDis = endPos.y - startPos.y;
            float arrowAngle = Mathf.Rad2Deg * Mathf.Atan2(yDis, xDis);
            GUIUtility.RotateAroundPivot(arrowAngle, arrowPos);
            GUI.DrawTexture(new Rect(arrowPos.x - 5, arrowPos.y - 5, 10, 10), arrowCapTex, ScaleMode.StretchToFill);
            GUIUtility.RotateAroundPivot(-arrowAngle, arrowPos);
        }

        /// <summary>
        ///  Draws a transparent link between 2 nodes.
        /// </summary>
        /// <param name="link">The link to draw.</param>
        /// <param name="active">For live preview: true if the link is currently in use.</param>
        /// <param name="editor">The <c>NodeMachineEditor</c> to draw to.</param>
        public static void DrawTransparentLink(Link link, bool active, NodeMachineEditor editor)
        {
            if (!editor._model.GetNodeFromID(link._from).visible || !editor._model.GetNodeFromID(link._to).visible)
                if (!editor._showInivisibleNodes)
                    return;
            
            Rect start = editor._model.GetNodeFromID(link._from).drawnTransform;
            Rect end = editor._model.GetNodeFromID(link._to).drawnTransform;
            start.position -= editor._nodeEditor.position;
            end.position -= editor._nodeEditor.position;
            float dirOffset = start.y > end.y ? -15 / editor._zoom : 15 / editor._zoom;
            Vector3 startPos = new Vector3(start.x + start.width / 2 + dirOffset, start.y + start.height / 2, 0);
            Vector3 endPos = new Vector3(end.x + end.width / 2 + dirOffset, end.y + end.height / 2, 0);
            Vector2 arrowPos = new Vector2((endPos.x - startPos.x) / 2 + startPos.x, (endPos.y - startPos.y) / 2 + startPos.y);
            Handles.color = Color.white;

            Handles.DrawAAPolyLine(whiteLineTex_t, 5f, startPos, endPos);

            Rect transform = new Rect(arrowPos.x - (25 / 2), arrowPos.y - (25 / 2), 25f, 25f);
            link._transform = new Rect(transform.position + editor._nodeEditor.position, transform.size);

            GUIStyle style = _linkBoxStyle_t;
            if (active)
                style = _linkBoxStyleClicked_t;

            GUI.Box(transform, "", style);

            float xDis = endPos.x - startPos.x;
            float yDis = endPos.y - startPos.y;
            float arrowAngle = Mathf.Rad2Deg * Mathf.Atan2(yDis, xDis);
            GUIUtility.RotateAroundPivot(arrowAngle, arrowPos);
            GUI.DrawTexture(new Rect(arrowPos.x - 5, arrowPos.y - 5, 10, 10), arrowCapTex_t, ScaleMode.StretchToFill);
            GUIUtility.RotateAroundPivot(-arrowAngle, arrowPos);
        }

        /// <summary>
        ///  Draws a grid to the node editor.
        /// </summary>
        /// <param name="gridSpacingUnzoomed">The spacing between grid lines.</param>
        /// <param name="gridOpacity">The opacity to draw lines with.</param>
        /// <param name="gridColor">The color to draw lines with.</param>
        /// <param name="editor">The <c>NodeMachineEditor</c> to draw to.</param>
        public static void DrawGrid(float gridSpacingUnzoomed, float gridOpacity, Color gridColor, NodeMachineEditor editor)
        {
            float gridSpacing = gridSpacingUnzoomed / editor._zoom;

            int widthDivs = Mathf.CeilToInt(editor._nodeEditor.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(editor._nodeEditor.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            Vector2 newOffset = new Vector3(editor._offset.x % gridSpacing, editor._offset.y % gridSpacing);

            for (int i = 0; i < widthDivs; i++)
            {
                DrawLine(new Vector2(gridSpacing * i, -gridSpacing) + newOffset, new Vector2(gridSpacing * i, editor.position.height) + newOffset);
            }

            for (int j = 0; j < heightDivs; j++)
            {
                DrawLine(new Vector2(-gridSpacing, gridSpacing * j) + newOffset, new Vector2(editor.position.width, gridSpacing * j) + newOffset);
            }

            // DEBUG code for offset positioning
            /*
            Handles.color = Color.magenta;
            DrawLine(new Vector2(editor._offset.x, editor.position.height / 2), new Vector2(editor.position.width / 2, editor.position.height / 2));

            GUI.Label(new Rect((editor._offset.x + ((editor.position.width / 2) - editor._offset.x) / 2) - 50f, editor.position.height / 2 - 20f, 100f, 20f), ((editor.position.width / 2) - editor._offset.x).ToString());

            Handles.color = Color.red;
            DrawLine(new Vector2(editor.position.width / 2, 0), new Vector2(editor.position.width / 2, editor.position.height));

            Handles.color = Color.blue;
            DrawLine(new Vector2(editor._offset.x, 0), new Vector2(editor._offset.x, editor.position.height));
            */

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        /// <summary>
        ///  Draws a line.
        /// </summary>
        /// <param name="start">The start position.</param>
        /// <param name="end">The end position.</param>
        public static void DrawLine(Vector2 start, Vector2 end)
        {
            Handles.DrawLine(new Vector3(start.x, start.y, 0), new Vector3(end.x, end.y, 0));
        }

        // TODO: needed?
        /*
        public static void DrawArrow(Rect start, Rect end, bool programmatic, NodeMachineEditor editor)
        {
            Vector3 startPos = new Vector3(start.x + start.width / 2, start.y + start.height / 2, 0);
            Vector3 endPos = new Vector3(end.x + end.width / 2, end.y + end.height / 2, 0);
            Vector2 arrowPos = new Vector2((endPos.x - startPos.x) / 2 + startPos.x, (endPos.y - startPos.y) / 2 + startPos.y);
            Handles.color = Color.white;
            if (programmatic)
                Handles.DrawAAPolyLine(programmaticLineTex, 5f, startPos, endPos);
            else
                Handles.DrawAAPolyLine(5f, startPos, endPos);
            float xDis = endPos.x - startPos.x;
            float yDis = endPos.y - startPos.y;
            float arrowAngle = Mathf.Rad2Deg * Mathf.Atan2(yDis, xDis);
            GUIUtility.RotateAroundPivot(arrowAngle, arrowPos);
            GUI.DrawTexture(new Rect(arrowPos.x - 5, arrowPos.y - 5, 10, 10), arrowCapTex, ScaleMode.StretchToFill);
            GUIUtility.RotateAroundPivot(-arrowAngle, arrowPos);
        }*/

        public static void DrawArrow(Rect start, Vector2 end, NodeMachineEditor editor)
        {
            Vector3 startPos = new Vector3(start.x + start.width / 2, start.y + start.height / 2, 0);
            Vector3 endPos = new Vector3(end.x, end.y, 0);
            Handles.color = Color.white;
            Handles.DrawAAPolyLine(5f, startPos, endPos);
            float xDis = endPos.x - startPos.x;
            float yDis = endPos.y - startPos.y;
            float arrowAngle = Mathf.Rad2Deg * Mathf.Atan2(yDis, xDis);
            GUIUtility.RotateAroundPivot(arrowAngle, end);
            GUI.DrawTexture(new Rect(endPos.x - 6, endPos.y - 10 / 2, 10, 10), arrowCapTex, ScaleMode.StretchToFill);
            GUIUtility.RotateAroundPivot(-arrowAngle, end);
        }

        public static void DrawArrowCurve(Rect start, Rect end, NodeMachineEditor editor)
        {
            Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
            Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
            Vector3 startTan = startPos + Vector3.right * 50;
            Vector3 endTan = endPos + Vector3.left * 50;

            Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.white, null, 5);
            GUI.DrawTexture(new Rect(endPos.x - 6, endPos.y - 10 / 2, 10, 10), arrowCapTex, ScaleMode.StretchToFill);
        }

        public static void DrawNothingOpenScreen(NodeMachineEditor editor)
        {
            GUILayout.BeginArea(new Rect(0, 0, editor.position.width, editor.position.height));
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Open a NodeMachine Model to start", _nothingOpenStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
        }

        public static void DrawMachineNoModelScreen (NodeMachineEditor editor)
        {
            GUILayout.BeginArea(new Rect(0, 0, editor.position.width, editor.position.height));
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(editor._selectedMachine?.name + " has no assigned model", _nothingOpenStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
        }
    }

}