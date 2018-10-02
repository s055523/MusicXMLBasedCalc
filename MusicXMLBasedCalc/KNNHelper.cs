using Accord.MachineLearning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicXMLBasedCalc
{
    public class KNNHelper
    {
        public static void AccordKNN(List<string> inputData, List<string> testData, System.IO.StreamWriter fw)
        {
            (int dimensionCount, double[][] inputs, int[] outputs) = PrepareDataAccordKNN(inputData);
            (int _, double[][] test, int[] answer) = PrepareDataAccordKNN(testData);

            for (var k = 1; k < 10; k++){
                KNNCompute(k, inputs, outputs, test, answer, testData, fw);
            }
        }

        private static void KNNCompute(int K, double[][] inputs, int[] outputs, double[][] test, int[] answer, List<string> testData, System.IO.StreamWriter fw)
        {
            var knn = new KNearestNeighbors(K);
            knn.Learn(inputs, outputs);

            //测试
            int i = 0;
            double accuracy;
            int correctCount = 0;
            foreach (var testDetail in test)
            {
                var predict = knn.Decide(testDetail);
                //fw.WriteLine($"歌曲：{testData[i].Split(',')[0]}, 正确答案是{answer[i]}, KNN(K={K}认为)：{predict}");
                if (answer[i] == predict)
                {
                    correctCount++;
                }
                i++;
            }
            accuracy = (double)correctCount / (double)test.Count();
            fw.WriteLine($"KNN(K={K})的正确率:" + accuracy);
        }

        private static (int, double[][] input, int[] output) PrepareDataAccordKNN(List<string> data)
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
