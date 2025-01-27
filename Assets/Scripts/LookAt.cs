using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    [SerializeField]
    private Transform _traget;
    void Update()
    {
     transform.LookAt(_traget);    
    }
}
