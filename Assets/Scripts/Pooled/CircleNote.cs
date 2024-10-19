using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleNote : NoteBase_2D
{
    public void SetScale(Vector3 scale)
    {
        transform.localScale = scale;
    }
}
