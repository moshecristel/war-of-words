using UnityEngine;

namespace WarOfWords
{
    public class MapOrderedLetterSequence
    {
        public bool IsReversed { get; }
        public MapLetterSequence Sequence { get; }

        public MapOrderedLetterSequence(MapLetterSequence sequence, bool isReversed)
        {
            Sequence = sequence;
            IsReversed = isReversed;
        }
    }
}
