using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;

namespace MusicXMLBasedCalc
{
    class Program
    {
        public static int FRAGMENT_LENGTH;
        public static bool CUT = true;
        public static string inputDirectory
        {
            get
            {
                return ConfigurationManager.AppSettings["inputDirectory"];
            }
        }
        public static string inputResult
        {
            get
            {
                return ConfigurationManager.AppSettings["inputResult"];
            }
        }
        public static string testDirectory
        {
            get
            {
                return ConfigurationManager.AppSettings["testDirectory"];
            }
        }
        public static string testResult
        {
            get
            {
                return ConfigurationManager.AppSettings["testResult"];
            }
        }

        static void Main(string[] args)
        {
            //Debug
            //var directory = @"D:\新西兰学习生活\大学上课\Debug";
            //var inputResult = @"D:\新西兰学习生活\大学上课\Debug\Result.csv";
            //var inputData = CalculateAndGenerate(directory, inputResult, CUT);

            if(args.Length > 0)
            {
                FRAGMENT_LENGTH = int.Parse(args[0]);
                Console.WriteLine("片段长度:" + FRAGMENT_LENGTH);
                if(FRAGMENT_LENGTH == -1)
                {
                    CUT = false;
                }
            }

            //Input
            var inputData = CalculateAndGenerate(inputDirectory, inputResult, CUT);

            //Test
            var testData = CalculateAndGenerate(testDirectory, testResult, CUT);

            var a = FRAGMENT_LENGTH + 1;
            var resultFile = DateTime.Now.ToString("yyyyMMddHHmmss") + (CUT == true ? "UseCut" : "");
            if (CUT)
            {
                resultFile += "FragmentLength=" + a;
            }
            resultFile += ".txt";

            using (var fw = new StreamWriter(Path.Combine(inputDirectory, resultFile)))
            {
                fw.WriteLine("目录：" + inputDirectory);
                fw.WriteLine("和弦音程权重倍数：" + ConfigurationManager.AppSettings["chord_weight"]);

                fw.WriteLine("输入集：");
                
                if (CUT) fw.WriteLine("片段长度:" + a);
                fw.WriteLine("巴洛克：" + HowManySongs(inputDirectory, "巴洛克") + "首歌曲，" + HowManyMeasures(inputDirectory, "巴洛克") + "个片段");
                fw.WriteLine("古典：" + HowManySongs(inputDirectory, "古典") + "首歌曲，" + HowManyMeasures(inputDirectory, "古典") + "个片段");
                fw.WriteLine("浪漫：" + HowManySongs(inputDirectory, "浪漫") + "首歌曲，" + HowManyMeasures(inputDirectory, "浪漫") + "个片段");
                fw.WriteLine("印象与现代：" + HowManySongs(inputDirectory, "印象与现代") + "首歌曲，" + HowManyMeasures(inputDirectory, "印象与现代") + "个片段");
                fw.WriteLine("输入集总共：" + HowManySongs(inputDirectory, "") + "首歌曲，" + HowManyMeasures(inputDirectory, "") + "个片段");

                fw.WriteLine("测试集总共：" + HowManySongs(testDirectory, "") + "首歌曲，" + HowManyMeasures(testDirectory, "") + "个片段");
                 
                //使用SVM进行分类
                SVMHelper.AccordSVM(inputData, testData, fw);
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
            var songList = new List<Song>();

            //整体的分析歌曲
            foreach (var file in songFiles)
            {
                var song = new Song(file.FullName, string.Empty);
                songList.Add(song);
                song.Parse();
                song.IntervalAnalysis();
                song.SongAnalysis();

                if (!song.skip)
                {
                    ret.Add(song.PrintResultToCsv());
                }
            }

            if (cut)
            {
                ret.Clear();
                foreach (var song in songList)
                {
                    var numOfMeasure = Song.GetNumOfMeasure(song.fileName);                  
                    for (int i = 1; i < numOfMeasure; i += FRAGMENT_LENGTH)
                    {
                        var start = i;
                        var end = Math.Min(start + FRAGMENT_LENGTH, numOfMeasure);
                        if (end - start < FRAGMENT_LENGTH)
                        {
                            break;
                        }
                        else
                        {
                            song.SetSuffix($"小节{start}-{end}");
                            song.SongAnalysis(start, end);

                            if (!song.skip)
                            {
                                ret.Add(song.PrintResultToCsv());
                            }
                        }
                        i++;
                    }
                }
            }

            //指定编码防止乱码
            using (StreamWriter file = new StreamWriter(fileName, true, Encoding.UTF8))
            {
                //Caption
                var type = typeof(Song);
                var properties = type.GetProperties();

                var caption = "Name,";
                foreach (PropertyInfo property in properties)
                {
                    caption += property.Name + ",";
                }

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
                var measureCount = Song.GetNumOfMeasure(songFile.FullName);
                ret += measureCount / FRAGMENT_LENGTH;
            }
            return ret;
        }
    }
}
