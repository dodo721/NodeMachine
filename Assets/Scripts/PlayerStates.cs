using UnityEngine;
using NodeMachine;
using NodeMachine.States;


/// <summary>
///  Holds state behaviours for Player
/// </summary>
[MachineProps("Player")]
public class PlayerStates : State
{

    [UseProp]
    float horizontal = 0f;
    
    [UseProp]
    float vertical = 0f;

    public float speed;

    [State]
    public void MoveForward () {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    [State]
    public void MoveBackward () {
        transform.Translate(Vector3.back * speed * Time.deltaTime);
    }

    [State]
    public void MoveLeft () {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }

    [State]
    public void MoveRight () {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    // Runs every frame as normal
    void Update () {

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

    }

}