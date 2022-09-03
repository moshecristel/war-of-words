using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WarOfWords
{
    public class MapLetterSequence : IEnumerable<MapLetter>
    {
        public static MapLetterSequence operator +(MapLetterSequence a, MapLetter b)
            => a.CloneAndAppend(b);
        
        
        private MapLetterSequence CloneAndAppend(MapLetter letter)
        {
            MapLetterSequence cloned = new MapLetterSequence(this);
            cloned.Append(letter);
            return cloned;
        }

        public MapLetter StartLetter => Letters.Count == 0 ? null : Letters[0];
        public MapLetter EndLetter => Letters.Count == 0 ? null : Letters[^1];  
        
        public List<MapLetter> Letters = new();
        public int Length => Letters.Count;

        public MapLetterSequence()
        {
        }

        public MapLetterSequence(MapLetter letter)
        {
            Append(letter);
        }

        public MapLetterSequence(MapLetterSequence other)
        {
            foreach (MapLetter letter in other.Letters)
            {
                Append(letter);
            }
        }

        public bool ContainsAny(List<MapLetter> letters)
        {
            return Letters.Any(letters.Contains);
        }
        
        public bool Contains(MapLetter letter)
        {
            return Letters.Contains(letter);
        }

        public void Append(MapLetter letter)
        {
            Letters.Add(letter);
        }

        protected bool Equals(MapLetterSequence other)
        {
            return other.Letters.SequenceEqual(Letters);
        }

        public IEnumerator<MapLetter> GetEnumerator()
        {
            return ((IEnumerable<MapLetter>)Letters).GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MapLetterSequence)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 19;
                foreach (var letter in Letters)
                {
                    hash = hash * 31 + letter.GetHashCode();
                }
                return hash;
            }
        }

        public override string ToString()
        {
            return string.Join("", Letters.Select(letter => letter.Character).ToList());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
