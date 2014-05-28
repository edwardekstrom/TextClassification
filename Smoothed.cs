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
    }
}
