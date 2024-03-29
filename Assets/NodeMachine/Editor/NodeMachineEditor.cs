﻿using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using System.IO;
using UnityEditor;
using UnityEngine;
using NodeMachine.Nodes;
using NodeMachine.Util;

namespace NodeMachine {

    public class NodeMachineEditor : EditorWindow
    {

        public Rect _nodeEditor;
        private Rect _sideMenu;
        private Rect _toolbar;

        public Vector2 _offset;
        public Vector2 _uncenteredOffset = new Vector2();
        public float _zoom = 1;
        private bool _creatingLink = false;
        private bool _dragging = false;
        private bool _drawTransparentLinks;
        public bool _showInivisibleNodes = false;
        private bool _changesMade = false;
        private static GUIContent titlePlain = new GUIContent("NodeMachine");
        private static GUIContent titleUnsaved = new GUIContent("NodeMachine *");
        private static Texture2D windowIcon;

        public Node _selectedNode = null;
        public Link _selectedLink = null;
        public Machine _selectedMachine = null;
        public NodeMachineModel _model = null;
        private int _modelInstanceID = -1;
        private PropertyMenu _propertyMenu = null;
        private ErrorPanel _errorPanel = null;
        private Dictionary<Type, INodeMenuHandler> nodeMenuHandlers = new Dictionary<Type, INodeMenuHandler>();
        public bool uncompiledChanges = false;
        public bool recompiling = false;

        public static void ShowWindow(NodeMachineEditor window)
        {
            windowIcon = EditorGUIUtility.Load("Assets/NodeMachine/Editor/Editor Resources/State Machine Icon.png") as Texture2D;
            titlePlain.image = windowIcon;
            titleUnsaved.image = windowIcon;
            window.titleContent = titlePlain;
            window.Show();
        }

        void OnEnable()
        {
            windowIcon = EditorGUIUtility.Load("Assets/NodeMachine/Editor/Editor Resources/State Machine Icon.png") as Texture2D;
            titlePlain.image = windowIcon;
            titleUnsaved.image = windowIcon;
            titleContent = titlePlain;
            NodeMachineGUIUtils.Init();
            _propertyMenu = new PropertyMenu(this);
            _errorPanel = new ErrorPanel(this);
            Undo.undoRedoPerformed += OnUndoRedo;
            if (_model == null)
            {
                _model = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(_modelInstanceID), typeof(NodeMachineModel)) as NodeMachineModel;
            }
            if (_model != null)
            {
                LoadModel(_model);
                if (_selectedLink != null)
                    _selectedLink = _model.GetLinkFromID(_selectedLink.ID);
                if (_selectedNode != null)
                    _selectedNode = _model.GetNodeFromID(_selectedNode.ID);
            }
            else
            {
                _selectedLink = null;
                _selectedNode = null;
            }
            FindNodeMenuHandlers();
        }

        void OnDestroy()
        {
            if (_changesMade)
                if (EditorUtility.DisplayDialog("Save changes", "This model has been modified. Save changes?", "Yes", "No"))
                    _model.SaveModel();
        }

        public void LoadModel(NodeMachineModel model)
        {
            _modelInstanceID = model.GetInstanceID();
            model.OnCheckin -= Repaint;
            model.OnCheckin += Repaint;
            model.OnSave -= MarkSaved;
            model.OnSave += MarkSaved;
            _model = model;
            Repaint();
        }

        public void SaveModel()
        {
            if (_model != null)
            {
                _model.SaveModel();
                MarkSaved();
            }
        }

        void MarkSaved()
        {
            _changesMade = false;
            
            GUI.changed = true;
        }

        void MarkUnsaved()
        {
            _changesMade = true;
            RecordUndo("Edit State Machine Model");
            EditorUtility.SetDirty(_model);
            GUI.changed = true;
        }

        void LoadMachine(Machine newMachine)
        {
            _selectedMachine?.MakeEditorCheckinEventTarget(false);
            if (_selectedMachine != null)
                _selectedMachine.OnMachineChange -= ReloadMachineModel;
            _selectedMachine = newMachine;
            _selectedMachine?.MakeEditorCheckinEventTarget(true);
            if (newMachine != null) {
                newMachine.OnMachineChange += ReloadMachineModel;
                ReloadMachineModel();
            }
            Repaint();
        }

        void ReloadMachineModel () {
            if (_selectedMachine._model != null) {
                LoadModel(_selectedMachine.GetModel());
            } else {
                _model = null;
                _modelInstanceID = -1;
            }
            Repaint();
        }

        void OnUndoRedo()
        {
            LoadModel(_model);
            Repaint();
        }

        void RecordUndo(string name)
        {
            Undo.RegisterCompleteObjectUndo(_model, name);
            Undo.FlushUndoRecordObjects();
        }

        void FindNodeMenuHandlers()
        {
            Assembly editorAssembly = Assembly.Load("Assembly-CSharp-Editor");
            IEnumerable<Type> nodeMenuTypes = editorAssembly.GetTypes().Where(t =>
                t.GetCustomAttribute<NodeMenuAttribute>() != null
                && typeof(INodeMenuHandler).IsAssignableFrom(t)
            );
            foreach (Type nodeMenuType in nodeMenuTypes)
            {
                Type nodeType = nodeMenuType.GetCustomAttribute<NodeMenuAttribute>().NodeType;
                if (typeof(Node).IsAssignableFrom(nodeType))
                {
                    INodeMenuHandler handler = System.Activator.CreateInstance(nodeMenuType) as INodeMenuHandler;
                    if (handler != null)
                        nodeMenuHandlers.Add(nodeType, handler);
                }
            }
        }

        void OnGUI()
        {

            if (_model == null) {
                if (_selectedMachine == null)
                    NodeMachineGUIUtils.DrawNothingOpenScreen(this);
                else
                    NodeMachineGUIUtils.DrawMachineNoModelScreen(this);
                return;
            }

            Vector2 center = new Vector2(position.width / 2, position.height / 2) - (_nodeEditor.position / 2);
            _offset = _uncenteredOffset + center;

            titleContent = _changesMade ? NodeMachineEditor.titleUnsaved : NodeMachineEditor.titlePlain;

            bool livePreview = _selectedMachine != null && EditorApplication.isPlaying;

            bool modelNeedsSaving = false;
            _nodeEditor = new Rect(250, 30, position.width - 250, position.height - 30);
            _sideMenu = new Rect(10, 20, 230, position.height - 20);
            _toolbar = new Rect(260, 5, position.width - 260, 25);

            // ----- TOOLBAR --------

            GUILayout.BeginArea(_toolbar);
            GUILayout.BeginHorizontal();
            
            _drawTransparentLinks = GUILayout.Toggle(_drawTransparentLinks," Link X-Ray", GUILayout.ExpandWidth(false));
            GUILayout.Label("  ", GUILayout.ExpandWidth(false));
            _showInivisibleNodes = GUILayout.Toggle(_showInivisibleNodes," Reveal hidden nodes", GUILayout.ExpandWidth(false));
            GUILayout.Label("  ", GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            // ----- SIDE MENU ------

            GUILayout.BeginArea(_sideMenu);
            GUILayout.BeginVertical();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            EditorGUILayout.LabelField(_model.name, EditorStyles.boldLabel);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.Space();

            float checkinTimeBeforeDraw = _model.CheckinTime;
            bool supportParallelBeforeDraw = _model.supportParallel;
            _model.CheckinTime = EditorGUILayout.FloatField("Checkin time", _model.CheckinTime);
            _model.supportParallel = EditorGUILayout.Toggle("Parallel flow", _model.supportParallel);
            if (checkinTimeBeforeDraw != _model.CheckinTime || supportParallelBeforeDraw != _model.supportParallel) {
                modelNeedsSaving = true;
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.Space();

            _propertyMenu.DrawMenu(EditorApplication.isPlayingOrWillChangePlaymode, _selectedMachine);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // --------- ERRORS ---------

            EditorGUILayout.Space();

            _errorPanel.Draw();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.EndVertical();
            GUILayout.EndArea();

            // ----- NODE EDITOR -----

            GUILayout.BeginArea(_nodeEditor, new GUIStyle("Box"));

            NodeMachineGUIUtils.DrawGrid(10, 0.2f, Color.gray, this); // * zoom
            NodeMachineGUIUtils.DrawGrid(50, 0.4f, Color.gray, this); // * zoom

            foreach (Link link in _model.GetLinks())
            {
                if (livePreview)
                {
                    NodeMachineGUIUtils.DrawLink(link, _selectedMachine.CurrentLinks.Contains(link), this);
                }
                else
                {
                    NodeMachineGUIUtils.DrawLink(link, false, this);
                }
            }

            if (_creatingLink)
            {
                Rect fromTransform = new Rect(_selectedNode.drawnTransform.position - _nodeEditor.position, _selectedNode.drawnTransform.size);
                NodeMachineGUIUtils.DrawArrow(fromTransform, Event.current.mousePosition, this);
            }

            foreach (Node node in _model.GetNodes())
            {
                if (livePreview && node is RunnableNode)
                {
                    RunnableNode runnable = node as RunnableNode;
                    if (_selectedMachine.CurrentRunnables.Contains(node))
                    {
                        if (NodeMachineGUIUtils.DrawNode(node, runnable.activeBackground, this, Event.current))
                            modelNeedsSaving = true;
                    }
                    else
                    {
                        if (NodeMachineGUIUtils.DrawNode(node, this, Event.current))
                            modelNeedsSaving = true;
                    }
                }
                else
                {
                    if (NodeMachineGUIUtils.DrawNode(node, this, Event.current))
                        modelNeedsSaving = true;
                }
            }

            // Draw transparent links over everything - looks like links are showing through from behind nodes
            if (_drawTransparentLinks) {
                foreach (Link link in _model.GetLinks())
                {
                    if (livePreview)
                    {
                        NodeMachineGUIUtils.DrawTransparentLink(link, _selectedMachine.CurrentLinks.Contains(link), this);
                    }
                    else
                    {
                        NodeMachineGUIUtils.DrawTransparentLink(link, false, this);
                    }
                }
            }

            GUILayout.EndArea();

            // --------------------- PROCESS EVENTS ----------------------------

            if (_nodeEditor.Contains(Event.current.mousePosition))
            {
                Vector2 totalOffset = _offset + _nodeEditor.position;
                foreach (Node node in _model.GetNodes().Reverse())
                {
                    if (node.visible || _showInivisibleNodes) {
                        if (node.ProcessEvents(Event.current, _zoom, SelectNode, ProcessNodeContextMenu))
                        {
                            modelNeedsSaving = true;
                        }
                    }
                }
                foreach (Link link in _model.GetLinks())
                {
                    link.ProcessEvents(Event.current, totalOffset, ProcessLinkContextMenu, SelectLink);
                }
            }

            ProcessEvents(Event.current);

            if (modelNeedsSaving)
            {
                MarkUnsaved();
            }
            if (GUI.changed) Repaint();
        }

        void Update()
        {
            // Components test true for null if they are destroyed
            // but aren't actually null, so will still throw errors
            // if you attempt access. Better to make sure they're null.
            if (_selectedMachine == null)
            {
                _selectedMachine = null;
            }
            GameObject sel = Selection.activeTransform?.gameObject;
            if (sel != _selectedMachine?.gameObject)
            {
                Machine machine = sel?.GetComponent<Machine>();
                LoadMachine(machine);
            }
            if (_creatingLink)
            {
                Repaint();
            }
        }

        void ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    _creatingLink = false;
                    if (e.button == 1 && _nodeEditor.Contains(e.mousePosition))
                    {
                        ProcessContextMenu(e.mousePosition);
                    }
                    else if (e.button == 0 && _nodeEditor.Contains(e.mousePosition))
                    {
                        _selectedNode = null;
                        _selectedLink = null;
                        _dragging = true;
                        GUI.changed = true;
                    }
                    break;
                case EventType.MouseDrag:
                    if (e.button == 0 && (_nodeEditor.Contains(e.mousePosition) || _dragging))
                    {
                        OnDrag(e.delta);
                    }
                    break;
                case EventType.MouseUp:
                    _dragging = false;
                    break;
                case EventType.ScrollWheel:
                    OnScroll(e.delta);
                    break;
                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.S && e.control)
                    {
                        SaveModel();
                        GUI.changed = true;
                    }
                    break;
            }
        }

        void ProcessContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();

            LoadNodeMenu(genericMenu, mousePosition);

            genericMenu.ShowAsContext();
        }

        void ProcessNodeContextMenu(Node node)
        {
            GenericMenu genericMenu = new GenericMenu();
            if (node.CanCreateLinkFrom())
                genericMenu.AddItem(new GUIContent("Create link"), false, () => { StartCreateLink(node); });
            else
                genericMenu.AddDisabledItem(new GUIContent("Create link"), false);

            if (nodeMenuHandlers.ContainsKey(node.GetType()))
            {
                INodeMenuHandler handler = nodeMenuHandlers[node.GetType()];
                NodeMenuItem[] items = handler.NodeContextMenuItems(node, _model);
                if (items != null)
                {
                    foreach (NodeMenuItem item in items)
                    {
                        if (!item.disabled)
                            genericMenu.AddItem(new GUIContent(item.label), item.ticked, item.Function);
                        else
                            genericMenu.AddDisabledItem(new GUIContent(item.label), item.ticked);
                    }
                }
            }

            if (node.CanBeHidden())
                genericMenu.AddItem(new GUIContent("Hide node"), !node.visible, () => node.visible = !node.visible);
            else
                genericMenu.AddDisabledItem(new GUIContent("Hide node"), !node.visible);
            
            if (node.CanBeRemoved())
                genericMenu.AddItem(new GUIContent("Remove node"), false, () => { RemoveNode(node); LoadModel(_model); });
            else
                genericMenu.AddDisabledItem(new GUIContent("Remove node"), false);
            genericMenu.ShowAsContext();
        }

        void ProcessLinkContextMenu(Link link)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Remove link"), false, () => { RemoveLink(link); LoadModel(_model); });
            genericMenu.ShowAsContext();
        }

        void SelectNode(Node node)
        {
            if (_creatingLink && node != _selectedNode)
            {
                _creatingLink = false;
                CreateLink(_selectedNode, node);
            }
            _selectedNode = node;
            _selectedLink = null;
            Repaint();
        }

        void SelectLink(Link link)
        {
            _selectedLink = link;
            _selectedNode = null;
            //_popupMenuShowing = true;
            Repaint();
        }

        void AddNode(Type type, Vector2 position)
        {
            if (!typeof(Node).IsAssignableFrom(type))
            {
                throw new Exception("Cannot add a node of a type not derived from Node!");
            }
            Node newNode = System.Activator.CreateInstance(type, _model, position - _offset) as Node;
            AddNode(newNode);
        }

        public void AddNode(Node node)
        {
            _model.AddNode(node);
            MarkUnsaved();
            Repaint();
        }

        void RemoveNode(Node node)
        {
            _model.RemoveNode(node);
            MarkUnsaved();
        }

        void RemoveLink(Link link)
        {
            _model.RemoveLink(link);
            MarkUnsaved();
        }

        void StartCreateLink(Node state)
        {
            _creatingLink = true;
        }

        void CreateLink(Node from, Node to)
        {
            if (_model.LinkExists(from, to))
            {
                EditorUtility.DisplayDialog("Unable to create link", "Link already exists", "OK");
                return;
            }
            Link link = new Link(from, to, _model.GetFreeLinkID());
            string fromNodeError = from.BeforeAddLink(link);
            string toNodeError = to.BeforeAddLink(link);
            bool dontAddLink = false;
            if (fromNodeError != null) {
                dontAddLink = true;
                EditorUtility.DisplayDialog("Unable to create link", fromNodeError, "OK");
            }
            if (toNodeError != null) {
                dontAddLink = true;
                EditorUtility.DisplayDialog("Unable to create link", toNodeError, "OK");
            }
            if (dontAddLink)
                return;
            _model.AddLink(link);
            from.AddLink(link);
            to.AddLink(link);
            MarkUnsaved();
        }

        bool NodeHasOutputLink(Node node)
        {
            foreach (Link otherLink in _model.GetAssociatedLinks(node))
            {
                if (_model.GetNodeFromID(otherLink._from) == node)
                    return true;
            }
            return false;
        }

        bool NodeHasInputLink(Node node)
        {
            foreach (Link otherLink in _model.GetAssociatedLinks(node))
            {
                if (_model.GetNodeFromID(otherLink._to) == node)
                    return true;
            }
            return false;
        }

        void OnDrag(Vector2 delta)
        {
            _uncenteredOffset += delta;
            GUI.changed = true;
        }

        void OnScroll(Vector2 delta)
        {
            float prevZoom = _zoom;
            _zoom += delta.y * 0.02f;
            _zoom = Mathf.Clamp(_zoom, 1f, 2f);
            _uncenteredOffset -= _uncenteredOffset * (_zoom - prevZoom) * 0.5f;
            GUI.changed = true;
        }

        /// <summary>
        ///  Builds the menu items needed for the "Add node" menu.
        ///  Looks for <c>INodeMenuHandler</c>s cached by the editor first,
        ///  which falls back to the <c>NodeInfo</c> attribute on the <c>Node</c> itself,
        ///  which falls back to adding the node type name to the menu root.
        /// </summary>
        /// <param name="menu">The menu to build onto.</param>
        /// <param name="mousePosition">The position of the mouse.</param>
        public void LoadNodeMenu(GenericMenu menu, Vector2 mousePosition)
        {
            Assembly engineAssembly = Assembly.Load("Assembly-CSharp");
            IEnumerable<Type> nodeTypes = engineAssembly.GetTypes().Where(t => typeof(Node).IsAssignableFrom(t));
            List<NodeMenuItem> menuItemsToAdd = new List<NodeMenuItem>();

            foreach (Type type in nodeTypes)
            {
                bool usedMenuHandler = false;
                if (nodeMenuHandlers.ContainsKey(type))
                {
                    INodeMenuHandler handler = nodeMenuHandlers[type];
                    NodeMenuItem[] menuItems = handler.AddNodeMenuItems(_model, mousePosition - _offset, this);
                    if (menuItems != null) {
                        menuItemsToAdd.AddRange(menuItems);
                        usedMenuHandler = true;
                    }
                }
                if (!usedMenuHandler)
                {
                    if (type.IsAbstract) {
                        continue;
                    }
                    NodeInfoAttribute nodeAttribute = type.GetCustomAttribute<NodeInfoAttribute>();
                    if (nodeAttribute != null)
                    {
                        if (nodeAttribute.visible)
                            menuItemsToAdd.Add(new NodeMenuItem(nodeAttribute.menuLabel, () => AddNode(type, mousePosition), false, false));
                    }
                    else
                    {
                        menuItemsToAdd.Add(new NodeMenuItem(type.ToString(), () => AddNode(type, mousePosition), false, false));
                    }
                }
            }

            foreach (NodeMenuItem menuItem in menuItemsToAdd)
            {
                if (menuItem.disabled)
                {
                    menu.AddDisabledItem(new GUIContent(menuItem.label), menuItem.ticked);
                }
                else
                {
                    menu.AddItem(new GUIContent(menuItem.label), menuItem.ticked, menuItem.Function);
                }
            }
        }

    }

}