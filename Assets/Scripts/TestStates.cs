using UnityEngine;
using NodeMachine;
using NodeMachine.States;


/// <summary>
///  Holds state behaviours for TestModel
/// </summary>
[MachineProps("TestModel")]
[StateTarget("TestModel")]
public class TestStates : State
{

    [UseProp]
    float newStateProperty = 1f;

    // Represents a state on the node machine
    [State]
    public void NewState () {
        
    }

    // Runs on game start
    void Start () {

    }

    // Runs every frame
    void Update () {

    }

}