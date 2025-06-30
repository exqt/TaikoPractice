using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HorizonalNoteLaneFrameUI : MonoBehaviour
{
    public Image Red, Blue;

    public float alphaRate = 4f;
    public float areaAlpha = 0.6f;

    float Redalpha, Bluealpha;

    void Start()
    {
        Redalpha = Bluealpha = 0.0f;
    }

    public void Hit(TaikoKeyType key)
    {
        switch (key)
        {
            case TaikoKeyType.LEFT_DON:
            case TaikoKeyType.RIGHT_DON:
                Redalpha = 1.0f;
                Bluealpha = 0.0f;
                break;
            case TaikoKeyType.LEFT_KA:
            case TaikoKeyType.RIGHT_KA:
                Redalpha = 0.0f;
                Bluealpha = 1.0f;
                break;
        }
    }

    void Update()
    {
        Redalpha = Mathf.Max(0.0f, Redalpha - Time.deltaTime * alphaRate);
        Bluealpha = Mathf.Max(0.0f, Bluealpha - Time.deltaTime * alphaRate);

        Red.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Pow(Redalpha * areaAlpha, 2f));
        Blue.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Pow(Bluealpha * areaAlpha, 2f));
    }
}

