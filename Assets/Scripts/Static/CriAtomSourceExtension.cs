using CriWare;
using System;

public static class CriAtomSourceExtension
{
    public static float GetLength(this CriAtomSource self, string sheetName = null, string cueName = null)
    {
        var exAcb = CriAtom.GetCueSheet(sheetName).acb;
        if (!exAcb.GetCueInfo(cueName, out CriAtomEx.CueInfo cueInfo)) throw new Exception();
        return cueInfo.length / 1000f;
    }
    public static float GetLength(this CriAtomSource self)
    {
        var exAcb = CriAtom.GetCueSheet(self.cueSheet).acb;
        if (!exAcb.GetCueInfo(self.cueName, out CriAtomEx.CueInfo cueInfo)) throw new Exception();
        return cueInfo.length / 1000f;
    }
}
