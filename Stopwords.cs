using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MNBClassifier
{
    class Stopwords
    {
        private List<string> stopwordList;

        public Stopwords()
        {
            stopwordList = new List<string>();
            string[] rawStopwords = File.ReadAllLines("stopwords.txt");
            foreach (string stop in rawStopwords)
            {
                stopwordList.Add(stop.Trim());
            }
        }

        public bool contains(string word)
        {
            return stopwordList.Contains(word);
        }
    }
}
