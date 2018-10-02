using System;
using System.Collections.Generic;
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

        //这首曲子所有的音符
        public List<Result> songNotes;
        public List<Note> outOfKeyNotes;

        //这首曲子所有的音程
        public List<Interval> intervalList;

        public List<Key> keyList;

        public int numOfMeasures;

        //和弦的音程权重
        private const double CHORD_INTERVAL_WEIGHT = 3;

        public bool skip = false;

        //维度
        public double PercentageIntervals1 { get; set; }
        public double PercentageIntervals2 { get; set; }
        public double PercentageIntervals3 { get; set; }
        public double PercentageIntervals4 { get; set; }
        public double PercentageIntervals5 { get; set; }
        public double PercentageIntervals6 { get; set; }

        public double ConsonanceMean { get; set; }

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
            songNotes = MusicXMLParser.Parser(fileName, start, end);
        }

        public void SongAnalysis(int start = -1, int end = -1)
        {
            XElement rootNode = XElement.Load(fileName);

            IntervalAnalysis();

            KeyAnalysis(rootNode, start, end);
            OrnamentAnalysis(start, end);

            var consonanceArray = new List<double>();

            var totalIntervalSum = intervalList.Sum(i => i.weight);

            if(totalIntervalSum == 0)
            {
                skip = true;
            }

            PercentageIntervals1 = Math.Round(GetNumberOfIntervals(ConsonanceCatagories.perfectConsonance) / totalIntervalSum, 3);
            consonanceArray.Add(PercentageIntervals1);
            PercentageIntervals2 = Math.Round(GetNumberOfIntervals(ConsonanceCatagories.medianConsonance) / totalIntervalSum, 3);
            consonanceArray.Add(PercentageIntervals2);
            PercentageIntervals3 = Math.Round(GetNumberOfIntervals(ConsonanceCatagories.imperfectConsonance) / totalIntervalSum, 3);
            consonanceArray.Add(PercentageIntervals3);
            PercentageIntervals4 = Math.Round(GetNumberOfIntervals(ConsonanceCatagories.imperfectDissonance) / totalIntervalSum, 3);
            consonanceArray.Add(PercentageIntervals4);
            PercentageIntervals5 = Math.Round(GetNumberOfIntervals(ConsonanceCatagories.medianDissonance) / totalIntervalSum, 3);
            consonanceArray.Add(PercentageIntervals5);
            PercentageIntervals6 = Math.Round(GetNumberOfIntervals(ConsonanceCatagories.perfectDisonnace) / totalIntervalSum, 3);
            consonanceArray.Add(PercentageIntervals6);

            ConsonanceMean = consonanceArray.Average();
            ConsonanceIntervalVariance = consonanceArray.Variance();

            DurationAnalysis();
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
                            //currentLineNotes[j].Play();
                            //currentLineNotes[k].Play();
                            intervalList.Add(new Interval(currentLineNotes[j], currentLineNotes[k], CHORD_INTERVAL_WEIGHT));
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
                        //currentLineNotes[j].Play();
                        //lastLineNotes[k].Play();
                        intervalList.Add(new Interval(currentLineNotes[j], lastLineNotes[k], 1));
                    }
                }
            }
        }

        public void KeyAnalysis(XElement rootNode, int start = -1, int end = -1)
        {          
            IEnumerable<XElement> measures = from target in rootNode.Descendants("measure") select target;

            keyList = MusicXMLParser.GetKeys(measures, start, end);

            outOfKeyNotes = new List<Note>();

            //统计调外音的数目
            foreach (var key in keyList)
            {
                var notes = songNotes.SelectMany(n => n.notes);
                var noteWithinKey = notes.Where(n => n.measureNumber < key.endMeasureNumber && n.measureNumber >= key.startMeasureNumber);
                GetNoteOutOfKeyNumber(key, noteWithinKey);
            }

            var allNotes = songNotes.SelectMany(n => n.notes);
            PercentageOutOfKey = outOfKeyNotes.Sum(n => n.duration) / allNotes.Sum(n => n.duration);
            //keyNum = keyList.Count();

            Console.WriteLine("调外音数目：" + outOfKeyNotes.Count());
            Console.WriteLine("比例：" + PercentageOutOfKey);
        }

        public void DurationAnalysis()
        {
            var noteCount = songNotes.Count();
            var notes = songNotes.SelectMany(s => s.notes);

            var durationList = notes.Select(s => s.duration).ToList();
            durationVariance = durationList.Variance();
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
        
        private void GetNoteOutOfKeyNumber(Key key, IEnumerable<Note> notes)
        {
            foreach(var note in notes)
            {
                if (!key.InKey(note))
                {
                    outOfKeyNotes.Add(note);
                }
            }
        }

        private double GetNumberOfIntervals(ConsonanceCatagories cc)
        {
            double ret = 0;
            foreach (var interval in intervalList)
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
}
