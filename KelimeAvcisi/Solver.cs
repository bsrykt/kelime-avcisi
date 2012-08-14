using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace com.bsrykt.KelimeAvcisi
{
    public sealed class Locale
    {
        public static readonly Locale TURKISH = new Locale("tr-TR");
        public static readonly Locale ENGLISH = new Locale("en-US");

        private readonly string value;

        private Locale (string value)
	    {
            this.value = value;
	    }

        public override string ToString()
        {
            return value;
        }

        public static implicit operator string(Locale locale)
        {
            return locale.ToString();
        }
    }

    class Solver
    {
        private Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
        private Dictionary<string, int[]> wordsFound = new Dictionary<string, int[]>();
        private int[][] neighbors;
        private string letters;
        private int matrixSize = 4;

        private Locale locale;
        public Locale Locale
        {
            get { return locale; }
            set { locale = value; }
        }

        public Solver(int matrixSize, Locale locale)
        {
            this.matrixSize = matrixSize;
            Locale = locale;

            generateNeighborsTable();
        }

        public List<KeyValuePair<string, int[]>> Find(string letters)
        {
            this.letters = letters.ToLower(new CultureInfo(Locale, false));

            wordsFound = new Dictionary<string,int[]>();
            string currentLetter;
            List<string> wordList;

            for (int i = 0; i <= matrixSize*matrixSize-1; i++)
            {
                currentLetter = this.letters.Substring(i, 1);

                if (currentLetter.Equals("ğ"))
                {
                    continue;
                }

                if (dictionary.ContainsKey(currentLetter))
                {
                    wordList = dictionary[currentLetter.ToString()];

                    foreach (string word in wordList)
                    {
                        List<int> wordIndexes;
                        if (null != (wordIndexes = findWord(i, new List<int>(), word, 0)))
                        {
                            if (!wordsFound.ContainsKey(word))
                            {
                                wordsFound.Add(word,wordIndexes.ToArray());
                            }
                        }
                    }
                }
            }

            return wordsFound.ToList();
        }

        private List<int> findWord(int currentIndex, List<int> usedIndexes, string word, int wordIndex)
        {
            List<int> result = null;

            if (!letters.Substring(currentIndex,1).Equals(word.Substring(wordIndex, 1)))
            {
                return null;
            }

            if (null != result || wordIndex == word.Length - 1)
            {
                usedIndexes.Add(currentIndex);
                return usedIndexes;
            }

            List<int> usedNew = new List<int>();
            if (usedIndexes.Count > 0)
            {
                foreach (int item in usedIndexes)
                {
                    usedNew.Add(item);
                }
            }
            usedNew.Add(currentIndex);

            foreach (int next in neighbors[currentIndex])
            {
                if (usedNew.Contains(next))
                {
                    continue;
                }

                result = findWord(next, usedNew, word, wordIndex + 1);
                if (null != result)
                {
                    break;
                }
            }

            return result;
        }

        private int[] getNeighborIndexes(int index, int matrixSize)
        {
            int row = index / matrixSize;
            int col = index - row * matrixSize;
            int neighborSize = 1;
            List<int> neighborsFound = new List<int>((neighborSize * 2 + 1) * (neighborSize * 2 + 1) - 1);
            int[,] matrix = new int[matrixSize, matrixSize];

            for (int r = 0; r < matrixSize; r++)
            {
                for (int c = 0; c < matrixSize; c++)
                {
                    matrix[r, c] = r * matrixSize + c;
                }
            }

            for (int r = Math.Max(0, row - neighborSize); r <= Math.Min(row + neighborSize, matrixSize - 1); r++)
            {
                for (int c = Math.Max(0, col - neighborSize); c <= Math.Min(col + neighborSize, matrixSize - 1); c++)
                {
                    if (!(r == row && c == col))
                    {
                        neighborsFound.Add(matrix[r, c]);
                    }
                }
            }

            return neighborsFound.ToArray();
        }

        private void generateNeighborsTable()
        {
            neighbors = new int[matrixSize * matrixSize][];

            for (int i = 0; i < matrixSize * matrixSize; i++)
            {
                neighbors[i] = getNeighborIndexes(i, matrixSize);
            }
        }

        public int LoadDictionary(string filePath, int minWordLength, int maxWordLength)
        {
            int wordCount = 0;

            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    string[] words;
                    string word;
                    string firstLetter;

                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();

                        // Ignore empty lines and lines starting with # character
                        if (line.StartsWith("#") || line.Equals(""))
                        {
                            continue;
                        }

                        words = line.Split(' ');
                        word = words[0].Trim().ToLower(new CultureInfo(locale.ToString(), false));
                        firstLetter = word.Substring(0, 1);

                        if (!dictionary.ContainsKey(firstLetter))
                        {
                            dictionary.Add(firstLetter, new List<string>());
                        }

                        if (words[0].Length >= minWordLength && words[0].Length <= maxWordLength)
                        {
                            dictionary[firstLetter].Add(words[0]);
                            wordCount++;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Dictionary could not be read");
                Console.WriteLine(e.Message);
            }

            return wordCount;
        }
    }
}
