using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNBClassifier
{
    class ConfusionMatrix
    {
        private int[,] matrix;
        private List<string> rowAndColTitles;

        public ConfusionMatrix(List<string> allLabels)
        {
            rowAndColTitles = allLabels;
            int count = allLabels.Count;
            matrix = new int[count,count];
        }

        public void addEntry(string actual, string labeled)
        {
            matrix[rowAndColTitles.IndexOf(actual), rowAndColTitles.IndexOf(labeled)]++;
        }

        public void print()
        {
            foreach (string label in rowAndColTitles)
            {
                Console.Write(label + ",");
            }
            for (int i = 0; i < rowAndColTitles.Count; i++)
            {
                Console.WriteLine();
                for (int j = 0; j < rowAndColTitles.Count; j++)
                {
                    Console.Write(matrix[i, j] + ",");
                }
            }
        }

    }
}
