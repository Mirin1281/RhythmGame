using UnityEngine;

namespace NoteCreating
{
    public class NormalNote : RegularNote
    {
        public override RegularNoteType Type => RegularNoteType.Normal;
    }
}