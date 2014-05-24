using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;

namespace MNBClassifier
{
    class MNBclassification
    {
        private Dictionary<int, string> docList;
        private Dictionary<string, MultinomialEntry> training_set;
        private Dictionary<string, MultinomialEntry> test_set;
        private Dictionary<string, int> trainingVocab;
        private Dictionary<string, Dictionary<string, int>> docVocabCounts;
        private Dictionary<string, int> classCounts;
        private Dictionary<string, int> numDocsWithW;
        private Dictionary<string, Dictionary<string, int>> numDocsWithWinC;
        private Dictionary<string, Dictionary<string, int>> numWinC;
        private int totalTrainingTerms;
        private int numTotalDocs;
        private Stopwords stopWords;
        private int fileCount;
        private int eighty;
        private MNBprobability probs;
        private MNBevaluation eval;
        private Semaphore poolSem;
        private string basePath;
        private string pathToCollection;
        private string pathToScrubbed;
        private string pathToUnscrubbed;
        private double finalAccuracyValue;

        //************************************************************//
        //  CONSTRUCTOR for MNBClassification                         //
        //************************************************************//

        public MNBclassification(int M)
        {
            Stopwatch s = new Stopwatch();
            s.Start();

            // TODO: Change this to a usable directory when giving away
            basePath = ".";
            pathToCollection = basePath + @"\20NG";
            pathToScrubbed = basePath + @"\scrubbed";
            pathToUnscrubbed = basePath + @"\toScrub";

            // initialize statistical variables
            trainingVocab = new Dictionary<string, int>();
            classCounts = new Dictionary<string, int>();
            numDocsWithW = new Dictionary<string, int>();
            numDocsWithWinC = new Dictionary<string, Dictionary<string, int>>();
            numWinC = new Dictionary<string, Dictionary<string, int>>();
            totalTrainingTerms = 0;
            finalAccuracyValue = 0.0;

            int numScrubbedDocs = 0;
            if(Directory.Exists(pathToScrubbed))
                numScrubbedDocs = Directory.GetFiles(pathToScrubbed, "*.txt", SearchOption.AllDirectories).Length;
            string[] documents = Directory.GetFiles(pathToCollection, "*.*", SearchOption.AllDirectories);
            fileCount = documents.Length; // init fileCount
            eighty = (int)(((double)fileCount) * 0.8 + 0.5); // init eighty

            // Scrubbing Documents
            if (numScrubbedDocs < fileCount)
            {
                Stopwatch q = new Stopwatch();
                q.Start();
                Console.WriteLine("Scrubbing Documents...");
                stopWords = new Stopwords(); // init stopwords
                poolSem = new Semaphore(10, 10); // init pool sem
                for (int i = 0; i < fileCount && numScrubbedDocs < fileCount; ++i)
                {
                    string scrubbedFile = pathToScrubbed + "\\" + documents[i].Substring(documents[i].LastIndexOf("20NG")) + ".txt";
                    if (!File.Exists(scrubbedFile))
                    {
                        poolSem.WaitOne(); // only allow 10 threads at a time
                        Thread t = new Thread(new ParameterizedThreadStart(threadingThings));
                        t.Start(documents[i]);
                        numScrubbedDocs++;
                    }
                }
                Console.WriteLine("\tdone");
                q.Stop();
                Console.WriteLine("Document Scrub Time: " + q.Elapsed);
                s.Stop();
                Console.ReadLine();
                s.Start();
            }

            // REPEAT EVERYTHING AFTER THIS 5 TIMES
            long avgTime = 0;
            const int NUM_ITERATONS = 1;
            for (int i = 1; i <= NUM_ITERATONS; ++i)
            {
                Stopwatch p = new Stopwatch();
                p.Start();
                Console.WriteLine("Beginning iteration " + i);
                fiveFoldTestIteration(M);
                p.Stop();
                avgTime += p.ElapsedMilliseconds;
                Console.WriteLine("Iteration " + i + " done");
                Console.WriteLine("Iteration Processing Time: " + p.Elapsed);
                Console.WriteLine("******************************");
            }

            finalAccuracyValue /= NUM_ITERATONS;
            Console.WriteLine("Final Averaged Accuracy: " + finalAccuracyValue);

            avgTime /= NUM_ITERATONS;
            long milliseconds = avgTime % 1000;
            avgTime = (avgTime - milliseconds) / 1000;
            long seconds = avgTime % 60;
            avgTime = (avgTime - seconds) / 60;
            long minutes = avgTime;
            string avgTimeString = minutes.ToString("00") + ":" + seconds.ToString("00") + "." + milliseconds;
            Console.WriteLine("Average Iteration Processing Time = " + avgTimeString);
            
            s.Stop();
            Console.WriteLine("Total Processing Time = " + s.Elapsed);
        }

        //************************************************************//
        //  REQUIRED FUNCTIONS                                        //
        //************************************************************//

        public Dictionary<string, int> featureSelection(List<string> dc_training, int M)
        {
            // declare feature list
            Dictionary<string, int> featureList = new Dictionary<string, int>();

            Console.WriteLine("Gathering Training Document Statistics...");
            gatherStats(dc_training);
            Console.WriteLine("\tdone");

            if(M == -1 || M >= trainingVocab.Count)
            {
                // using the entire vocab
                foreach(string word in trainingVocab.Keys)
                {
                    featureList.Add(word, 0);
                }
                return featureList;
            }

            Console.WriteLine("Calculating Selected features...");
            List<KeyValuePair<string, double>> rawList = new List<KeyValuePair<string, double>>();
            // calculate the information gain on each vocab word
            int count = 0;
            foreach(string word in trainingVocab.Keys)
            {
                double informationGain = IG(word);

                rawList.Add(new KeyValuePair<string, double>(word, informationGain));
                //Console.WriteLine(count + "\t" + word + "\t" + informationGain);
                ++count;
            }
            // sort feature list by information gain
            rawList.Sort(Compare);
            // add the first M items to the feature list
            for (int i = 0; i < M; ++i)
            {
                featureList.Add(rawList[i].Key, 0);
                //Console.WriteLine(rawList[i].Key + "\t" + rawList[i].Value);
            }

            Console.WriteLine("\tdone");

            return featureList; // return the set of vocab of size M with the greatest Information Gain
        }

        public string label(MultinomialEntry testSetDoc)
        {
            double argmax = double.NegativeInfinity;
            string topC = "";
            foreach(string c in classCounts.Keys)
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
                if(arg > argmax)
                {
                    argmax = arg;
                    topC = c;
                }
            }

            //Console.WriteLine("topC:\t" + topC);
            return topC;
        }

        //************************************************************//
        //  PRIVATE HELPER FUNCTIONS                                  //
        //************************************************************//

        public double FinalAccuracy
        {
            get { return finalAccuracyValue; }
        }

        private void fiveFoldTestIteration(int M)
        {
            // initialize probability and evaluation variables
            probs = new MNBprobability();
            eval = new MNBevaluation();

            // reinitialize some other variables
            docVocabCounts = new Dictionary<string, Dictionary<string, int>>();
            training_set = new Dictionary<string, MultinomialEntry>();
            test_set = new Dictionary<string, MultinomialEntry>();

            // Map unique random IDs to all Documents - always yeilds about 76 percent accuracy
            Console.WriteLine("Identifying Scrubbed Documents...");

            // CS 470 document sorting code
            string[] scrubbedDocsTraining = Directory.GetFiles(pathToScrubbed + "\\20NG\\20news-bydate-train", "*.txt", SearchOption.AllDirectories);
            string[] scrubbedDocsTest = Directory.GetFiles(pathToScrubbed + "\\20NG\\20news-bydate-test", "*.txt", SearchOption.AllDirectories);
            // separate the documents into the dc_training and dc_test sets
            List<string> dc_training = new List<string>(); // init dc_training
            List<string> dc_test = new List<string>(); // init dc_test
            foreach(string trDoc in scrubbedDocsTraining)
            {
                dc_training.Add(trDoc);
            }
            foreach(string testDoc in scrubbedDocsTest)
            {
                dc_test.Add(testDoc);
            }

            // ORIGINAL document sorting code
            //SetOfRandom r = new SetOfRandom(fileCount);
            //string[] scrubbedDocs = Directory.GetFiles(pathToScrubbed, "*.txt", SearchOption.AllDirectories);
            //docList = new Dictionary<int, string>();
            //foreach (string doc in scrubbedDocs)
            //{
            //    docList.Add(r.nextUniqueNum(), doc);
            //}
            //
            //// separate the documents into the dc_training and dc_test sets
            //List<string> dc_training = new List<string>(); // init dc_training
            //List<string> dc_test = new List<string>(); // init dc_test
            //for (int i = 0; i < eighty; ++i)
            //{
            //    dc_training.Add(docList[i]);
            //}
            //for (int i = eighty; i < fileCount; ++i)
            //{
            //    dc_test.Add(docList[i]);
            //}

            numTotalDocs = dc_training.Count + dc_test.Count;
            Console.WriteLine("\tdone -> " + (numTotalDocs) + " documents found");

            // return the selected feature set
            Dictionary<string, int> selectedFeatures = featureSelection(dc_training, M);

            // create the training_set structure
            Console.WriteLine("Creating training_set structure...");
            foreach (string doc in dc_training)
            {
                Dictionary<string, int> subsetVCounts = new Dictionary<string, int>();
                foreach (string feature in selectedFeatures.Keys)
                {
                    if (docVocabCounts[doc].ContainsKey(feature))
                    {
                        subsetVCounts.Add(feature, docVocabCounts[doc][feature]);
                    }
                }

                training_set.Add(doc, new MultinomialEntry(subsetVCounts, docToClass(doc)));
            }
            Console.WriteLine("\tdone");

            // create the test_set structure
            Console.WriteLine("Creating test_set structure...");
            foreach (string doc in dc_test)
            {
                Dictionary<string, int> thisDocVocab = new Dictionary<string, int>();
                string[] testLines = File.ReadAllLines(doc);
                foreach (string line in testLines)
                {
                    string[] wc = line.Split(' ');
                    if (selectedFeatures.ContainsKey(wc[0]))
                    {
                        if (!thisDocVocab.ContainsKey(wc[0]))
                        {
                            thisDocVocab.Add(wc[0], int.Parse(wc[1]));
                        }
                        else
                        {
                            thisDocVocab[wc[0]] += int.Parse(wc[1]);
                        }
                    }
                }
                test_set.Add(doc, new MultinomialEntry(thisDocVocab, docToClass(doc)));
            }
            Console.WriteLine("\tdone");

            // compute word probabilities
            Console.WriteLine("Computing wordProbabilites...");
            probs.computeWordProbability(training_set, trainingVocab);
            Console.WriteLine("\tdone");

            // compute class probabilites
            Console.WriteLine("Computing classProbabilites...");
            probs.computeClassProbability(training_set, classCounts);
            Console.WriteLine("\tdone");

            // label the test documents
            Console.WriteLine("Labeling Test Documents...");
            Dictionary<string, string> testDocLabels = new Dictionary<string, string>();
            foreach (string testDoc in test_set.Keys)
            {
                testDocLabels.Add(testDoc, label(test_set[testDoc]));
            }
            Console.WriteLine("\tdone");

            // get the accuracy measure
            Console.WriteLine("Getting accuracy measure...");
            double accuracyValue = eval.accuracyMeasure(test_set, testDocLabels);
            finalAccuracyValue += accuracyValue;
            Console.WriteLine("\tdone");

            // display accuracy
            Console.WriteLine("Labeling Accuracy: " + accuracyValue);
        }

        private int Compare(KeyValuePair<string, double> a, KeyValuePair<string, double> b)
        {
            if (a.Value < b.Value)
                return 1;
            else if (a.Value > b.Value)
                return -1;
            else
                return 0;
        }

        private string docToClass(string doc)
        {
            string prefix = "20NG\\";
            int start = doc.LastIndexOf(prefix) + prefix.Length;
            string docClass = doc.Substring(start, doc.LastIndexOf('\\') - start);
            docClass = docClass.Substring(docClass.IndexOf('\\') + 1);

            return docClass;
        }

        private void gatherStats(List<string> dc_training)
        {
            // cycle through the docs
            foreach (string doc in dc_training)
            {
                // get words and counts
                string[] docText = File.ReadAllLines(doc);

                // calculate the number of docs in each class
                string docClass = docToClass(doc);
                if (!classCounts.ContainsKey(docClass))
                {
                    classCounts.Add(docClass, 1);
                }
                else
                {
                    ++classCounts[docClass];
                }

                // increment various statistical values
                Dictionary<string, int> addToDocsWithW = new Dictionary<string, int>();
                Dictionary<string, int> addToDocVCounts = new Dictionary<string, int>();
                foreach (string line in docText)
                {
                    string[] wc = line.Split(' ');

                    // increment total training terms
                    ++totalTrainingTerms;

                    // increment counts for words in this doc
                    if (!addToDocsWithW.ContainsKey(wc[0]))
                    {
                        addToDocsWithW.Add(wc[0], 1);
                    }

                    // increment the number of times in class C that W happens
                    if(!numWinC.ContainsKey(docClass))
                    {
                        Dictionary<string, int> numW = new Dictionary<string, int>();
                        numW.Add(wc[0], 1);
                        numWinC.Add(docClass, numW);
                    }
                    else if (!numWinC[docClass].ContainsKey(wc[0]))
                    {
                        numWinC[docClass].Add(wc[0], 1);
                    }
                    else
                    {
                        ++numWinC[docClass][wc[0]];
                    }

                    // add words to class wide training vocab
                    if (!trainingVocab.ContainsKey(wc[0]))
                    {
                        trainingVocab.Add(wc[0], int.Parse(wc[1]));
                    }
                    else
                    {
                        trainingVocab[wc[0]] += int.Parse(wc[1]);
                    }

                    // add words to doc vocab
                    if (!addToDocVCounts.ContainsKey(wc[0]))
                    {
                        addToDocVCounts.Add(wc[0], int.Parse(wc[1]));
                    }
                    else
                    {
                        addToDocVCounts[wc[0]] += int.Parse(wc[1]);
                    }

                    // increment the num docs containing W that are labled as class C
                    if(!numDocsWithWinC.ContainsKey(wc[0]))
                    {
                        Dictionary<string, int> wCountInC = new Dictionary<string, int>();
                        wCountInC.Add(docClass, 1);
                        numDocsWithWinC.Add(wc[0], wCountInC);
                    }
                    else if(!numDocsWithWinC[wc[0]].ContainsKey(docClass))
                    {
                        numDocsWithWinC[wc[0]].Add(docClass, 1);
                    }
                    else
                    {
                        ++numDocsWithWinC[wc[0]][docClass];
                    }
                }

                // add the doc vocab to the dictionary
                docVocabCounts.Add(doc, addToDocVCounts);
                // add the word counts from this doc to the words counts for all the docs
                foreach (string key in addToDocsWithW.Keys)
                {
                    if (!numDocsWithW.ContainsKey(key))
                    {
                        numDocsWithW.Add(key, 1);
                    }
                    else
                    {
                        ++numDocsWithW[key];
                    }
                }
            }
        }

        private void threadingThings(object docPath)
        {
            Dictionary<string, int> rawTrainingVocab = new Dictionary<string, int>();

            string doc = (string)docPath;
            string docToUse = doc.Substring(doc.IndexOf("20NG")) + ".txt";
            
            Console.WriteLine(docToUse + " does not exist...creating");
            // remove the header from each doc,
            //  remove the stopwords,
            //  and add the set of vocab words to the raw vocab
            removeStuff(doc, ref rawTrainingVocab);
            // stem the words in the document
            stemDocWords(docToUse, rawTrainingVocab);

            poolSem.Release();
        }

        private void removeStuff(string docPath, ref Dictionary<string, int> rawVocab)
        {
            Regex rx = new Regex(@"[^a-zA-Z]+");

            StreamReader file = new StreamReader(docPath);

            // remove the header from each doc
            string line;
            while((line = file.ReadLine()) != null)
            {
                if (line.StartsWith("Lines: "))
                    break;
            }

            while ((line = file.ReadLine()) != null)
            {
                string[] words = rx.Split(line);

                foreach (string word in words)
                {
                    // don't add empty strings or stopwords
                    if (word.Equals("") || stopWords.contains(word))
                        continue;

                    // add to rawVocab
                    string toAdd = word.ToLower();
                    if(!rawVocab.ContainsKey(toAdd))
                    {
                        rawVocab.Add(toAdd.ToLower(), 1);
                    }
                    else
                    {
                        ++rawVocab[toAdd];
                    }
                }
            }

            file.Close();
        }

        private Dictionary<string, int> stemDocWords(string doc, Dictionary<string, int> rawVocab)
        {

            string toStemDoc = pathToUnscrubbed + "\\" + doc;
            string stemmedDoc = pathToScrubbed + "\\" + doc;

            if (!File.Exists(toStemDoc))
            {
                string processText = "";
                foreach (string w in rawVocab.Keys)
                {
                    processText += w + " " + rawVocab[w] + "\r\n";
                }
                string toScrubDir = toStemDoc.Substring(0, toStemDoc.LastIndexOf('\\'));
                if (!Directory.Exists(toScrubDir))
                    Directory.CreateDirectory(toScrubDir);
                File.WriteAllText(toStemDoc, processText);
            }

            string scrubDir = stemmedDoc.Substring(0, stemmedDoc.LastIndexOf('\\'));
            if (!Directory.Exists(scrubDir))
                Directory.CreateDirectory(scrubDir);

            Process p = new Process();
            p.StartInfo.FileName = "\"C:\\Program Files\\Java\\jre7\\bin\\java.exe\"";
            p.StartInfo.Arguments = "-jar Stemmer.jar \"" + toStemDoc + "\" \"" + stemmedDoc + "\"";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();
            p.WaitForExit();

            Dictionary<string, int> stemmedVocab = new Dictionary<string, int>();
            string[] stemmedLines = File.ReadAllLines(stemmedDoc);
            foreach(string line in stemmedLines)
            {
                string[] entry = line.Split(' ');
                if(!stemmedVocab.ContainsKey(entry[0]))
                {
                    stemmedVocab.Add(entry[0], int.Parse(entry[1]));
                }
                else
                {
                    stemmedVocab[entry[0]] += int.Parse(entry[1]);
                    //Console.WriteLine("this happened");
                }
            }

            return stemmedVocab;
        }

        //************************************************************//
        //  INFORMATION GAIN EQUATIONS                                //
        //************************************************************//

        private double IG(string word)
        {
            double retVal = -sum(word, 1) + (Pw(word) * sum(word, 2)) + (Pwnot(word) * sum(word, 3));
            return retVal;
        }

        private double sum(string w, int part)
        {
            double sum = 0.0;
            double temp = 0.0;
            foreach (string c in classCounts.Keys)
            {
                switch (part)
                {
                    // Part 1
                    case 1:
                        sum += (Pc(c) * Math.Log(Pc(c), 2));
                        break;
                    // Part 2
                    case 2:
                        temp = Pcw(c, w);
                        sum += (temp != 0 ? temp * Math.Log(temp, 2) : 0);
                        break;
                    // Part 3
                    case 3:
                        temp = Pcwnot(c, w);
                        sum += (temp != 0 ? temp * Math.Log(temp, 2) : 0);
                        break;
                    default:
                        break;
                } 
            }

            //Console.WriteLine(w + "\t" + temp + "\t" + sum);
            return sum;
        }

        private double Pc(string c)
        {
            return (double)classCounts[c] / (double)numTotalDocs;
        }

        private double Pw(string word)
        {
            return (double)numDocsWithW[word] / (double)numTotalDocs;
        }

        private double Pwnot(string notWord)
        {
            return (double)(numTotalDocs - numDocsWithW[notWord]) / (double)numTotalDocs;
        }

        private double Pcw(string c, string word)
        {
            if(numDocsWithW[word] == 0)
            {
                throw new Exception("numDocsWithW has a zero count in Pcw: " + word);
            }

            try
            {
                return (double)numDocsWithWinC[word][c] / (double)numDocsWithW[word];
            }
            catch (Exception)
            {
                return 0.0;
            }
        }

        private double Pcwnot(string c, string notWord)
        {
            if (numDocsWithW[notWord] == 0)
            {
                throw new Exception("numDocsWithW has a zero count in Pcwnot: " + notWord);
            }

            try
            {
                return (double)(classCounts[c] - numDocsWithWinC[notWord][c]) / (double)(numTotalDocs - numDocsWithW[notWord]);
            }
            catch (Exception)
            {
                return (double)(classCounts[c]) / (double)(numTotalDocs - numDocsWithW[notWord]);
            }
        }
    }
}
