using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNBClassifier
{
    class SetOfRandom
    {
        private Dictionary<int, bool> isPresent;
        Random generator;
        int max;
        int numReturned;

        public SetOfRandom(int maximum)
        {
            max = maximum;
            numReturned = 0;
            generator = new Random();
            isPresent = new Dictionary<int, bool>();

        }

        public int nextUniqueNum()
        {
            if(numReturned >= max)
            {
                throw new Exception("This generator has reached its defined max number of unique positive integers");
            }

            int result;
            while(true)
            {
                result = generator.Next(max);

                if (!isPresent.ContainsKey(result))
                {
                    isPresent.Add(result, false);
                    break;
                }
                else if(!isPresent[result])
                {
                    break;
                }
            }

            isPresent[result] = true;
            numReturned++;
            return result;
        }
    }
}
