using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif
using UnityEditor;
using System.IO;
using SimpleJSON;
using NodeMachine;
using NodeMachine.Util;
using NodeMachine.Nodes;

public class NodeMachineModel : ScriptableObject {

    private Dictionary<int, Node> nodes = new Dictionary<int, Node>();
    private Dictionary<int, Link> links = new Dictionary<int, Link>();
    public Type _propertyType;
    private float _checkinTime = 1f;
    private Dictionary<Type, List<Node>> cachedNodeTypes = new Dictionary<Type, List<Node>>();

    [SerializeField]
    private List<Node> nodesList = new List<Node>();

    public static readonly string STATE_MACHINE_FILE_EXT = "machine";

    public float CheckinTime {
        get { return _checkinTime; }
        set { if (value >= 0) _checkinTime = value; }
    }

    public bool supportParallel = true;

    public delegate void PropValueChangeEvent (string name, dynamic value);
    public event PropValueChangeEvent OnPropValueChange;
    public delegate void CheckinEvent ();
    public event CheckinEvent OnCheckin ;
    public delegate void SaveEvent();
    public event SaveEvent OnSave;
    public delegate void ReloadPropsEvent();
    public event ReloadPropsEvent OnPropsReload;
    public delegate void NodeErrorEvent();
    public event NodeErrorEvent OnNodeError;
    public delegate object GetMachineProperty ();
    public delegate void SetMachineProperty (object value);

    public struct MachinePropertyFieldDelegates {
        public GetMachineProperty getter;
        public SetMachineProperty setter;
        public Type fieldType;
    }

    public Dictionary<UnityEngine.Object, Dictionary<string, MachinePropertyFieldDelegates>> machinePropertiesDelegates = new Dictionary<UnityEngine.Object, Dictionary<string, MachinePropertyFieldDelegates>>();
    public Dictionary<string, Type> machinePropsSchema = new Dictionary<string, Type>();

    [NonSerialized]
    public List<NodeError> nodeErrors = new List<NodeError>();

    private bool hasLoadedProps = false;

    void OnEnable () {
        hasLoadedProps = false;
        LoadProperties();
        ReloadModel();

        if (OnPropsReload != null)
            OnPropsReload.Invoke();
        
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += CheckErrorsOnPlay;
#endif
    }

#if UNITY_EDITOR
    void CheckErrorsOnPlay (PlayModeStateChange change) {
        if (change == PlayModeStateChange.ExitingEditMode && nodeErrors.Count != 0) {
            string errorStr = "";
            foreach (NodeError error in nodeErrors) {
                errorStr += error.errorFull + "\n";
            }
            EditorUtility.DisplayDialog("NodeMachine errors", "The NodeMachine model " + name + " contains errors. The game will play on \"OK\".\n" + errorStr, "OK");
        }
    }

    static NodeMachineModel[] GetAllInstances()
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(NodeMachineModel).Name);
        NodeMachineModel[] a = new NodeMachineModel[guids.Length];

        for (int i = 0; i < guids.Length; i++) {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            a[i] = AssetDatabase.LoadAssetAtPath<NodeMachineModel>(path);
        }

        return a;
    }
#endif

    public void ReloadProperties () {
        if (!hasLoadedProps) {
            LoadProperties();
            hasLoadedProps = true;
        }
    }

    public void ForceReloadProperties () {
        LoadProperties();
        hasLoadedProps = true;
    }

#if UNITY_EDITOR
    [DidReloadScripts]
    static void ReloadAllProperties () {
        foreach (NodeMachineModel model in GetAllInstances()) {
            model.LoadProperties();
        }
    }
#endif

    void LoadProperties () {
        
        machinePropertiesDelegates.Clear();
        machinePropsSchema.Clear();

        Assembly assembly = Assembly.Load("Assembly-CSharp");
        IEnumerable<Type> propertyTypes = assembly.GetTypes().Where(t => t.GetCustomAttribute<MachinePropsAttribute>() != null);
        foreach (Type type in propertyTypes) {
            MachinePropsAttribute propAttribute = type.GetCustomAttribute<MachinePropsAttribute>();
            if (propAttribute != null) {
                if (!typeof(UnityEngine.Object).IsAssignableFrom(type)) {
                    Debug.LogWarning("Ignoring MachineProps type " + type.ToString() + " as it is not derived from UnityEngine.Object\nThe MachineProps attribute will only be effective on classes deriving from UnityEngine.Object (MonoBehaviour, ScriptableObject, etc)");
                    continue;
                }
                if (propAttribute.Model == name) {
                    _propertyType = type;
                    break;
                }
            }
        }

        if (_propertyType != null) {
        
            Type[] getMachinePropertiesArgs = {typeof(object)};
            Type[] setMachinePropertiesArgs = {typeof(object), typeof(object)};

            FieldInfo[] fields = _propertyType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.GetCustomAttribute<UsePropAttribute>() != null).ToArray();
            
            foreach (FieldInfo field in fields) {
                // SCHEMA
                if (!machinePropsSchema.ContainsKey(field.Name))
                    machinePropsSchema.Add(field.Name, field.FieldType);

                // GETTER
                DynamicMethod getMachineProperty = new DynamicMethod(
                    "GetMachineProp_" + field.Name,       // Name
                    typeof(object),                       // Return type
                    getMachinePropertiesArgs,             // Argument types
                    _propertyType);                       // Owner type
                ILGenerator ilMH_g = getMachineProperty.GetILGenerator();

                ilMH_g.Emit(OpCodes.Ldarg_0);                       // Load "this" onto the stack
                ilMH_g.Emit(OpCodes.Ldfld, field);                  // Load field address from "this" onto stack
                ilMH_g.Emit(OpCodes.Box, field.FieldType);           // Convert address to reference
                ilMH_g.Emit(OpCodes.Ret);                           // Return and finish execution

                // SETTER
                DynamicMethod setMachineProperty = new DynamicMethod(
                    "SetMachineProp_" + field.Name,       // Name
                    typeof(void),                         // Return type
                    setMachinePropertiesArgs,             // Argument types
                    _propertyType);                       // Owner type
                ILGenerator ilMH_s = setMachineProperty.GetILGenerator();

                ilMH_s.Emit(OpCodes.Ldarg_0);                       // Load "this" onto the stack - for Stfld
                ilMH_s.Emit(OpCodes.Ldarg_1);                       // Load argument address onto stack (0 = this, 1 = first argument)
                ilMH_s.Emit(OpCodes.Unbox_Any, field.FieldType);    // Unbox to appropriate value
                ilMH_s.Emit(OpCodes.Stfld, field);                  // Store in field in "this"
                ilMH_s.Emit(OpCodes.Ret);                           // Return and finish execution
                
                UnityEngine.Object[] propsInstances = FindObjectsOfType(_propertyType);
                foreach (UnityEngine.Object obj in propsInstances) {
                    if (!machinePropertiesDelegates.ContainsKey(obj))
                        machinePropertiesDelegates.Add(obj, new Dictionary<string, MachinePropertyFieldDelegates>());
                    MachinePropertyFieldDelegates delegates;
                    delegates.getter = (GetMachineProperty) getMachineProperty.CreateDelegate(typeof(GetMachineProperty), obj);
                    delegates.setter = (SetMachineProperty) setMachineProperty.CreateDelegate(typeof(SetMachineProperty), obj);
                    delegates.fieldType = field.FieldType;
                    machinePropertiesDelegates[obj].Add(field.Name, delegates);
                }
            }

        }
        ReloadNodes();

    }

    public void ReloadNodes () {
        nodeErrors.Clear();
        foreach (Node node in nodes.Values) {
            node.model = this;
            if (_propertyType != null)
                node.OnLoad();
        }
    }

    public void TriggerPropValueChangeEvent (string name, dynamic value) {
        if (OnPropValueChange != null)
            OnPropValueChange.Invoke(name, value);
    }

    public void TriggerCheckinEvent () {
        if (OnCheckin != null)
            OnCheckin.Invoke();
    }

#if UNITY_EDITOR
    public void SaveModel () {
        string filepath = AssetDatabase.GetAssetPath(GetInstanceID());
        if (File.Exists(filepath))
            SaveToPath(filepath);
        if (OnSave != null)
            OnSave.Invoke();
    }

    public static NodeMachineModel SaveNewModel (string filepath) {
        NodeMachineModel model = ScriptableObject.CreateInstance<NodeMachineModel>();
        EntryNode entryNode = new EntryNode(model);
        model.AddNode(entryNode);
        model.SaveToPath(filepath);
        DestroyImmediate(model);
        model = AssetDatabase.LoadAssetAtPath<NodeMachineModel>(filepath);
        Selection.activeObject = model;
        return model;
    }
#endif

    public void ReloadModel () {
        string filepath = AssetDatabase.GetAssetPath(GetInstanceID());
        if (File.Exists(filepath))
            LoadFromPath(filepath);
    }

    public Node[] GetNodes () {
        return nodesList.ToArray();
    }

    public Link[] GetLinks () {
        return links.Values.ToArray();
    }

    public Node[] GetNodes<T> () where T : Node {
        Type nodeType = typeof(T);
        if (!typeof(Node).IsAssignableFrom(nodeType)) {
            throw new Exception("Can only get nodes by type with a type inheriting from Node!");
        }
        if (cachedNodeTypes.ContainsKey(nodeType))
            return cachedNodeTypes[nodeType].ToArray();
        else
            return new Node[0];
    }

    public int GetFreeLinkID () {
        int i = 0; 
        while (GetLinkFromID(i) != null)
            if (++i >= links.Count)
                return i;
        return i;
    }

    public Link GetLinkFromID (int id) {
        if (!links.ContainsKey(id))
            return null;
        return links[id];
    }

    public int GetFreeNodeID () {
        int i = 0; 
        while (GetNodeFromID(i) != null)
            if (++i >= nodes.Count)
                return i;
        return i;
    }

    public Node GetNodeFromID (int id) {
        if (!nodes.ContainsKey(id))
            return null;
        return nodes[id];
    }

    public bool LinkExists (Node from, Node to) {
        foreach (int linkID in from.linkIDs) {
            Link link = GetLinkFromID(linkID);
            if (link._to == to.ID)
                return true;
        }
        return false;
    }

    public List<Link> GetAssociatedLinks (Node node) {
        List<Link> nodelinks = new List<Link>();
        foreach (int id in node.linkIDs) {
            nodelinks.Add(GetLinkFromID(id));
        }
        return nodelinks;
    }

    public List<Link> GetOutputLinks (Node node) {
        List<Link> nodelinks = new List<Link>();
        foreach (int id in node.linkIDs) {
            Link link = GetLinkFromID(id);
            if (link._from == node.ID)
                nodelinks.Add(link);
        }
        return nodelinks;
    }

    public void LoadFromPath (string filepath) {
        string jsonStr = File.ReadAllText(filepath);
        JSONObject json = JSON.Parse(jsonStr).AsObject;
        string propsJSON = json["props"].ToString();
        string linksJSON = json["links"].ToString();
        nodes = new Dictionary<int, Node>();
        ClearCache();
        JSONArray nodesListArr = json["nodes"].AsArray;
        foreach (JSONNode nodeListJSON in nodesListArr) {
            string typeName = nodeListJSON["Type"];
            Type type = Type.GetType(typeName);
            bool typeValid = type != null;
            if (!typeof(Node).IsAssignableFrom(type))
                typeValid = false;
            if (!typeValid) {
                Debug.LogError("Encountered a malformed Node type within the model (" + typeName + ").\nCheck all Node types referenced exist and extend Node!");
                continue;
            }
            Type jsonWrapperType = typeof(JsonGenericWrapper<>).MakeGenericType(type);
            dynamic jsonWrapper = System.Activator.CreateInstance(jsonWrapperType);
            foreach (JSONNode nodeJSON in nodeListJSON["Items"]) {
                Node node = jsonWrapper.FromJson(nodeJSON.ToString());
                AddNode(node);
            }
        }
        links = IDArrayToDictionary(JsonHelper.FromJson<Link>(linksJSON));
        _checkinTime = json["checkinTime"].AsFloat;
        supportParallel = json["supportParallel"].AsBool;
        ReloadNodes();
    }

    private void SaveToPath (string filepath) {
        string linksJSON = JsonHelper.ToJson<Link>(links.Values.ToArray(), true);
        string nodesJSON = "[";
        Type[] types = cachedNodeTypes.Keys.ToArray();
        for (int i = 0; i < types.Length; i++) {
            string typedNodeJSON = JsonHelper.ToJson(types[i], cachedNodeTypes[types[i]].ToArray());
            nodesJSON += typedNodeJSON + (i == types.Length - 1 ? "" : ",");
        }
        nodesJSON += "]";
        string allDataJSON = "{\n\"links\":" + linksJSON + ",\n\"nodes\":" + nodesJSON + ",\n\"checkinTime\":" + _checkinTime + ",\n\"supportParallel\":" + supportParallel + "}";
        File.WriteAllText(filepath, allDataJSON);
        AssetDatabase.Refresh();
    }

    private Dictionary<int, T> IDListToDictionary <T> (List<T> list) where T : IDObject {
        Dictionary<int, T> dict = new Dictionary<int, T>();
        foreach (T obj in list) {
            dict.Add(obj.ID, obj);
        }
        return dict;
    }

    private Dictionary<int, T> IDArrayToDictionary <T> (T[] list) where T : IDObject {
        Dictionary<int, T> dict = new Dictionary<int, T>();
        foreach (T obj in list) {
            dict.Add(obj.ID, obj);
        }
        return dict;
    }

    public bool AddNode (Node node) {
        /*if (node is ConditionNode) {
            Debug.Log((node as ConditionNode).condition.GetComparisonValue());
        }*/
        if (nodes.ContainsKey(node.ID))
            return false;
        if (node.model == null)
            node.model = this;
        nodes.Add(node.ID, node);
        CacheNode(node);
        return true;
    }

    public bool AddLink (Link link) {
        if (links.ContainsKey(link.ID))
            return false;
        links.Add(link.ID, link);
        return true;
    }

    public bool RemoveNode (Node node) {
        if (!nodes.ContainsKey(node.ID))
            return false;
        List<Link> linksToRemove = new List<Link>();
        foreach (int linkID in node.linkIDs) {
            linksToRemove.Add(links[linkID]);
        }
        foreach (Link link in linksToRemove) {
            RemoveLink(link);
        }
        nodes.Remove(node.ID);
        return RemoveNodeFromCache(node);
    }

    public bool RemoveLink (Link link) {
        if (!links.ContainsKey(link.ID))
            return false;
        nodes[link._from].linkIDs.Remove(link.ID);
        nodes[link._to].linkIDs.Remove(link.ID);
        links.Remove(link.ID);
        return true;
    }

    private void CacheNode (Node node) {
        nodesList.Add(node);
        Type nodeType = node.GetType();
        if (!nodeType.IsSubclassOf(typeof(Node))) {
            throw new Exception("Cannot cache a Type not derived from Node!");
        }
        if (!cachedNodeTypes.ContainsKey(nodeType)) {
            cachedNodeTypes.Add(nodeType, new List<Node>());
        }
        cachedNodeTypes[nodeType].Add(node);
    }

    private bool RemoveNodeFromCache (Node node) {
        nodesList.Remove(node);
        if (cachedNodeTypes.ContainsKey(node.GetType())) {
            if (cachedNodeTypes[node.GetType()].Remove(node)) {
                if (cachedNodeTypes[node.GetType()].Count == 0) {
                    cachedNodeTypes.Remove(node.GetType());
                }
                return true;
            } else {
                return false;
            }
        } else {
            return false;
        }
    }

    private void ClearCache () {
        cachedNodeTypes.Clear();
        nodesList.Clear();
    }

    public void PushError (string error, string errorFull, Node source) {
        nodeErrors.Add(new NodeError(error, errorFull, source));
        if (OnNodeError != null)
            OnNodeError.Invoke();
    }

}