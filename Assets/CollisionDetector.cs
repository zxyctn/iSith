using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    public GameObject collidedObject;
    public bool collided;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnCollisionEnter(Collision other)
    {
        //Debug.Log("Collision with " + other.gameObject.name);
        collided = true;
        collidedObject = other.gameObject;
    }

    void OnCollisionExit(Collision other)
    {
        //print("No longer in contact with " + other.transform.name);
        collided = false;
        collidedObject = null;
    }
}
