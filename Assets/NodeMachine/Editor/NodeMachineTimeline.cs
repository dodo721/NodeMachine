using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NodeMachine.Nodes;

namespace NodeMachine {
    
    public class NodeMachineTimeline : EditorWindow
    {
        
        static Texture2D windowIcon;
        static GUIContent _title;
        public Rect _timelineArea;
        private Rect _sideMenu;
        private Rect _toolbar;

        private Texture2D recordBtnOff;
        private Texture2D recordBtnOn;

        private Machine target;
        private bool _recording;

        public struct TimelineEvent {
            public Machine machine;
            public HashSet<Machine.NodePath> currentNodePaths;
            public float time;
        }
        public HashSet<TimelineEvent> events;


        [MenuItem("NodeMachine/Machine Timeline")]
        public static void ShowWindow()
        {
            NodeMachineTimeline window = (NodeMachineTimeline)EditorWindow.GetWindow(typeof(NodeMachineTimeline));
            windowIcon = EditorGUIUtility.Load("Assets/NodeMachine/Editor/Editor Resources/State Machine Icon.png") as Texture2D;
            _title = new GUIContent("Machine Timeline");
            _title.image = windowIcon;
            window.titleContent = _title;
            window.minSize = new Vector2(1024, 512);
            window.Init();
            window.Show();
        }

        void Init () {
            events = new HashSet<TimelineEvent>();
            recordBtnOff = EditorGUIUtility.Load("Assets/NodeMachine/Editor/Editor Resources/recordOff.png") as Texture2D;
            recordBtnOn = EditorGUIUtility.Load("Assets/NodeMachine/Editor/Editor Resources/recordOn.png") as Texture2D;
        }

        void OnGUI () {

            _toolbar = new Rect(10, 5, position.width - 10, 25);
            _timelineArea = new Rect(0, 30, position.width, position.height - 30);

            // ------ TOOLBAR ------

            GUILayout.BeginArea(_toolbar);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(_recording ? recordBtnOn : recordBtnOff, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false))) {
                _recording = !_recording;
                target.recordNodePaths = _recording;
            }

            if (target != null) {
                EditorGUILayout.LabelField(target.name, EditorStyles.boldLabel);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            // ------- TIMELINE ------

            GUILayout.BeginArea(_timelineArea, new GUIStyle("Box"));
            GUILayout.EndArea();

        }

        void Update () {
            // Components test true for null if they are destroyed
            // but aren't actually null, so will still throw errors
            // if you attempt access. Better to make sure they're null.
            if (target == null)
            {
                target = null;
            }
            GameObject sel = Selection.activeTransform?.gameObject;
            if (sel != target?.gameObject)
            {
                Machine machine = sel?.GetComponent<Machine>();
                LoadMachine(machine);
            }
        }

        void LoadMachine(Machine machine) {
            if (target != null) {
                target.OnCheckin -= OnCheckin;
                target.recordNodePaths = false;
            }
            if (machine != null) {
                machine.OnCheckin += OnCheckin;
                machine.recordNodePaths = _recording;
            }
            target = machine;
        }

        void OnCheckin () {
            if (Application.isPlaying && _recording) {
                foreach (Machine.NodePath path in target.checkinNodePaths) {
                    Machine.NodePath curPath = path;
                    string log = "";
                    while (curPath != null) {
                        log += curPath.currentNode.ToString() + " < ";
                        curPath = curPath.fromPath;
                    }
                    Debug.Log(log);
                }
                /*TimelineEvent tEvent;
                tEvent.currentNodePaths = new HashSet<Machine.NodePath>();
                foreach (RunnableNode node in target.CurrentRunnables) {
                    foreach (Machine.NodePath path in target.checkinNodePaths) {
                        Debug.Log("There is path " + path.currentNode.ToString());
                        if (path.currentNode == node) {
                            tEvent.currentNodePaths.Add(path);
                            break;
                        }
                    }
                }
                tEvent.machine = target;
                tEvent.time = Time.time;

                string pathStr = "";
                foreach (Machine.NodePath path in tEvent.currentNodePaths) {
                    pathStr += "\t" + path.currentNode + "\n";
                }
                Debug.Log("New timeline event!:\nTime: " + tEvent.time + "\nMachine: " + tEvent.machine.name + "\nPaths:\n" + pathStr);*/
            }
        }

    }

}