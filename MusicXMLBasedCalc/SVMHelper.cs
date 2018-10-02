using System;
using System.Collections.Generic;
using libsvm;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.MachineLearning.VectorMachines;
using Accord.Statistics.Kernels;

namespace MusicXMLBasedCalc
{
    public static class SVMHelper
    {
        static double C = 0.8;
        static double gamma = 0.0025;

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

            //测试
            int i = 0;
            double accuracy;
            int correctCount = 0;
            foreach (var testDetail in test)
            {
                var predict = machine.Decide(testDetail);
                fw.WriteLine($"歌曲：{testData[i].Split(',')[0]}, 正确答案是{answer[i]}, SVM认为：{predict}");
                if (answer[i] == predict)
                {
                    correctCount++;
                }
                i++;    
            }
            accuracy = (double)correctCount / (double)test.Count();
            fw.WriteLine("SVM的正确率:" + accuracy);
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

            //测试
            int i = 0;
            double accuracy;
            int correctCount = 0;
            foreach (var testDetail in test)
            {
                var predict = machine.Decide(testDetail);
                fw.WriteLine($"歌曲：{testData[i].Split(',')[0]}, 正确答案是{answer[i]}, SVM认为：{predict}");

                if(answer[i] == predict)
                {
                    correctCount++;
                }
                i++;
            }
            accuracy = (double)correctCount / (double)test.Count();
            fw.WriteLine("SVM四类的正确率:" + accuracy);
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
}
