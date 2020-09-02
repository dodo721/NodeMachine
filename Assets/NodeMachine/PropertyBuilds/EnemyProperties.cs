using NodeMachine;

/// <summary>
///  Property holding object for Enemy NodeMachine model
/// </summary>

// Target this property to the Enemy model
// WARNING: DO NOT CHANGE unless you know what you're doing!!
[NodeMachineProperty("Enemy")]
public class EnemyProperties : NodeMachineProperties {

    // Float properties


    // Int properties


    // Bool properties
	public bool FoundPlayer = false;


    /// <summary>
    ///  Returns a property by name
    /// </summary>
    public override object GetProp (string name) {
        switch (name) {
			case "FoundPlayer": return FoundPlayer;

        }
        return null;
    }

    /// <summary>
    ///  Sets a property by name
    /// </summary>
    public override bool SetProp (string name, object val) {
        switch (name) {
			case "FoundPlayer": FoundPlayer = (bool)val; break;

        }
        return false;
    }

}