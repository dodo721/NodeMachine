using UnityEngine;
using NodeMachine.States;
using NodeMachine;

/// <summary>
///  Holds various state behaviours
/// </summary>
[MachineProps("Enemy")]
[StateTarget("Enemy")]
public class EnemyStates : State
{

    public float speed = 1;
    public float chaseMultiplier = 2;
    public float rotSpeed = 1;
    public float lookSpeed = 1;

    public GameObject vision;

    [UseProp]
    private bool FoundPlayer = false;

    [UseProp]
    private float startAvoidingWallTime = 0;

    [UseProp]
    private float timeSinceAvoidWallStart = 0;

    [UseProp]
    public float avoidWallTime = 1;

    [UseProp]
    private float distToPlayer = Mathf.Infinity;

    private string Hola;

    public PlayerStates player;

    private float startTime;

    [State]
    public void Searching () {
        vision.GetComponent<Renderer>().material.color = new Color(1f, 0.5f, 0f);
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
        transform.Rotate(0f, rotSpeed * Time.deltaTime, 0);
    }

    void OnTriggerEnter (Collider other) {
        if (other.tag == "Player") {
            RaycastHit hit;
            int layerMask = (1 << 9) | (1 << 2);
            if (Physics.Raycast(transform.position, player.transform.position - transform.position, out hit, layerMask)) {
                if (hit.collider.tag == "Player") {
                    FoundPlayer = true;
                }
            }
        }
    }

    void OnTriggerExit (Collider other) {
        if (other.tag == "Player") {
            FoundPlayer = false;
        }
    }

    void OnCollisionEnter (Collision other) {
        startAvoidingWallTime = Time.time;
    }

    [Event]
    public void StartChase () {
        startTime = Time.time;
    }

    [State]
    public void Chasing () {
        if (player != null) {
            vision.GetComponent<Renderer>().material.color = Color.red;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up), lookSpeed * Time.deltaTime);
            transform.Translate(Vector3.forward * Time.deltaTime * speed * chaseMultiplier, Space.Self);
        }
    }

    [State]
    private void AvoidWall () {
        transform.Translate(Vector3.back * Time.deltaTime * speed);
        transform.Rotate(0f, rotSpeed * Time.deltaTime, 0);
        timeSinceAvoidWallStart = Time.time - startAvoidingWallTime;
    }

    [Event]
    private void StopAvoidingWall () {
        startAvoidingWallTime = 0;
        timeSinceAvoidWallStart = 0;
    }

    [Event]
    public void KillPlayer () {
        player.dead = true;
    }

    [Event]
    public void MoveBackward () {
        timeSinceAvoidWallStart = Time.time - startAvoidingWallTime;
        transform.Translate(Vector3.back * Time.deltaTime * speed * 2);
    }
    
    void Update () {
        if (player != null) {
            distToPlayer = Vector3.Distance(player.transform.position, transform.position);
            if (FoundPlayer) {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, player.transform.position - transform.position, out hit)) {
                    if (hit.collider.tag != "Player") {
                        FoundPlayer = false;
                    }
                } else
                    FoundPlayer = false;
            }
        }
    }

    void FixedUpdate () {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

}