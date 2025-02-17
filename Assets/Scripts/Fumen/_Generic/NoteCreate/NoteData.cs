using UnityEngine;
using System;

namespace NoteCreating
{
    public interface INoteData
    {
        public Lpb Wait { get; }
        public RegularNoteType Type { get; }
        public float X { get; }
        public Lpb Length { get; }
    }

    [Serializable]
    public struct NoteData : INoteData, IEquatable<NoteData>
    {
        [SerializeField, Min(0)] Lpb wait;
        [SerializeField] RegularNoteType type;
        [SerializeField] float x;
        [SerializeField, Min(0)] Lpb length;
        [SerializeField] float option1;
        [SerializeField] float option2;

        public readonly Lpb Wait => wait;
        public readonly RegularNoteType Type => type;
        public readonly float X => x;
        public readonly Lpb Length => length;
        public readonly float Option1 => option1;
        public readonly float Option2 => option2;

        public NoteData(Lpb wait = default, RegularNoteType noteType = RegularNoteType.Normal, float x = 0, Lpb length = default, float option1 = 0, float option2 = 0)
        {
            this.wait = wait;
            this.type = noteType;
            this.x = x;
            this.length = length;
            this.option1 = option1;
            this.option2 = option2;
        }

        public readonly bool Equals(NoteData other)
        {
            return (wait, type, x, length, option1, option2) == (other.wait, other.type, other.x, other.length, other.option1, other.option2);
        }

        public readonly override bool Equals(object other)
        {
            if (other is NoteData noteData)
            {
                return Equals(noteData);
            }
            else return false;
        }

        public override readonly int GetHashCode() => (wait, type, x, length, option1, option2).GetHashCode();
    }
}
