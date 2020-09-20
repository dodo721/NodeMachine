using UnityEngine;
using NodeMachine;
using NodeMachine.States;


/// <summary>
///  Holds state behaviours for Player
/// </summary>
[MachineProps("Player")]
[StateTarget("Player")]
public class PlayerStates : State
{

    [UseProp]
    float horizontal = 0f;
    
    [UseProp]
    float vertical = 0f;

    public float speed;

    [UseProp]
    public bool dead;

    public GameObject deadMe;

    private enum PlayerEmotion {
        HAPPY, SAD, ANGRY, CALM, CONFUSED, DETERMINED
    }

    [UseProp]
    private PlayerEmotion emotion;

    [Event]
    void MoveForward () {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    [Event]
    void MoveBackward () {
        transform.Translate(Vector3.back * speed * Time.deltaTime);
    }

    [Event]
    void MoveLeft () {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }

    [Event]
    void MoveRight () {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    [Event]
    void Kill () {
        dead = true;
        Instantiate(deadMe, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    // Runs every frame as normal
    void Update () {

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

    }

}