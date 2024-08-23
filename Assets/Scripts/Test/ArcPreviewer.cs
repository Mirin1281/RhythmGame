using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcPreviewer : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(true);
        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
