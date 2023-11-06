using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tutorialCamera : MonoBehaviour
{
    public float minX, maxX;
    public float minDist;
    [SerializeField] Transform player;

    public float speed;

    void Update()
    {
        if (transform.position.x < minX)
            transform.position = new Vector3(minX, transform.position.y, -10);
        if (transform.position.x > maxX)
            transform.position = new Vector3(maxX, transform.position.y, -10);

        if (transform.position.x >= minX && transform.position.x <= maxX)
        {
            float dist = Mathf.Abs(transform.position.x - player.position.x);
            if(dist > minDist)
            {
                if(transform.position.x < player.position.x)
                {
                    //direita
                    transform.Translate(Vector2.right * speed * Time.deltaTime * dist);
                }
                else
                {
                    //esquerda
                    transform.Translate(Vector2.left * speed * Time.deltaTime * dist);
                }
            }
        }
    }
}
