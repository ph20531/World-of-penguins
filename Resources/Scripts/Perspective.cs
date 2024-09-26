using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Perspective : MonoBehaviour
{
    public float split = 100f;
    void Update()
    {
        foreach (Transform child in transform) {
            Vector3 perspective = child.position;
            perspective.z = transform.position.z + (perspective.y / split);
            child.position = perspective;
        }
    }
}
