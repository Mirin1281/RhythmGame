using UnityEngine;

namespace NoteCreating
{
    public class RegularNotePool : PoolBase<RegularNote>
    {
        [Space(20)]
        [SerializeField] Sprite normalSprite;
        [SerializeField] Sprite normalMultitapSprite;
        [Space(10)]
        [SerializeField] Sprite slideSprite;
        [SerializeField] Sprite slideMultitapSprite;
        [Space(10)]
        [SerializeField] Sprite flickSprite;
        [SerializeField] Sprite flickMultitapSprite;

        public RegularNote GetNote(RegularNoteType type, bool isMultitap = false)
        {
            int index = type switch
            {
                RegularNoteType.Normal => 0,
                RegularNoteType.Slide => 1,
                RegularNoteType.Flick => 2,
                _ => throw new System.Exception()
            };

            var n = GetInstance(index);
            n.SetRot(0);
            n.SetWidth(1f);
            n.transform.localScale = Vector3.one;
            n.SetRendererEnabled(true);
            n.SetSprite(isMultitap ? GetMultitapSprite(type) : GetDefaultSprite(type));
            n.SetAlpha(1f);
            n.transform.SetParent(this.transform);
            n.IsVerticalRange = false;
            return n;
        }

        Sprite GetDefaultSprite(RegularNoteType type) => type switch
        {
            RegularNoteType.Normal => normalSprite,
            RegularNoteType.Slide => slideSprite,
            RegularNoteType.Flick => flickSprite,
            RegularNoteType.Hold => null,
            _ => throw new System.Exception()
        };

        public Sprite GetMultitapSprite(RegularNoteType type) => type switch
        {
            RegularNoteType.Normal => normalMultitapSprite,
            RegularNoteType.Slide => slideMultitapSprite,
            RegularNoteType.Flick => flickMultitapSprite,
            RegularNoteType.Hold => null,
            _ => throw new System.Exception()
        };
    }
}