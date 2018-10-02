using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MusicXMLBasedCalc
{
    class Program
    {
        public const int FRAGMENT_LENGTH = 6;
        public const bool CUT = true;

        static void Main(string[] args)
        {
            //Debug
            var directory = @"D:\新西兰学习生活\大学上课\Debug";
            var inputResult = @"D:\新西兰学习生活\大学上课\Debug\Result.csv";
            var inputData = CalculateAndGenerate(directory, inputResult, CUT);

            //Input
            var inputDirectory = @"D:\新西兰学习生活\大学上课\乐谱数据";
            inputResult = @"D:\新西兰学习生活\大学上课\乐谱数据\Result.csv";
            inputData = CalculateAndGenerate(inputDirectory, inputResult, CUT);

            //Test
            var testDirectory = @"D:\新西兰学习生活\大学上课\测试数据";
            inputResult = @"D:\新西兰学习生活\大学上课\测试数据\Result.csv";
            var testData = CalculateAndGenerate(testDirectory, inputResult, CUT);

            var a = FRAGMENT_LENGTH + 1;
            var resultFile = DateTime.Now.ToString("yyyyMMddHHmmss") + (CUT == true ? "UseCut" : "");
            if (CUT)
            {
                resultFile += "FragmentLength=" + a;
            }
            resultFile += ".txt";

            using (var fw = new StreamWriter(@"D:\新西兰学习生活\大学上课\乐谱数据\" + resultFile))
            {
                fw.WriteLine("输入集：");
                
                if (CUT) fw.WriteLine("片段长度:" + a);
                fw.WriteLine("巴洛克：" + HowManySongs(inputDirectory, "巴洛克") + "首歌曲，" + HowManyMeasures(inputDirectory, "巴洛克") + "个片段");
                fw.WriteLine("古典：" + HowManySongs(inputDirectory, "古典") + "首歌曲，" + HowManyMeasures(inputDirectory, "古典") + "个片段");
                fw.WriteLine("浪漫与印象：" + HowManySongs(inputDirectory, "浪漫与印象") + "首歌曲，" + HowManyMeasures(inputDirectory, "浪漫与印象") + "个片段");
                fw.WriteLine("现代：" + HowManySongs(inputDirectory, "现代") + "首歌曲，" + HowManyMeasures(inputDirectory, "现代") + "个片段");
                fw.WriteLine("输入集总共：" + HowManySongs(inputDirectory, "") + "首歌曲，" + HowManyMeasures(inputDirectory, "") + "个片段");

                fw.WriteLine("测试集总共：" + HowManySongs(testDirectory, "") + "首歌曲，" + HowManyMeasures(testDirectory, "") + "个片段");

                //使用SVM进行分类
                //SVMHelper.AccordSVM(inputData, testData, fw);
                //
                SVMHelper.AccordSVMMultiClasses(inputData, testData, fw);

                Console.WriteLine("============================================");

                //使用KNN进行分类
                KNNHelper.AccordKNN(inputData, testData, fw);
            }
            Console.WriteLine("Finished!");
            //Console.ReadKey();

        }

        public static List<string> CalculateAndGenerate(string directory, string fileName, bool cut = false)
        {
            var directoryInfo = new DirectoryInfo(directory);
            var songFiles = directoryInfo.GetFiles("*.musicxml", SearchOption.AllDirectories);

            if (File.Exists(fileName)) File.Delete(fileName);
            var ret = new List<string>();

            if (cut)
            {
                foreach (var file in songFiles)
                {
                    var numOfMeasure = MusicXMLParser.GetNumOfMeasure(file.FullName);
                    
                    for (int i = 0; i < numOfMeasure; i += FRAGMENT_LENGTH)
                    {
                        var start = i;
                        var end = Math.Min(start + FRAGMENT_LENGTH, numOfMeasure);
                        if (end - start < FRAGMENT_LENGTH) break;
                        var song = new Song(file.FullName, "小节" + start + "-" + end, start, end);
                        song.SongAnalysis();
                        if (!song.skip)
                        {
                            ret.Add(song.PrintResultToCsv());
                        }
                        i++;
                    }
                }
            }
            else
            {
                foreach (var file in songFiles)
                {
                    var song = new Song(file.FullName, string.Empty);
                    song.SongAnalysis();
                    if (!song.skip)
                    {
                        ret.Add(song.PrintResultToCsv());
                    }
                }
            }

            //指定编码防止乱码
            using (StreamWriter file = new StreamWriter(fileName, true, Encoding.UTF8))
            {
                //Caption
                string caption = ",1，8度,4，5度,3度,6度,大2，小七,小二，大7，增四,音程平均值,音程方差,调外音,节奏方差,转调次数";
                file.WriteLine(caption);

                foreach (var str in ret)
                    file.WriteLine(str);
            }
            return ret;
        }

        public static int HowManySongs(string directory, string key)
        {
            var path = string.Empty;
            if (key == string.Empty) path = directory;
            else
                path = Path.Combine(directory, key);

            var dInfo = new DirectoryInfo(path);
            var songFiles = dInfo.GetFiles("*.musicxml", SearchOption.AllDirectories);
            return songFiles.Length;
        }

        public static int HowManyMeasures(string directory, string key)
        {
            var path = string.Empty;
            if (key == string.Empty) path = directory;
            else
                path = Path.Combine(directory, key);

            var dInfo = new DirectoryInfo(path);
            var songFiles = dInfo.GetFiles("*.musicxml", SearchOption.AllDirectories);

            if (!CUT) return songFiles.Length;

            var ret = 0;

            foreach(var songFile in songFiles)
            {
                var measureCount = MusicXMLParser.GetNumOfMeasure(songFile.FullName);
                ret += measureCount / FRAGMENT_LENGTH;
            }
            return ret;
        }
    }
}
