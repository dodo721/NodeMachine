using NodeMachine;

/// <summary>
///  Property holding object for Player NodeMachine model
/// </summary>

// Target this property to the Player model
// WARNING: DO NOT CHANGE unless you know what you're doing!!
[NodeMachineProperty("Player")]
public class PlayerProperties : NodeMachineProperties {

    // Float properties


    // Int properties


    // Bool properties


    /// <summary>
    ///  Returns a property by name
    /// </summary>
    public override object GetProp (string name) {
        switch (name) {

        }
        return null;
    }

    /// <summary>
    ///  Sets a property by name
    /// </summary>
    public override bool SetProp (string name, object val) {
        switch (name) {

        }
        return false;
    }

}