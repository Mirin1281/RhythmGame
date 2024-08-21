using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using NoteGenerating;
using UnityEngine;

public static class MyUtility
{
    public static GameObject GetPreviewObject()
    {
        GameObject previewObj = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(obj => obj.name == ConstContainer.DebugPreviewObjName)
            .FirstOrDefault();
        previewObj.SetActive(true);
        foreach(var child in previewObj.transform.OfType<Transform>().ToArray())
        {
            GameObject.DestroyImmediate(child.gameObject);
        }
        return previewObj;
    }

    public static UniTask WaitSeconds(float time, CancellationToken token) => UniTask.Delay(TimeSpan.FromSeconds(time), cancellationToken: token);
}
