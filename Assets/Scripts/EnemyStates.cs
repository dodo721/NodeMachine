using UnityEngine;
using NodeMachine.States;
using NodeMachine;

/// <summary>
///  Holds various state behaviours
/// </summary>
[MachineProps("Enemy")]
public class EnemyStates : State
{

    public float speed = 1;
    public float rotSpeed = 1;
    public float searchLength = 10;
    
    [UseProp]
    private string enemyName;

    [UseProp]
    public bool FoundPlayer = false;

    [UseProp]
    public float Test = 0.2f;

    [UseProp]
    public GameObject myObj = null;

    [UseProp]
    private bool myprivbool = false;

    public enum CurrentState {
        SEARCHING, CHASING, ATTACKING
    }

    [UseProp]
    public CurrentState currentState;

    public PlayerStates player;

    [State]
    public void Searching () {
        currentState = CurrentState.SEARCHING;
    }


    void OnSearching () {
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
        transform.Rotate(0f, rotSpeed * Time.deltaTime, 0);
        RaycastHit hit;
        Color rayColor = Color.green;
        if (Physics.Raycast(transform.position, transform.forward, out hit, searchLength)) {
            rayColor = Color.magenta;
            if (hit.collider.CompareTag("Player")) {
                player = hit.collider.GetComponent<PlayerStates>();
                if (player != null) {
                    FoundPlayer = true;
                    rayColor = Color.red;
                }
            }
        }
        Debug.DrawRay(transform.position, transform.forward * searchLength, rayColor);
    }

    [State]
    public void Chasing () {
        currentState = CurrentState.CHASING;
    }

    void OnChasing () {
        transform.LookAt(player.transform, Vector3.up);
        transform.Translate(Vector3.forward * Time.deltaTime * speed, Space.Self);
    }

    [State]
    public void Attacking () {
        currentState = CurrentState.ATTACKING;
    }

    void OnAttacking () {

    }

    // Runs every frame as normal
    void Update () {

        // Runs when any state method in this class is running
        if (running) {
            switch (currentState) {
                case CurrentState.SEARCHING:
                    OnSearching();
                    break;
                case CurrentState.CHASING:
                    OnChasing();
                    break;
                case CurrentState.ATTACKING:
                    OnAttacking();
                    break;
            }
        }

    }

}