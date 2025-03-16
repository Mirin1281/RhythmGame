using UnityEngine;

namespace NoteCreating
{
    public class RegularNotePool : PoolBase<RegularNote>
    {
        [Space(20)]
        [SerializeField] Sprite normalSprite;
        [SerializeField] Sprite normalMultitapSprite;

        public RegularNote GetNote(RegularNoteType type, bool isMultitap = false)
        {
            int index = type switch
            {
                RegularNoteType.Normal => 0,
                RegularNoteType.Slide => 1,
                _ => -1
            };
            if (index == -1)
            {
                Debug.LogError("Invalid");
                return null;
            }

            var n = GetInstance(index);

            n.Refresh();
            if (type == RegularNoteType.Normal)
            {
                n.SetSprite(isMultitap ? normalMultitapSprite : normalSprite);
            }
            n.transform.SetParent(this.transform);

            return n;
        }

        public void SetMultitapSprite(RegularNote note)
        {
            if (note == null || note.Type != RegularNoteType.Normal) return;
            note.SetSprite(normalMultitapSprite);
        }
    }
}