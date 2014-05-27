using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace MNBClassifier
{
    class Program
    {
        static void Main(string[] args)
        {
            //MNBclassification classify = new MNBclassification(6200);
            //MNBclassification classify = new MNBclassification(12400);
            //MNBclassification classify = new MNBclassification(18600);
            //MNBclassification classify = new MNBclassification(24800);
            MNBclassification classify = new MNBclassification(-1);

            Console.WriteLine("\tdone");
            Console.ReadLine();
        }
    }
}
