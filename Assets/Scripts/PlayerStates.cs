using UnityEngine;
using NodeMachine.States;

/// <summary>
///  Holds various state behaviours
/// </summary>
[StateInfo(UsesMethods=true)]
public class PlayerStates : State
{

	PlayerProperties props;

	void Start () {
		props = properties as PlayerProperties;
	}

    // Represents a state on the node machine
    [StateInfo]
    public void NewState () {
        
    }

    // Runs every frame as normal
    void Update () {

        // Runs when any state method in this class is running
        if (running) {
            
        }

    }

}