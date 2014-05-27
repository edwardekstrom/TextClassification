using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNBClassifier
{
    class MNBprobability
    {
        private Dictionary<string, Dictionary<string, double>> wordProbabilities;
        private Dictionary<string, double> classProbabilities;
        private string type;

        public MNBprobability(string type)
        {
            wordProbabilities = new Dictionary<string, Dictionary<string, double>>();
            classProbabilities = new Dictionary<string, double>();
            this.type = type;
        }

        // this function calculates the probability that a word W is in a class C
        public Dictionary<string, Dictionary<string, double>> computeWordProbability(
            Dictionary<string, BayesEntry> training_set,
            Dictionary<string, int> trainingVocab,
            Dictionary<string, Dictionary<string, int>> numDocsWithWinC,
            Dictionary<string, int> classCounts,
            string type)
        {
            if (type.Equals("Multinomial"))
            {
                wordProbabilities = Multinomial.estimatePdc(training_set, trainingVocab); 
            }
            else if (type.Equals("Bernoulli"))
            {
                wordProbabilities = MVBernoulli.estimatePdc(training_set, trainingVocab, numDocsWithWinC, classCounts);
            }
            else if (type.Equals("Smoothed"))
            {
                wordProbabilities = Smoothed.estimatePdc(training_set, trainingVocab);
            }
            else
            {
                return new Dictionary<string, Dictionary<string, double>>();
            }

            return wordProbabilities;
        }

        public Dictionary<string, double> computeClassProbability(
            Dictionary<string, BayesEntry> training_set,
            Dictionary<string, int> classCounts)
        {
            foreach(string c in classCounts.Keys)
            {
                classProbabilities.Add(c, ((double)classCounts[c] / (double)training_set.Count));
            }

            return classProbabilities;
        }

        public double getWordProbability(string word, string c)
        {
            if (wordProbabilities.Count == 0)
                throw new Exception("Word probabilities not computed yet");

            return wordProbabilities[word][c];
        }

        public double getClassProbability(string c)
        {
            if (classProbabilities.Count == 0)
                throw new Exception("Class probabilities not computed yet");

            return classProbabilities[c];
        }
    }
}
