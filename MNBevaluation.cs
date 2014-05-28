using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNBClassifier
{
    class MNBevaluation
    {
        private double accuracy;
        private string type;

        public MNBevaluation(string type)
        {
            accuracy = 0.0;
            this.type = type;
        }

        public double accuracyMeasure(
            Dictionary<string, BayesEntry> test_set,
            Dictionary<string, string> docLabels)
        {
            int totalLabels = test_set.Count;
            int numCorrect = 0;
            foreach(string doc in test_set.Keys)
            {
                if(test_set[doc].Label.Equals(docLabels[doc]))
                {
                    ++numCorrect;
                }
            }

            accuracy = (double)numCorrect / (double)totalLabels;

            return accuracy;
        }

        public ConfusionMatrix getConfusion(
            Dictionary<string, BayesEntry> test_set,
            Dictionary<string, string> docLabels,
            HashSet<string> allLabels)
        {
            List<string> labels = new List<string>(allLabels);
            ConfusionMatrix cm = new ConfusionMatrix(labels);
            foreach (string doc in test_set.Keys)
            {
                cm.addEntry(test_set[doc].Label, docLabels[doc]);
            }
            return cm;

        }
    }
}
