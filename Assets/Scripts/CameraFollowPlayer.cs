using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    public Transform target;
    public float speed;

    void Update()
    {
        transform.position = Vector3.Slerp(transform.position, target.position, speed * Time.deltaTime);
    }
}
