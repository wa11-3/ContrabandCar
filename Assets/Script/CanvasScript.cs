using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasScript : MonoBehaviour
{
    public GameObject dpadIm;
    public GameObject buttonAIm;
    public GameObject buttonBIm;

    private void Awake()
    {
#if UNITY_ANDROID
        dpadIm.SetActive(true);
        buttonAIm.SetActive(true);
        buttonBIm.SetActive(true);
#endif
    }
}
