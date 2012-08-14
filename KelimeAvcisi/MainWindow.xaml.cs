using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Globalization;

namespace com.bsrykt.KelimeAvcisi
{
    public partial class MainWindow : Window
    {
        private string letters;
        private char[] letterArray;

        private Solver s;
        private static readonly int minWordLength = 3;
        private static readonly int maxWordLength = 10;
        private static readonly int matrixSize = 4;

        private static readonly string[] backs = new string[]{"back1", "back2", "back3"};
        private static readonly string dictionaryPath = "sozluk.txt";

        public MainWindow()
        {
            InitializeComponent();
            this.Background = randomBackground();


            textLetters.Focus();

            s = new Solver(matrixSize, Locale.TURKISH);
            int wordCount = s.LoadDictionary(dictionaryPath, minWordLength, maxWordLength);

            this.Title = "Kelime Avcısı @bsrykt | " + wordCount + " adet kelime yüklendi..";
        }

        private ImageBrush randomBackground()
        {
            int random = (new Random()).Next(0,3);

            return new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/KelimeAvcisi;component/res/" + backs[random] + ".jpg")));
        }


        private void textLetters_TextChanged(object sender, TextChangedEventArgs e)
        {
            letters = textLetters.Text;
            letterArray = letters.ToCharArray();

            for (int i = 0; i < matrixSize*matrixSize; i++)
            {
                TextBox txt = UIHelper.FindChild<TextBox>(this, "txt" + i);
                if (null != txt)
                {
                    txt.Text = letterArray.Length>i?letterArray[i].ToString():"";
                }
            }

            for (int i = minWordLength; i <= maxWordLength; i++)
            {
                TextBox tb = UIHelper.FindChild<TextBox>(this, "txtWords" + i);

                if (null != tb)
                {
                    tb.Text = "";
                }
            }
            txtWords42.Text = "";
            txtWords32.Text = "";

            if (letters.Length == 16)
            {
                List<KeyValuePair<string,int[]>> wordsFound =  s.Find(letters);

                wordsFound.Sort(compareStringsByLength);

                int[] txtWordsBreak = new int[] { 1,1,1,1,1,1,1,1,1,1,1,1};
                int word4Count = 0;
                int word3Count = 0;
                TextBox tb;

                foreach (KeyValuePair<string, int[]> word in wordsFound)
                {

                    if (word.Key.Length == 3)
                    {
                        word3Count++;
                    }
                    if (word.Key.Length == 4)
                    {
                        word4Count++;
                    }

                    if (word.Key.Length == 3 && word3Count > 21)
                    {
                        tb = UIHelper.FindChild<TextBox>(this, "txtWords" + word.Key.Length + "2");
                    }
                    else if (word.Key.Length == 4 && word4Count > 21)
                    {
                        tb = UIHelper.FindChild<TextBox>(this, "txtWords" + word.Key.Length + "2");
                    }
                    else
                    {
                        tb = UIHelper.FindChild<TextBox>(this, "txtWords" + word.Key.Length);
                    }
                    if (null == tb)
                    {
                        continue;
                    }
                    tb.Text += word.Key + System.Environment.NewLine;

                    int mod = word.Key.Length >= 5 ? 2 : 3;

                    if (txtWordsBreak[word.Key.Length] % mod == 0)
                    {
                        tb.Text += System.Environment.NewLine;
                    }
                    txtWordsBreak[word.Key.Length]++;
                }
            }
        }

        private int compareStringsByLength(KeyValuePair<string, int[]> a, KeyValuePair<string, int[]> b)
        {
            if (a.Key.Length != b.Key.Length)
            {
                return b.Key.Length.CompareTo(a.Key.Length);
            }
            else
            {
                return a.Key.CompareTo(b.Key);
            }
        }
    }
}
