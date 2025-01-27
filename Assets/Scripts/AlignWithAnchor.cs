using System;
using System.Collections;
using UnityEngine;

public class AlignWithAnchor : MonoBehaviour
{
    private void Update()
    {
        if (FlagPole.Instance != null)
        {
            transform.position = FlagPole.Instance.transform.position;
            transform.rotation = FlagPole.Instance.transform.rotation;
        }
    }
}
