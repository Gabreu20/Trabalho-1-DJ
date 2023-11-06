using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public AudioSource soundClosing;
    public float target;
    public bool side;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y> target && side)
            transform.Translate(0, Time.deltaTime*5, 0);
        else if(transform.position.x > target && !side)
            transform.Translate(0, Time.deltaTime * 5, 0);
        else
            soundClosing.Play();
    }
}
