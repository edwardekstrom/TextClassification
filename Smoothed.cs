using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNBClassifier
{
    // this code was copied from the Multinomial Code and should be updated as needed
    class Smoothed
    {
        public static Dictionary<string, Dictionary<string, double>> estimatePdc(
            Dictionary<string, BayesEntry> training_set,
            Dictionary<string, int> trainingVocab)
        {
            //throw new Exception("Smoothed probability estimation not implemented yet");

            Dictionary<string, Dictionary<string, double>> wordProbabilities = new Dictionary<string, Dictionary<string, double>>();
            
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

            //foreach (string c in classCounts.Keys)
            //{
            //    double arg = double.NegativeInfinity;
            //    double arg1 = Math.Log10(probs.getClassProbability(c));
            //    double arg2 = 0.0;
            //    foreach (string word in testSetDoc.VocabOccur.Keys)
            //    {
            //        double wp = Math.Log10(probs.getWordProbability(word, c));

            //        if (wp == 0.0)
            //            throw new Exception("wp should never be zero");

            //        arg2 += wp;
            //    }

            //    arg = arg1 + arg2;
            //    if (arg > argmax)
            //    {
            //        argmax = arg;
            //        topC = c;
            //    }
            //}

            return topC;
        }
    }
}
