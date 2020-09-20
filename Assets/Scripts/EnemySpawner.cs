using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Vector2 enemySquare;
    public float distancing;
    public GameObject enemy;
    public PlayerStates player;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < enemySquare.x; i ++) {
            for (int j = 0; j < enemySquare.y; j ++) {
                GameObject enemyInst = Instantiate(enemy, transform.position, transform.rotation);
                enemyInst.GetComponent<EnemyStates>().player = player;
                transform.Translate(Vector3.right * distancing);
            }
            transform.Translate(Vector3.left * distancing * enemySquare.y);
            transform.Translate(Vector3.forward * distancing);
        }
    }
}
