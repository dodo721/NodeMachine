using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorClose : MonoBehaviour
{

    public float timeToClose = 0;
    public float targetY;
    private Vector3 targetPos;
    private float startTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        targetPos = new Vector3(transform.position.x, targetY, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (startTime != 0) {
            transform.position = Vector3.Lerp(transform.position, targetPos, (Time.time - startTime) / timeToClose);
        }
    }

    void OnTriggerEnter (Collider other) {
        if (other.tag == "Player")
            startTime = Time.time;
    }
}
