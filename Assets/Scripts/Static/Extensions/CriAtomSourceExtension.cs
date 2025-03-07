using CriWare;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class CriAtomSourceExtension
{

#if UNITY_EDITOR
    public static float GetLength(this CriAtomSource self, string sheetName = null, string cueName = null)
    {
        CriAtomExAcb exAcb;
        if (EditorApplication.isPlaying)
        {
            exAcb = CriAtom.GetCueSheet(sheetName).acb;
        }
        else
        {
            exAcb = CriWare.Editor.CriAtomEditorUtilities.LoadAcbFile(new CriFsBinder(), $"{sheetName}.acb", null);
        }
        if (exAcb == null) return -1;

        if (!exAcb.GetCueInfo(cueName, out CriAtomEx.CueInfo cueInfo)) throw new Exception();
        return cueInfo.length / 1000f;
    }
#endif

    public static float GetLength(this CriAtomSource self)
    {
        var exAcb = CriAtom.GetCueSheet(self.cueSheet).acb;
        if (!exAcb.GetCueInfo(self.cueName, out CriAtomEx.CueInfo cueInfo)) throw new Exception();
        return cueInfo.length / 1000f;
    }
}
