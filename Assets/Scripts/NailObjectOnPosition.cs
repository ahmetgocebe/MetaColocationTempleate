using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NailObjectOnPosition : MonoBehaviour
{
    [SerializeField] private string objectName;
    private GameObject targetObj;
    [Space]
    [Header("Allow align with")]
    [SerializeField] private bool x;
    [SerializeField] private bool y;
    [SerializeField] private bool z;
    // Update is called once per frame
    Vector3 pos;
    void Update()
    {
        if (targetObj == null)
        {
            targetObj = GameObject.Find(objectName);
            return;
            
        }

        pos.x = x ? transform.position.x : targetObj.transform.position.x;
        pos.y = y ? transform.position.y : targetObj.transform.position.y;
        pos.z = z ? transform.position.z : targetObj.transform.position.z;

        targetObj.transform.position = pos;
    }
}
