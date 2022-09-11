using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;

namespace WarOfWords
{
    public class LibraryConsolidation : MonoBehaviour
    {
        private TextInfo _textInfo;
        private void Awake()
        {
            _textInfo = new CultureInfo("en-US",false).TextInfo;
            // StartCoroutine(ProcessWords());
        }

        

        IEnumerator ProcessWords()
        {
            HashSet<string> commonWords = new HashSet<string>(GetGoogleCommonWords());
            List<string> scrabbleWords = GetScrabbleWords();
            HashSet<string> facebookBadWords = new HashSet<string>(GetFacebookBadWords());
            Dictionary<string,int> wordToFrequencyRank = GetWordToFrequencyRank();

            List<WordListRecord> wordList = new();
            print($"{scrabbleWords.Count} scrabble words.");

            int words = 0;
            int commonWordMatches = 0;
            int wordFreqMatches = 0;
            int badWords = 0;
            foreach (string scrabbleWord in scrabbleWords)
            {
                words++;
                if (facebookBadWords.Contains(scrabbleWord))
                {
                    badWords++;
                    continue;
                }
                
                int isCommon = commonWords.Contains(scrabbleWord) ? 1 : 0;
                int frequencyRank = 0;

                if (wordToFrequencyRank.ContainsKey(scrabbleWord))
                    frequencyRank = wordToFrequencyRank[scrabbleWord];
                // wordList.Add($"{scrabbleWord},{isCommon},{frequencyRank}");
                
                wordList.Add(new WordListRecord { Word = scrabbleWord, IsCommon = isCommon, FrequencyRank = frequencyRank});

                if (isCommon == 1) commonWordMatches++;
                if (frequencyRank != 0) wordFreqMatches++;

                if (words % 1000 == 0)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
            
            wordList.Sort((a, b) =>
            {
                int aFreq = a.FrequencyRank == 0 ? 999999 : a.FrequencyRank;
                int bFreq = b.FrequencyRank == 0 ? 999999 : b.FrequencyRank;
                return aFreq - bFreq;
            });
            
            print($"Done: words={words}, commonWordMatches={commonWordMatches}, wordFreqMatches={wordFreqMatches}, badWords={badWords}");

            List<string> output = wordList.Select(record => $"{record.Word},{record.FrequencyRank}").ToList();
            
            
            ES3.Save("WordList", output, "/Users/moshecristel/Desktop/WordList.txt");
        }

        private Dictionary<string, string> GetOPTEDWordToDefinition()
        {
            Dictionary<string, string> wordToDefinition = new();
            foreach (char letter in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                print($"Loading: {letter}");
                TextAsset textAsset = Resources.Load<TextAsset>($"Text/MapLab/Source/OPTED/{letter}");
                string[] lines = textAsset.text.Split("\n\r");

                foreach (string line in lines)
                {
                    // Example: test used as a noun is common (polysemy count = 6)
                    Match match = Regex.Match(line, "[\\s]*[\"]{0,1}(.*) [(](.*)[)] (.+)[\"]{0,1}[\\s]*$");
                    if (match.Groups.Count == 4)
                    {
                        wordToDefinition[match.Groups[1].Captures[0].Value.ToLower()] =
                            match.Groups[3].Captures[0].Value;
                        // print($"word={match.Groups[1].Captures[0].Value}, part={match.Groups[2].Captures[0].Value}, def={match.Groups[3].Captures[0].Value}");
                    }
                }
                
                print("Size now " + wordToDefinition.Keys.Count);
            }
            
            print("Done!");

            return wordToDefinition;
        }
        
        private bool IsWordInWordNet(string word)
        {
            return !string.IsNullOrEmpty(GetWordNetOutput($"{word} -over"));
        }

        private bool CheckWord(string word)
        {
            Dictionary<PartOfSpeech,WordDefinition> defs = GetWordDefinitions(word);
            print("definition count: " + defs.Keys.Count);
            if (defs.Keys.Count == 0) return false;

            foreach (PartOfSpeech key in defs.Keys)
            {
                string json = JsonConvert.SerializeObject(defs[key]);
                print("json=" + json);
            }

            return true;

            // Dictionary<PartOfSpeech,Polysemy> polys = GetPolysemy(word);
            //
            // if (defs.Keys.Count == 0) return false;
            //
            // PartOfSpeech best = default;
            // int bestPolyCount = -1;
            //
            // foreach (PartOfSpeech partOfSpeech in polys.Keys)
            // {
            //     if (polys[partOfSpeech].Count > bestPolyCount)
            //     {
            //         best = partOfSpeech;
            //         bestPolyCount = polys[partOfSpeech].Count;
            //     }    
            // }
            //
            // if (best == default) return false;
            //
            // return true;
        }

        private Dictionary<string, int> GetWordToFrequencyRank()
        {
            TextAsset textAsset = Resources.Load<TextAsset>("Text/MapLab/Source/WordFrequencies");
            string[] lines = textAsset.text.Split("\n");
            List<string> words = lines.Select(line => line.Substring(0, line.IndexOf(","))).Where(word => word.Length >= 3).Select(word => word.ToLower().Trim().ToString()).ToList();
            
            print("First word: " + words[0]);

            Dictionary<string, int> wordToFrequencyRank = new();
            for (int i = 0; i < words.Count; i++)
            {
                wordToFrequencyRank[words[i]] = i + 1;
            }

            return wordToFrequencyRank;
        }
        
        private List<string> GetGoogleCommonWords()
        {
            return GetWordList("Text/MapLab/Source/GoogleCommonWords");
        }
        
        private List<string> GetScrabbleWords()
        {
            return GetWordList("Text/MapLab/Source/ScrabbleWords");
        }
        
        private List<string> GetFacebookBadWords()
        {
            return GetWordList("Text/MapLab/Source/FacebookBadWords");
        }
        
        private List<string> GetWordList(string path)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(path);
            string[] lines = textAsset.text.Split("\n");
            return lines.Select(word => word.ToLower().Trim().ToString()).Where(line => line.Length >= 3).ToList();
        }

        private List<WordDefinition> GetNonWordNetDefinitions()
        {
            TextAsset textAsset = Resources.Load<TextAsset>("Text/MapLab/Source/NonWordNetDefinitions");
            List<Dictionary<string,object>> records = CSVReader.Read(textAsset);

            List<WordDefinition> definitions = new();
            foreach (Dictionary<string,object> record in records)
            {
                string word = record["Word"].ToString();
                PartOfSpeech partOfSpeech =
                    (PartOfSpeech)Enum.Parse(typeof(PartOfSpeech), record["PartOfSpeech"].ToString(), true);
                string definition = record["Definition"].ToString();
                
                definitions.Add(new WordDefinition
                {
                    Word = word,
                    Part = partOfSpeech,
                    Synonyms = new(),
                    Definitions = new List<string>() { definition }
                });
            }

            return definitions;
        }
        
        
        private Dictionary<PartOfSpeech, Polysemy> GetPolysemy(string word)
        {
            Dictionary<PartOfSpeech, Polysemy> dict = new();
            
            foreach (PartOfSpeech part in GetWordNetPartsOfSpeech())
            {
                string x = ToArgumentFlag(part);
                string output = GetWordNetOutput($"{word} -faml{x}");
                if (!string.IsNullOrEmpty(output))
                {
                    // Example: test used as a noun is common (polysemy count = 6)
                    Match match = Regex.Match(output, "as a[\\w]{0,1} [\\w]+ is ([\\w\\s]+) [(]polysemy count = (\\d+)");
                    if (match.Groups.Count == 3)
                    {
                        string description = _textInfo.ToTitleCase(match.Groups[1].Captures[0].Value);
                        int count = int.Parse(match.Groups[2].Captures[0].Value);
                        dict[part] = new Polysemy
                        {
                            Part = part,
                            Count = count,
                            Description = description
                        };
                    }
                }
            }

            return dict;
        }

        private Dictionary<PartOfSpeech, WordDefinition> GetWordDefinitions(string word)
        {
            Dictionary<PartOfSpeech, WordDefinition> dict = new();
            
            foreach (PartOfSpeech part in GetWordNetPartsOfSpeech())
            {
                // print("part=" + part);
                string x = ToArgumentFlag(part);
                
                //wn test -synsn -g
                string output = GetWordNetOutput($"{word} -syns{x} -g");
                // print("output=" + output);
                if (!string.IsNullOrEmpty(output))
                {
                    // print("Output not empty for part=" + part);
                    // Example:
                    // Sense 1
                    // trial, trial run, test, tryout -- (trying something to find out about it; "a sample for ten days free trial"; "a trial of progesterone failed to relieve the pain")
                    //
                    // Sense 2
                    //           ...
                    
                    // Capture 1 = Sense number
                    // Capture 2 = Synonyms
                    // Capture 3 = Definition

                    List<string> synonyms = new();
                    List<string> definitions = new();
                    
                    foreach (Match match in Regex.Matches(output,
                                 "Sense ([\\d]{1})\\s+(.+) [-]{2} [(](.+?)[;)]", RegexOptions.Multiline))
                    {
                        if (match.Groups.Count == 4)
                        {
                            // int senseNumber = int.Parse(match.Groups[1].Captures[0].Value);
                            string syns = match.Groups[2].Captures[0].Value;
                            string definition = match.Groups[3].Captures[0].Value;
                            
                            synonyms.Add(syns);
                            definitions.Add(definition);
                        }
                    }

                    if (synonyms.Count > 0)
                    {
                        dict[part] = new WordDefinition
                        {
                            Word = word,
                            Part = part,
                            Synonyms = synonyms,
                            Definitions = definitions
                        };
                    }
                }
            }

            return dict;
        }

        private List<PartOfSpeech> GetWordNetPartsOfSpeech()
        {
            return new List<PartOfSpeech>()
                { PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Adjective, PartOfSpeech.Adverb };
        }


        private string GetWordNetOutput(string arguments)
        {
            // Start the child process.
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "wn";
            p.StartInfo.Arguments = arguments;
            p.Start();
            
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            return output;
        }

        private string ToArgumentFlag(PartOfSpeech pos)
        {
            return pos switch
            {
                PartOfSpeech.Noun => "n",
                PartOfSpeech.Verb => "v",
                PartOfSpeech.Adjective => "a",
                PartOfSpeech.Adverb => "r",
                _ => throw new ArgumentOutOfRangeException(nameof(pos), pos, null)
            };
        }
    }

    [Serializable]
    public class Polysemy
    {
        public PartOfSpeech Part { get; set; }
        public int Count { get; set; }
        public string Description { get; set; }

        public Polysemy()
        {
        }

        public override string ToString()
        {
            return $"{nameof(Part)}: {Part}, {nameof(Count)}: {Count}, {nameof(Description)}: {Description}";
        }
    }

    // syns{n|v|a|r} output
    [Serializable]
    public class WordDefinition
    {
        public string Word { get; set; }
        public PartOfSpeech Part { get; set; }
        public List<string> Synonyms { get; set; }
        public List<string> Definitions { get; set; }

        public WordDefinition()
        {
        }

        public override string ToString()
        {
            return $"{nameof(Word)}: {Word}, {nameof(Part)}: {Part}, {nameof(Synonyms)}: {string.Join("|", Synonyms)}, {nameof(Definitions)}: {string.Join("|", Definitions)}";
        }
    }

    [Serializable]
    public class WordListRecord
    {
        public string Word { get; set; }
        public int IsCommon { get; set; }
        public int FrequencyRank { get; set; }

        public override string ToString()
        {
            return $"{nameof(Word)}: {Word}, {nameof(IsCommon)}: {IsCommon}, {nameof(FrequencyRank)}: {FrequencyRank}";
        }
    }
}
