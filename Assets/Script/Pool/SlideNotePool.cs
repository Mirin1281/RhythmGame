using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideNotePool : PoolBase<SlideNote>
{
    public SlideNote GetNote()
    {
        return InitNote(GetInstance());


        static SlideNote InitNote(SlideNote note)
        {
            return note;
        }
    }
}
