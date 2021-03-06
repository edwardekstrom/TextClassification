﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNBClassifier
{
    class BayesEntry
    {
        private Dictionary<string, int> vocabOccur;
        private string label;
        private int numTermsInDoc;

        public BayesEntry(Dictionary<string, int> vocabOccur, string lbl)
        {
            this.vocabOccur = vocabOccur;
            label = lbl;
            numTermsInDoc = 0;
            foreach (string key in vocabOccur.Keys)
            {
                numTermsInDoc += vocabOccur[key];
            }
        }

        public void printVocabCounts()
        {
            foreach (string word in vocabOccur.Keys)
            {
                Console.WriteLine("\t" + word + "\t\t" + vocabOccur[word]);
            }
        }

        public Dictionary<string, int> VocabOccur
        {
            get { return vocabOccur; }
        }

        public int NumTermsInDoc
        {
            get { return numTermsInDoc; }
        }

        public string Label
        {
            get { return label; }
            set { label = value; }
        }
    }
}
