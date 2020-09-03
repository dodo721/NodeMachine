using UnityEngine;
using NodeMachine.States;
using NodeMachine;

/// <summary>
///  Holds various state behaviours
/// </summary>
[MachineProps("Player")]
public class PlayerStates : State
{

    // Represents a state on the node machine
    [State]
    public void NewState () {
        
    }

    // Runs every frame as normal
    void Update () {

        // Runs when any state method in this class is running
        if (running) {
            
        }

    }

}