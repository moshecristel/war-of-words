using System.Collections.Generic;

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

        public MapLetter StartLetter => IsReversed ? Sequence.EndLetter : Sequence.StartLetter;
        public MapLetter EndLetter => IsReversed ? Sequence.StartLetter : Sequence.EndLetter;
    }
}
