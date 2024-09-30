using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingCanvas : MonoBehaviour
{
    [SerializeField] Canvas canvas;

    public void Open()
    {
        gameObject.SetActive(true);
        canvas.enabled = true;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        canvas.enabled = false;
    }

    public void Toggle()
    {
        bool isActive = gameObject.activeSelf;
        gameObject.SetActive(!isActive);
        canvas.enabled = !isActive;
    }
}
