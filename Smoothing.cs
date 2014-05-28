using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNBClassifier
{
    class Smoothing
    {
        public static Dictionary<string, Dictionary<string, double>> estimatePdc(
            Dictionary<string, BayesEntry> training_set,
            Dictionary<string, int> trainingVocab)
        {
            Dictionary<string, Dictionary<string, double>> wordProbabilities = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<string, int> totalTermsInC = new Dictionary<string, int>();
            Dictionary<string, Dictionary<string, int>> numTimeWIsInC = new Dictionary<string, Dictionary<string, int>>();
            foreach (string doc in training_set.Keys)
            {
                // get the number of terms present in one class
                if (!totalTermsInC.ContainsKey(training_set[doc].Label))
                {
                    totalTermsInC.Add(training_set[doc].Label, training_set[doc].NumTermsInDoc);
                }
                else
                {
                    totalTermsInC[training_set[doc].Label] += training_set[doc].NumTermsInDoc;
                }

                // get the number of word occurances
                string c = training_set[doc].Label;
                if (!numTimeWIsInC.ContainsKey(c))
                {
                    numTimeWIsInC.Add(c, training_set[doc].VocabOccur);
                }
                else
                {
                    foreach (string word in training_set[doc].VocabOccur.Keys)
                    {
                        if (!numTimeWIsInC[c].ContainsKey(word))
                        {
                            numTimeWIsInC[c].Add(word, training_set[doc].VocabOccur[word]);
                        }
                        else
                        {
                            numTimeWIsInC[c][word] += training_set[doc].VocabOccur[word];
                        }
                    }
                }
            }

            foreach (string word in trainingVocab.Keys)
            {
                Dictionary<string, double> classProb = new Dictionary<string, double>();
                foreach (string c in totalTermsInC.Keys)
                {
                    if (numTimeWIsInC.ContainsKey(c) && numTimeWIsInC[c].ContainsKey(word))
                    {
                        double pwc = (double)(numTimeWIsInC[c][word] + 1) / (double)(totalTermsInC[c] + trainingVocab.Count);
                        classProb.Add(c, pwc);
                    }
                    else
                    {
                        double pwc = 1.0 / (double)(totalTermsInC[c] + trainingVocab.Count);
                        classProb.Add(c, pwc);
                    }
                }

                wordProbabilities.Add(word, classProb);
            }

            return wordProbabilities;
        }



        public static string label(BayesEntry testSetDoc, Dictionary<string, int> classCounts, MNBprobability probs)
        {
            double argmax = double.NegativeInfinity;
            string topC = "";

            foreach (string c in classCounts.Keys)
            {
                double arg = double.NegativeInfinity;
                double arg1 = Math.Log10(probs.getClassProbability(c));
                double arg2 = 0.0;
                foreach (string word in testSetDoc.VocabOccur.Keys)
                {
                    double wp = probs.getWordProbability(word, c);
                    if (wp == 0.0)
                        throw new Exception("wp should never be zero");
                    else
                        arg2 += Math.Log10(Math.Pow(wp, testSetDoc.VocabOccur[word]));
                }

                arg = arg1 + arg2;
                if (arg > argmax)
                {
                    argmax = arg;
                    topC = c;
                }
            }

            return topC;
        }
    }
}
