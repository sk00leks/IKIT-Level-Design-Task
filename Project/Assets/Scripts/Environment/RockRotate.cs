using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockRotate : MonoBehaviour
{
    public Vector3 targetVector;
    public float speed;
    public bool isLocal;
    Transform transformObj;

    private void Start()
    {
        transformObj = this.transform;
    }
    private void FixedUpdate()
    {
        if (isLocal)
        {
            transformObj.Rotate(targetVector *speed, Space.Self);
        }
        else
        transformObj.Rotate(targetVector * speed);
    }
}
