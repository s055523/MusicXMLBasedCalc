using MusicXMLBasedCalc.BasicStructures;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MusicXMLBasedCalc
{
    public class Song
    {
        //曲名
        public string Name { get; set; }

        public string fileName;

        public double division;

        //这首曲子所有的音符
        public List<Result> songNotes;
        public List<Note> outOfKeyNotes;

        //这首曲子所有的音程
        public List<Interval> intervalList;

        public List<Scale> scaleList;

        public int numOfMeasures;

        //和弦的音程权重
        private double CHORD_INTERVAL_WEIGHT
        {
            get
            {
                return double.Parse(ConfigurationManager.AppSettings["chord_weight"]);
            }
        }
                
        public bool skip = false;

        //维度
        public double PercentageIntervals1 { get; set; }
        //public double PercentageIntervals2 { get; set; }
        //public double PercentageIntervals3 { get; set; }
        //public double PercentageIntervals4 { get; set; }
        public double PercentageIntervals5 { get; set; }
        public double PercentageIntervals6 { get; set; }

        //public double ConsonanceMean { get; set; }

        //if it is small, this song contains both consonance and dissonance intervals
        public double ConsonanceIntervalVariance { get; set; }

        public double PercentageOutOfKey { get; set; }

        public double durationVariance { get; set; }
        
        public int NumOfMordent { get; set; }
        public int NumOfTurn { get; set; }
        public int NumOfTrill { get; set; }

        //public int keyNum { get; set; }

        public Song(string f, string suffix, int start = -1, int end = -1)
        {
            Name = f.Split('\\').Last() + suffix;
            fileName = f;
        }

        public static int GetNumOfMeasure(string fileName)
        {
            //Guard clause
            if (!File.Exists(fileName))
            {
                throw new Exception($"File {fileName} not found!");
            }
            Console.WriteLine(fileName);
            XElement rootNode = XElement.Load(fileName);

            //Find part node, it contains all measures
            IEnumerable<XElement> measures = from target in rootNode.Descendants("measure")
                                             select target;
            return measures.Count();
        }

        /// <summary>
        /// 在一定范围之内搜索调号
        /// </summary>
        /// <param name="measures"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public void GetScales(IEnumerable<XElement> measures, int start = -1, int end = -1)
        {
            scaleList = new List<Scale>();
            var numOfMeasures = measures.Count();

            foreach (var measure in measures)
            {
                var scaleNode = measure.Descendants("key").FirstOrDefault();
                if (scaleNode != null)
                {
                    var measureNumber = measure.Attribute("number").Value;
                    var keyMode = string.Empty;
                    var fifth = scaleNode.Descendants("fifths").First();
                    var mode = scaleNode.Descendants("mode").FirstOrDefault();
                    if (mode == null)
                    {
                        keyMode = "major";
                    }
                    else
                    {
                        keyMode = mode.Value;
                    }

                    //新的调
                    var key = new Scale(keyMode, int.Parse(fifth.Value));
                    Console.WriteLine($"新的调，为{key.baseNoteName.Substring(0, key.baseNoteName.Length - 1)} {key.mode}，从第{measureNumber}小节开始。");
                    key.startMeasureNumber = int.Parse(measureNumber);
                    scaleList.Add(key);
                }
            }

            //为调的终止小节赋值
            for (int i = 0; i < scaleList.Count; i++)
            {
                if (i + 1 == scaleList.Count)
                {
                    scaleList[i].endMeasureNumber = numOfMeasures;
                }
                else
                {
                    scaleList[i].endMeasureNumber = scaleList[i + 1].startMeasureNumber - 1;
                }
            }
            //if (start == -1 && end == -1) return scaleList;

            //var fragmentKeyList = new List<Scale>();
            //foreach (var key in scaleList)
            //{
            //    if (key.endMeasureNumber >= start && key.startMeasureNumber <= end)
            //    {
            //        fragmentKeyList.Add(key);
            //    }
            //}
            //return fragmentKeyList;
        }

        public void Parse()
        {
            //Guard clause
            if (!File.Exists(fileName))
            {
                throw new Exception($"File {fileName} not found!");
            }
            Console.WriteLine(fileName);
            XElement rootNode = XElement.Load(fileName);

            //Find part node, it contains all measures
            IEnumerable<XElement> parts = from target in rootNode.Descendants("part")
                                          select target;

            songNotes = new List<Result>();

            //节拍号在第一个小节
            var firstMeasure = parts.First().Descendants("measure").First();

            //division标志，只有一个
            var divisionCount = firstMeasure.Descendants("divisions").Count();
            if (divisionCount > 1)
            {
                throw new Exception("多于一个division");
            }
            if (divisionCount < 1)
            {
                throw new Exception("没有division");
            }

            //所有的duration都要除这个数
            division = double.Parse(firstMeasure.Descendants("divisions").First().Value);

            //double currDurationInMeasure = Measure.GetDurationInMeasure(firstMeasure.Descendants("divisions").First());

            foreach (var part in parts)
            {
                var measures = part.Descendants("measure");

                //获得所有调和他们的范围
                GetScales(part.Descendants("measure"));

                foreach (var measure in measures)
                {
                    //每小节有几拍
                    //double durationInMeasure = Measure.GetDurationInMeasure(measure);

                    ////没有写节拍号，意味着节拍没有变
                    //if (durationInMeasure == -1)
                    //{
                    //    durationInMeasure = currDurationInMeasure;
                    //}

                    ////更新节拍
                    //else if (durationInMeasure != currDurationInMeasure)
                    //{
                    //    currDurationInMeasure = durationInMeasure;
                    //}

                    Measure m = new Measure();
                    m.Parse(scaleList, measure, division);

                    var mergedNoteResults = MergeResults(m.noteResults);
                    songNotes.AddRange(mergedNoteResults);
                }
            }
        }

        private static List<Result> MergeResults(List<Result> results)
        {
            var mergedResults = new List<Result>();
            var groups = results.OrderBy(r => r.position).GroupBy(r => r.position);

            foreach (var g in groups)
            {
                var key = g.Key;
                var notes = results.Where(r => r.position == key);
                var information = string.Join(",", notes.Select(n => n.information));
                mergedResults.Add(new Result(key, information, notes.SelectMany(n => n.notes).ToList()));
            }
            return mergedResults;
        }

        public void SongAnalysis(int start = -1, int end = -1)
        {
            XElement rootNode = XElement.Load(fileName);

            KeyAnalysis(rootNode, start, end);

            OrnamentAnalysis(start, end);

            var consonanceArray = new List<double>();

            double totalIntervalSum = 0;
            if (start != -1 && end != -1)
                totalIntervalSum = intervalList.Where(i => i.measureNumber >= start && i.measureNumber <= end).Sum(i => i.weight);
            else
                totalIntervalSum = intervalList.Sum(i => i.weight);

            if (totalIntervalSum == 0)
            {
                skip = true;
            }

            PercentageIntervals1 = Math.Round(GetNumberOfIntervals(ConsonanceCatagories.perfectConsonance, start, end) / totalIntervalSum, 3);
            consonanceArray.Add(PercentageIntervals1);
            //PercentageIntervals2 = Math.Round(GetNumberOfIntervals(ConsonanceCatagories.medianConsonance, start, end) / totalIntervalSum, 3);
            //consonanceArray.Add(PercentageIntervals2);
            //PercentageIntervals3 = Math.Round(GetNumberOfIntervals(ConsonanceCatagories.imperfectConsonance, start, end) / totalIntervalSum, 3);
            //consonanceArray.Add(PercentageIntervals3);
            //PercentageIntervals4 = Math.Round(GetNumberOfIntervals(ConsonanceCatagories.imperfectDissonance, start, end) / totalIntervalSum, 3);
            //consonanceArray.Add(PercentageIntervals4);
            PercentageIntervals5 = Math.Round(GetNumberOfIntervals(ConsonanceCatagories.medianDissonance, start, end) / totalIntervalSum, 3);
            consonanceArray.Add(PercentageIntervals5);
            PercentageIntervals6 = Math.Round(GetNumberOfIntervals(ConsonanceCatagories.perfectDisonnace, start, end) / totalIntervalSum, 3);
            consonanceArray.Add(PercentageIntervals6);

            //ConsonanceMean = consonanceArray.Average();
            ConsonanceIntervalVariance = consonanceArray.Variance();

            DurationAnalysis(start, end);
        }

        public void IntervalAnalysis()
        {
            intervalList = new List<Interval>();
            for (int i = 0; i < songNotes.Count; i++)
            {
                var currentLineNotes = songNotes[i].notes;

                //处于相同位置的音程（和弦）
                if (currentLineNotes.Count > 1)
                {
                    for (int j = 0; j < currentLineNotes.Count; j++)
                    {
                        for (int k = j + 1; k < currentLineNotes.Count; k++)
                        {
                            var interval = new Interval(currentLineNotes[j], currentLineNotes[k], CHORD_INTERVAL_WEIGHT);
                            interval.measureNumber = currentLineNotes[j].measureNumber;
                            intervalList.Add(interval);
                        }
                    }
                }

                if (i == 0) continue;
                var lastLineNotes = songNotes[i - 1].notes;

                //和上一个音/和弦的音程关系
                for (int j = 0; j < currentLineNotes.Count; j++)
                {
                    for (int k = 0; k < lastLineNotes.Count; k++)
                    {
                        var interval = new Interval(currentLineNotes[j], lastLineNotes[k], 1);
                        interval.measureNumber = currentLineNotes[j].measureNumber;
                        intervalList.Add(interval);
                    }
                }
            }
        }

        public void KeyAnalysis(XElement rootNode, int start = -1, int end = -1)
        {          
            outOfKeyNotes = new List<Note>();
            var notes = songNotes.SelectMany(n => n.notes);

            if (start == -1 && end == -1)
            {
                //统计调外音的数目
                foreach (var key in scaleList)
                {                    
                    var noteWithinKey = notes.Where(n => n.measureNumber < key.endMeasureNumber && n.measureNumber >= key.startMeasureNumber);
                    GetNoteOutOfKeyNumber(key, noteWithinKey);
                }

                var allNotes = songNotes.SelectMany(n => n.notes);
                PercentageOutOfKey = outOfKeyNotes.Sum(n => n.duration) / allNotes.Sum(n => n.duration);
                //keyNum = keyList.Count();
            }
            else
            {
                notes = notes.Where(n => n.measureNumber >= start && n.measureNumber <= end);

                //介于start和end之间的调
                var sl = scaleList.Where(s => (s.startMeasureNumber <= start && s.endMeasureNumber >= start) || 
                                              (s.startMeasureNumber <= end && s.endMeasureNumber >= end));
                foreach (var key in sl)
                {
                    var noteWithinKey = notes.Where(n => n.measureNumber <= key.endMeasureNumber && n.measureNumber >= key.startMeasureNumber);
                    GetNoteOutOfKeyNumber(key, noteWithinKey);
                }
                var allNotes = songNotes.SelectMany(n => n.notes).Where(n => n.measureNumber >= start && n.measureNumber <= end);
                PercentageOutOfKey = outOfKeyNotes.Sum(n => n.duration) / allNotes.Sum(n => n.duration);
            }
        }

        public void DurationAnalysis(int start = -1, int end = -1)
        {
            var noteCount = songNotes.Count();
            var notes = songNotes.SelectMany(s => s.notes);

            if(start != -1 && end != -1)
            {
                notes = notes.Where(n => n.measureNumber >= start && n.measureNumber <= end);
            }

            var durationList = notes.Select(s => s.duration).ToList();

            if (!durationList.Any())
            {
                durationVariance = 0;
                return;
            }

            //归一化
            var max = durationList.Max();
            var adjustedDurationList = new List<double>();
            foreach (var d in durationList)
            {
                adjustedDurationList.Add(d / max);
            }

            durationVariance = adjustedDurationList.Variance();
        }    
        
        public void OrnamentAnalysis(int start = -1, int end = -1)
        {
            var notes = songNotes.SelectMany(s => s.notes);

            if (start != -1 && end != -1)
            {
                notes = notes.Where(n => n.measureNumber >= start && n.measureNumber <= end);
            }
            NumOfMordent = notes.Count(n => n.isMordent == true);
            NumOfTrill = notes.Count(n => n.isTrill == true);
            NumOfTurn = notes.Count(n => n.isTurn == true);
        }       
        
        private void GetNoteOutOfKeyNumber(Scale key, IEnumerable<Note> notes)
        {
            foreach(var note in notes)
            {
                if (!key.InKey(note))
                {
                    outOfKeyNotes.Add(note);
                }
            }
        }

        private double GetNumberOfIntervals(ConsonanceCatagories cc, int start, int end)
        {
            double ret = 0;
            var partIntervalList = new List<Interval>();
            if (start != -1 && end != -1)
            {
                partIntervalList = intervalList.Where(i => i.measureNumber >= start && i.measureNumber <= end).ToList();
            }
            else
            {
                partIntervalList = intervalList;
            }

            foreach (var interval in partIntervalList)
            {
                if (interval.consonanceCategory == cc)
                {
                    ret += interval.weight;
                }
            }
            return ret;
        }

        public string PrintResultToCsv()
        {
            var type = typeof(Song);
            var resultValues = new List<string>();
            var properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                resultValues.Add(property.GetValue(this).ToString());
            }
            return string.Join(",", resultValues);
        }
    }

    public class Result
    {
        public double position;
        public string information;
        public List<Note> notes;

        public Result(double p, string i, List<Note> n)
        {
            position = p;
            information = i;
            notes = n;
        }
    }
}
