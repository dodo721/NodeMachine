using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    public Transform target;
    public float speed;
    public bool smooth;

    void LateUpdate()
    {
        if (target != null) {
            if (smooth)
                transform.position = Vector3.Slerp(transform.position, target.position, speed * Time.deltaTime);
            else
                transform.position = target.position;
        }
    }
}
