using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNBClassifier
{
    // this code was copied from the Multinomial Code and should be updated as needed
    class MVBernoulli
    {
        public static Dictionary<string, Dictionary<string, double>> estimatePdc(
            Dictionary<string, BayesEntry> training_set,
            Dictionary<string, int> trainingVocab,
            Dictionary<string, Dictionary<string, int>> numDocsWithWinC,
            Dictionary<string, int> classCounts)
        {
            Dictionary<string, Dictionary<string, double>> wordProbabilities = new Dictionary<string, Dictionary<string, double>>();

            foreach (string word in trainingVocab.Keys)
            {
                foreach (string c in classCounts.Keys)
                {
                    double numerator = 1.0;
                    double denominator = 1.0;
                    
                    if (numDocsWithWinC.ContainsKey(word) && numDocsWithWinC[word].ContainsKey(c))
                    {
                        numerator += numDocsWithWinC[word][c];
                    }

                    if (classCounts.ContainsKey(c))
                    {
                        denominator += classCounts[c];
                    }

                    double pwc = numerator / denominator;

                    if (!wordProbabilities.ContainsKey(word))
                    {
                        Dictionary<string, double> classProb = new Dictionary<string, double>();
                        classProb.Add(c, pwc);
                        wordProbabilities.Add(word, classProb);
                    }
                    else
                    {
                        if (!wordProbabilities[word].ContainsKey(c))
                        {
                            wordProbabilities[word].Add(c, pwc);
                        }
                        else
                        {
                            throw new Exception("P(W|C) probabilites should only be added once");
                        }
                    }
                }
            }

            return wordProbabilities;
        }



        public static string label(
            BayesEntry testSetDoc,
            Dictionary<string, int> classCounts,
            MNBprobability probs,
            Dictionary<string, int> trainingVocab)
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
                    double wp = Math.Log10(probs.getWordProbability(word, c));

                    if (wp == 0.0)
                        throw new Exception("wp should never be zero");
                    
                    arg2 += wp;
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
