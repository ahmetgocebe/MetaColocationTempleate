using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.OVR.Input;
using UnityEngine;
/// <summary>
/// 0=>Artificial world
/// 1=>Real world
/// </summary>
public class PasstroughController : MonoBehaviour
{
    
    #region Singleton

    public static PasstroughController Instance;
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    [SerializeField] OVRPassthroughLayer passthroughLayer;
    [SerializeField] OVRManager manager;

    private void Update()
    {
        //if (OVRInput.GetDown(OVRInput.Button.Three))//X            
        //{
        //    Debug.Log($"Setting pass trough{!manager.isInsightPassthroughEnabled} to {1}");
        //    SetPassTrough(!manager.isInsightPassthroughEnabled, 1f);
        //}
        //if (OVRInput.GetDown(OVRInput.Button.Four))//Y            
        //{
        //    Debug.Log($"Setting pass trough{!manager.isInsightPassthroughEnabled} to {0}");
        //    SetPassTrough(!manager.isInsightPassthroughEnabled, 0f);
        //}
        //if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        //{
        //    StartCoroutine(Fade(1, 0, 5));
        //}
        if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
        {
            if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            {
                manager.isInsightPassthroughEnabled = !manager.isInsightPassthroughEnabled;
                passthroughLayer.textureOpacity = 0.3f;
            }
        }
    }

    public void SetPassTrough(bool isEnable, float opacity = 0)
    {
        Debug.Log("passtrough is " + isEnable);
        //passthroughLayer.textureOpacity = opacity;//think like alpha value

        //
        manager.isInsightPassthroughEnabled = isEnable;

        if (isEnable)
        {
            StartCoroutine(Fade(0, 1, 5));
        }
        else
        {
            passthroughLayer.textureOpacity = 0;
        }
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        Debug.Log($"Fade started from {startAlpha} to {endAlpha} ");
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Calculate the new opacity using Lerp and assign it directly
            passthroughLayer.textureOpacity = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }

        // Ensure the final alpha value is set
        passthroughLayer.textureOpacity = endAlpha;
    }

}
