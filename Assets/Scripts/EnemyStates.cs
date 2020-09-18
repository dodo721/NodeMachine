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

    [UseProp]
    public float searchLength = 10;

    [UseProp]
    public bool FoundPlayer = false;

    [UseProp]
    private float distToPlayer = Mathf.Infinity;

    public Transform player;

    [State]
    public void Searching () {
        GetComponent<Renderer>().material.color = Color.blue;
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
        transform.Rotate(0f, rotSpeed * Time.deltaTime, 0);
        RaycastHit hit;
        Color rayColor = Color.green;
        bool hitPlayer = false;
        if (Physics.Raycast(transform.position, transform.forward, out hit, searchLength)) {
            rayColor = Color.magenta;
            if (hit.transform.tag == "Player") {
                FoundPlayer = true;
                hitPlayer = true;
                rayColor = Color.red;
            }
        }
        if (!hitPlayer)
            FoundPlayer = false;
        Debug.DrawRay(transform.position, transform.forward * searchLength, rayColor);
    }

    [State]
    public void Chasing () {
        GetComponent<Renderer>().material.color = Color.red;
        FoundPlayer = false;
        transform.LookAt(player, Vector3.up);
        transform.Translate(Vector3.forward * Time.deltaTime * speed, Space.Self);
    }

    [State]
    public void Attacking () {
        GetComponent<Renderer>().material.color = Color.magenta;
    }

    void Update () {
        distToPlayer = Vector3.Distance(player.position, transform.position);
    }

}