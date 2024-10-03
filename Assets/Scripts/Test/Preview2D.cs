using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Preview2D : MonoBehaviour
{
    void Awake()
    {
        Destroy(this.gameObject);
        //gameObject.SetActive(false);
    }
}
