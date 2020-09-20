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
    public float rotSpeed = 1;
    public float lookSpeed = 1;

    public GameObject vision;

    [UseProp]
    private bool FoundPlayer = false;

    [UseProp]
    private float distToPlayer = Mathf.Infinity;

    public Transform player;

    private float startTime;

    [State]
    public void Searching () {
        vision.GetComponent<Renderer>().material.color = new Color(1f, 0.5f, 0f);
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
        transform.Rotate(0f, rotSpeed * Time.deltaTime, 0);
    }

    void OnTriggerEnter (Collider other) {
        if (other.tag == "Player") {
            FoundPlayer = true;
        }
    }

    void OnTriggerExit (Collider other) {
        if (other.tag == "Player") {
            FoundPlayer = false;
        }
    }

    [Event]
    public void StartChase () {
        startTime = Time.time;
    }

    [State]
    public void Chasing () {
        vision.GetComponent<Renderer>().material.color = Color.red;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(player.position - transform.position, Vector3.up), lookSpeed * Time.deltaTime);
        transform.Translate(Vector3.forward * Time.deltaTime * speed, Space.Self);
    }

    [State]
    public void Attacking () {
        GetComponent<Renderer>().material.color = Color.magenta;
    }

    [Event]
    public void SayHi () {
        Debug.Log("Hi pal!");
    }
    
    void Update () {
        distToPlayer = Vector3.Distance(player.position, transform.position);
    }

}