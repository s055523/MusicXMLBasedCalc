using System.Collections.Generic;
using libsvm;
using System.Linq;
using System.IO;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using System.Configuration;

namespace MusicXMLBasedCalc
{
    public static class SVMHelper
    {
        static double C
        {
            get
            {
                return double.Parse(ConfigurationManager.AppSettings["svm_c"]);
            }
        }
        static double gamma
        {
            get
            {
                return double.Parse(ConfigurationManager.AppSettings["svm_gamma"]);
            }
        }

        public static void LibSVM(List<string> inputData, List<string> testData)
        {
            var inputFilePath = @"D:\新西兰学习生活\大学上课\乐谱数据\input.txt";
            var testFilePath = @"D:\新西兰学习生活\大学上课\乐谱数据\test.txt";

            PrepareDataLibSvm(inputData, inputFilePath);
            PrepareDataLibSvm(testData, testFilePath);

            var _prob = ProblemHelper.ReadAndScaleProblem(inputFilePath);
            var svm = new C_SVC(_prob, KernelHelper.RadialBasisFunctionKernel(gamma), C);
        }

        public static void AccordSVM(List<string> inputData, List<string> testData, StreamWriter fw)
        {
            (int dimensionCount, double[][] inputs, int[] outputs) = PrepareDataAccordSvm(inputData);
            (int _, double[][] test, int[] answer) = PrepareDataAccordSvm(testData);

            var teacher = new MulticlassSupportVectorLearning<Gaussian>()
            {
                Learner = (param) => new SequentialMinimalOptimization<Gaussian>()
                {
                    // Estimate a suitable guess for the Gaussian kernel's parameters.
                    // This estimate can serve as a starting point for a grid search.
                    UseKernelEstimation = true
                }
            };

            // Learn a machine
            var machine = teacher.Learn(inputs, outputs);

            // Obtain class predictions for each sample
            int[] predicted = machine.Decide(inputs);

            // Get class scores for each sample
            double[] scores = machine.Score(inputs);

            List<SongResult> songResults = new List<SongResult>();

            //测试
            int i = 0;
            double accuracy;
            int correctCount = 0;
            foreach (var testDetail in test)
            {
                //目前的曲名
                var songName = testData[i].Split(',')[0];

                //曲名中去掉无关部分之后（.musicxml之前的内容）
                var name = songName.Split('.')[0];

                //统计一首歌（所有片段）然后取最多者
                if (!songResults.Select(s => s.name).Contains(name))
                {
                    var sr = new SongResult();
                    sr.name = name;
                    sr.label = answer[i];
                    sr.fragmentResults = new List<int>();
                    songResults.Add(sr);
                }

                var predict = machine.Decide(testDetail);
                fw.WriteLine($"歌曲：{songName}, 正确答案是{answer[i]}, SVM认为：{predict}");

                var songr = songResults.First(s => s.name == name);
                songr.fragmentResults.Add(predict);

                if (answer[i] == predict)
                {
                    correctCount++;
                }
                i++;    
            }
            accuracy = (double)correctCount / (double)test.Count();
            fw.WriteLine("SVM的正确率（分段）:" + accuracy);

            correctCount = 0;
            foreach (var sr in songResults)
            {
                IEnumerable<int> top4 = sr.fragmentResults
                                        .GroupBy(a => a)
                                        .OrderByDescending(g => g.Count())
                                        .Take(4)
                                        .Select(g => g.Key);

                fw.WriteLine($"歌曲：{sr.name}, 出现次数最多的是 {top4.First()}，正确答案是{sr.label}");
                sr.result = top4.First();
                if(top4.First() == sr.label)
                {
                    correctCount++;
                }
            }
            accuracy = (double)correctCount / (double)songResults.Count();
            fw.WriteLine("SVM的正确率（汇总）:" + accuracy);

        }

        public static void AccordSVMMultiClasses(List<string> inputData, List<string> testData, StreamWriter fw)
        {
            (int dimensionCount, double[][] inputs, int[] outputs) = PrepareDataAccordSvmMultiClasses(inputData);
            (int _, double[][] test, int[] answer) = PrepareDataAccordSvmMultiClasses(testData);

            var teacher = new MulticlassSupportVectorLearning<Gaussian>()
            {
                Learner = (param) => new SequentialMinimalOptimization<Gaussian>()
                {
                    // Estimate a suitable guess for the Gaussian kernel's parameters.
                    // This estimate can serve as a starting point for a grid search.
                    UseKernelEstimation = true
                }
            };

            // Learn a machine
            var machine = teacher.Learn(inputs, outputs);

            // Obtain class predictions for each sample
            int[] predicted = machine.Decide(inputs);

            // Get class scores for each sample
            double[] scores = machine.Score(inputs);

            List<SongResult> songResults = new List<SongResult>();

            //测试
            int i = 0;
            double accuracy;
            int correctCount = 0;
            foreach (var testDetail in test)
            {
                //目前的曲名
                var songName = testData[i].Split(',')[0];

                //曲名中去掉无关部分之后（.musicxml之前的内容）
                var name = songName.Split('.')[0];

                //统计一首歌（所有片段）然后取最多者
                if (!songResults.Select(s => s.name).Contains(name))
                {
                    var sr = new SongResult();
                    sr.name = name;
                    sr.label = answer[i];
                    sr.fragmentResults = new List<int>();
                    songResults.Add(sr);
                }

                var predict = machine.Decide(testDetail);
                fw.WriteLine($"歌曲：{testData[i].Split(',')[0]}, 正确答案是{answer[i]}, SVM认为：{predict}");

                var songr = songResults.First(s => s.name == name);
                songr.fragmentResults.Add(predict);

                if (answer[i] == predict)
                {
                    correctCount++;
                }
                i++;
            }
            accuracy = (double)correctCount / (double)test.Count();
            fw.WriteLine("SVM四类的正确率（分段）:" + accuracy);

            correctCount = 0;
            foreach (var sr in songResults)
            {
                IEnumerable<int> top4 = sr.fragmentResults
                                        .GroupBy(a => a)
                                        .OrderByDescending(g => g.Count())
                                        .Take(4)
                                        .Select(g => g.Key);

                fw.WriteLine($"歌曲：{sr.name}, 出现次数最多的是 {top4.First()}，正确答案是{sr.label}");
                sr.result = top4.First();
                if (top4.First() == sr.label)
                {
                    correctCount++;
                }
            }
            accuracy = (double)correctCount / (double)songResults.Count();
            fw.WriteLine("SVM的正确率（汇总）:" + accuracy);
        }

        public static void PrepareDataLibSvm(List<string> data, string testFilePath)
        {
            using (var w = new StreamWriter(testFilePath))
            {
                foreach (var str in data)
                {
                    var strArray = str.Split(',');
                    var fileName = strArray[0];
                    var dataStr = string.Empty;

                    if (fileName.Contains("巴洛克") || fileName.Contains("古典")) dataStr += "1";
                    else if (fileName.Contains("浪漫") || fileName.Contains("现代")) dataStr += "2";

                    dataStr += " ";

                    int count = 1;
                    foreach (var attribute in strArray.Skip(1))
                    {
                        dataStr += count;
                        dataStr += ":";
                        dataStr += attribute;
                        dataStr += " ";
                        count++;
                    }
                    w.WriteLine(dataStr.TrimEnd(' '));
                }
            }
        }

        public static (int, double[][], int[]) PrepareDataAccordSvm(List<string> data)
        {
            var dimensionCount = data[0].Length - 1;
            var dataLength = data.Count;
            var input = new double[dataLength][];
            var output = new int[dataLength];

            var i = 0;
            foreach (var str in data)
            {
                var strArray = str.Split(',');
                var fileName = strArray[0];
                var dataStr = string.Empty;

                //label
                if (fileName.Contains("巴洛克") || fileName.Contains("古典")) output[i] = 0;
                else if (fileName.Contains("浪漫") || fileName.Contains("现代")) output[i] = 1;

                input[i] = new double[strArray.Length - 1];
                int count = 0;
                foreach (var attribute in strArray.Skip(1))
                {
                    input[i][count] = double.Parse(attribute);
                    count++;
                }
                i++;
            }
            return (dimensionCount, input, output);
        }

        public static (int, double[][], int[]) PrepareDataAccordSvmMultiClasses(List<string> data)
        {
            //数据除了第一列是名字之外其他都是维度
            var dimensionCount = data[0].Length - 1;
            var dataLength = data.Count;
            var input = new double[dataLength][];
            var output = new int[dataLength];

            var i = 0;
            foreach (var str in data)
            {
                var strArray = str.Split(',');
                var fileName = strArray[0];
                var dataStr = string.Empty;

                //label
                if (fileName.Contains("巴洛克")) output[i] = 0;
                else if (fileName.Contains("古典")) output[i] = 1;
                else if (fileName.Contains("浪漫")) output[i] = 2;
                else output[i] = 3;

                input[i] = new double[strArray.Length - 1];
                int count = 0;
                foreach (var attribute in strArray.Skip(1))
                {
                    input[i][count] = double.Parse(attribute);
                    count++;
                }
                i++;
            }
            return (dimensionCount, input, output);
        }
    }

    public class SongResult
    {
        public string name { get; set; }
        public List<int> fragmentResults { get; set; }
        public int result { get; set; }
        public int label { get; set; }
    }
}
