using System.Collections.Generic;
using UnityEngine;

namespace WarOfWords
{
    public class DictionaryTrie
    {
        public Dictionary<string, DictionaryNode> Roots { get; set; }

        public DictionaryTrie()
        {
            Load();
        }

        public void Load()
        {
            // Initialize root dictionary and add root nodes
            Roots = new Dictionary<string, DictionaryNode>();
            string remainingAlpha = CharacterUtils.ALPHABET;
            while(remainingAlpha.Length > 0)
            {
                string letter = remainingAlpha.Substring(0, 1);
                Roots[letter] = new DictionaryNode(letter);

                remainingAlpha = remainingAlpha.Substring(1);
            }
            
            // Index words from dictionary files into trie
            TextAsset textAsset = Resources.Load<TextAsset>("Text/dictionary");
            List<string> words = new List<string>(textAsset.text.Split('\n'));

            foreach (string word in words)
            {
                Index(word.ToUpper().Trim(), null);
            }
        }

        private void Index(string sequence, DictionaryNode parentDictionaryNode)
        {
            string firstLetter = sequence.Substring(0, 1);
            string restOfWord = sequence.Substring(1);
            
            DictionaryNode firstDictionaryNode;
            if (parentDictionaryNode == null)
            {
                // First letter of sequence
                firstDictionaryNode = Roots[firstLetter];
            }
            else
            {
                // Not the first letter of the sequence
                if (parentDictionaryNode.Children.ContainsKey(firstLetter))
                {
                    firstDictionaryNode = parentDictionaryNode.Children[firstLetter];
                }
                else
                {
                    firstDictionaryNode = new DictionaryNode(firstLetter);
                    parentDictionaryNode.Children[firstLetter] = firstDictionaryNode;
                }
            }
            
            // Sequence is fully indexed
            if (restOfWord.Length == 0)
            {
                firstDictionaryNode.IsWordEnd = true;
                return;
            }
            
            Index(restOfWord, firstDictionaryNode);
        }

        public bool IsWord(string sequence)
        {
            sequence = sequence.ToUpper();
            
            string firstLetter = sequence.Substring(0, 1);
            string restOfWord = sequence.Substring(1);

            DictionaryNode root = Roots[firstLetter];
            return restOfWord.Length == 0 ? root.IsWordEnd : CheckChildrenForSequence(restOfWord, root.Children);
        }

        private bool CheckChildrenForSequence(string sequence, Dictionary<string, DictionaryNode> children)
        {
            string firstLetter = sequence.Substring(0, 1);
            string restOfWord = sequence.Substring(1);

            if (!children.ContainsKey(firstLetter)) return false;

            DictionaryNode firstDictionaryNode = children[firstLetter];
            
            return restOfWord.Length == 0
                ? firstDictionaryNode.IsWordEnd
                : CheckChildrenForSequence(restOfWord, firstDictionaryNode.Children);
        }
    }
}
