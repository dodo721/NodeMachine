using UnityEngine;

public abstract class NodeMachineProperties : MonoBehaviour{

    public abstract object GetProp (string name);
    public abstract bool SetProp (string name, object val);

}