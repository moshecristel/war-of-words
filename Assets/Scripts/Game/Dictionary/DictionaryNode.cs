using System.Collections.Generic;

namespace WarOfWords
{
    public class DictionaryNode
    {
        public string Character { get; }
        public Dictionary<string, DictionaryNode> Children { get; }
        public bool IsWordEnd { get; set; }

        public DictionaryNode(string character)
        {
            Character = character;
            Children = new Dictionary<string, DictionaryNode>();
        }
    }
}
