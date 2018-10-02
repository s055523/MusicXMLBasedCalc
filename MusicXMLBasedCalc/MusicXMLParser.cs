using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace MusicXMLBasedCalc
{
    public static class MusicXMLParser
    {
        public static List<Result> results;
        public static double durationInMeasure;

        public static List<Result> Parser(string filename, int start = -1, int end = -1)
        {
            //Guard clause
            if (!File.Exists(filename))
            {
                throw new Exception($"File {filename} not found!");
            }
            Console.WriteLine(filename);
            XElement rootNode = XElement.Load(filename);

            //Find part node, it contains all measures
            IEnumerable<XElement> parts = from target in rootNode.Descendants("part")
                                          select target;

            results = new List<Result>();

            //节拍号在第一个小节
            var firstMeasure = parts.First().Descendants("measure").First();

            //division标志，只有一个
            //所有的duration都要除这个数
            double division = double.Parse(firstMeasure.Descendants("divisions").First().Value);

            double currDurationInMeasure = GetDurationInMeasure(firstMeasure.Descendants("divisions").First());
            double totalDuration = 0;
           
            foreach (var part in parts)
            {
                var measures = part.Descendants("measure");

                if (start != -1) measures = measures.Skip(start);

                //获得所有调和他们的范围
                var keyList = GetKeys(part.Descendants("measure"), start, end);

                foreach (var measure in measures)
                {
                    double prevDefaultx = 0;
                    double prevNoteDuration = 0;

                    var measureNum = measure.Attribute("number").Value;

                    //现代音乐莫名其妙的含有小节为X1的谱
                    if (measureNum.Contains("X")) continue;

                    var measureNumber = int.Parse(measureNum);

                    if (end != -1 && measureNumber > end) break;

                    //每小节有几拍
                    durationInMeasure = GetDurationInMeasure(measure);

                    //没有写节拍号，意味着节拍没有变
                    if (durationInMeasure == -1)
                    {
                        durationInMeasure = currDurationInMeasure;
                    }
                    //更新节拍
                    else if(durationInMeasure != currDurationInMeasure)
                    {
                        currDurationInMeasure = durationInMeasure;
                    }

                    //小节中的位置
                    double currentPos = totalDuration;

                    //小节中所有临时记号字典
                    var accidentalDic = new List<Accidental>();

                    var elements = measure.Elements();
                    for (int i = 0; i < elements.Count(); i++)
                    {
                        if (elements.ElementAt(i).Name == "note")
                        {                           
                            //解析出音的音高和时值
                            var parsedNote = ParseNote(elements.ElementAt(i), division, measureNumber);

                            //倚音的情况
                            if (parsedNote == null) continue;

                            //休止符的情况
                            if (parsedNote.pitch == "rest")
                            {
                                currentPos += parsedNote.duration;
                                continue;
                            }

                            //获得x轴位置
                            var defaultx = double.Parse(elements.ElementAt(i).Attribute("default-x").Value);
                        
                            //调性修正(即没有任何临时记号的音需要按照调性去修正，例如G大调所有的F都是升的)
                            var currKey = keyList.First(k => k.startMeasureNumber <= measureNumber && k.endMeasureNumber >= measureNumber);

                            //现在所在的调含有的音
                            //一般来说，没有临时记号的音才有可能考虑调性修正
                            if (parsedNote.accidental == string.Empty)
                            {
                                parsedNote.pitch = currKey.KeyFix(parsedNote);
                            }

                            //这个音没有被临时记号修饰，考虑临时记号修正                            
                            if (parsedNote.accidental == string.Empty)
                            {
                                //如果之前有临时记号在同音高，则需要做修正
                                //一般来说，如果这个音被调性修正了，那么它通常不会再被临时记号修饰
                                var accidentalWithSamePitch = accidentalDic.Where(a => a.pitch == parsedNote.pitch);
                                if (accidentalWithSamePitch.Any())
                                {
                                    var accs = accidentalWithSamePitch.Select(a => a.acc);
                                    
                                    //是按顺序加入的，因此如果最后是natural，则抵消前面的效果
                                    //如果最后一个不是，则本音需要继承这个临时记号
                                    if (accs.Last() != "natural")
                                    {
                                        switch (accs.Last())
                                        {
                                            case "sharp":
                                                parsedNote.pitch = NoteHelper.GetNote(parsedNote.pitch, 1);
                                                break;
                                            case "sharp-sharp":
                                                parsedNote.pitch = NoteHelper.GetNote(parsedNote.pitch, 2);
                                                break;
                                            case "flat":
                                                parsedNote.pitch = NoteHelper.GetNote(parsedNote.pitch, -1);
                                                break;
                                            case "flat-flat":
                                                parsedNote.pitch = NoteHelper.GetNote(parsedNote.pitch, -2);
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //加入到临时记号字典中
                                var acc = new Accidental()
                                {
                                    acc = parsedNote.accidental,
                                    pitch = parsedNote.GetOriginalPitchForAccidental()
                                };
                                accidentalDic.Add(acc);
                            }
                        
                            //不是休止符
                            if (parsedNote != null && parsedNote.pitch != "rest")
                            {
                                //这表示这个音和上一个音音高相同(只要tie就可以了，不用再判断音高)，并且用连线连接了起来
                                if (parsedNote.slurStatus == SlurStatus.stop && results.Any() && results.Last().notes.First().slurStatus == SlurStatus.start)
                                {
                                    Console.WriteLine("侦测到同音高连线。音高：" + results.Last().notes.First().pitch + "，小节：" + measureNumber);

                                    //上一个同音高的音
                                    var lastNote = results.Last().notes.Where(n => n.pitch == parsedNote.pitch).FirstOrDefault();
                                    if (lastNote == null) continue;

                                    //更新它的时值
                                    lastNote.duration += parsedNote.duration;
                                    prevNoteDuration += parsedNote.duration;

                                    //更新information的值（从上一个音，可能是一个和弦中找出同音高的音，更新它的时值）
                                    var notes = results.Last().notes;
                                    results.Last().information = string.Join(",", notes.Select(n => n.ToString()));
                                }
                                else
                                {
                                    //如果是和弦那么currentPos不变，否则就前进
                                    if (defaultx > prevDefaultx && prevDefaultx != 0 && prevNoteDuration != 0)
                                    {
                                        //defaultx > prevDefault说明和上一个音的位置不同（这里多声部不对齐就暂时没办法，只能改谱子）
                                        //currentPos表示现在音符开始时所在节拍的位置
                                        //如果prevDefaultx为0说明为小节内第一个音符，所以就不需要加当前音符的时值
                                        currentPos += prevNoteDuration;
                                    }

                                    results.Add(new Result(currentPos, parsedNote.ToString(), new List<Note> { parsedNote }));
                                    prevDefaultx = defaultx;
                                    prevNoteDuration = parsedNote.duration;
                                }
                            }
                        }

                        //换手了，位置归零
                        if (elements.ElementAt(i).Name == "backup")
                        {
                            currentPos = totalDuration;
                            prevDefaultx = 0;
                            prevNoteDuration = 0;
                        }
                    }
                    //总节拍数增加
                    totalDuration += currDurationInMeasure;
                }
            }
            return MergeResults(results);
        }

        /// <summary>
        /// 在一定范围之内搜索调号
        /// </summary>
        /// <param name="measures">全曲所有的小节</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static List<Key> GetKeys(IEnumerable<XElement> measures, int start = -1, int end = -1)
        {
            var keyList = new List<Key>();
            var numOfMeasures = measures.Count();

            //全曲搜
            foreach (var measure in measures)
            {
                var keyNode = measure.Descendants("key").FirstOrDefault();
                if (keyNode != null)
                {
                    var measureNumber = measure.Attribute("number").Value;
                    var keyMode = string.Empty;
                    var fifth = keyNode.Descendants("fifths").First();
                    var mode = keyNode.Descendants("mode").FirstOrDefault();
                    if (mode == null)
                    {
                        keyMode = "major";
                    }
                    else
                    {
                        keyMode = mode.Value;
                    }

                    //新的调
                    var key = new Key(keyMode, int.Parse(fifth.Value));
                    Console.WriteLine($"新的调，为{key.baseNoteName.Substring(0, key.baseNoteName.Length - 1)} {key.mode}，从第{measureNumber}小节开始。");
                    key.startMeasureNumber = int.Parse(measureNumber);
                    keyList.Add(key);
                }
            }

            //为调的终止小节赋值
            for (int i = 0; i < keyList.Count; i++)
            {
                if (i + 1 == keyList.Count)
                {
                    keyList[i].endMeasureNumber = numOfMeasures;
                }
                else
                {
                    keyList[i].endMeasureNumber = keyList[i + 1].startMeasureNumber - 1;
                }
            }

            if (start == -1 && end == -1) return keyList;

            var fragmentKeyList = new List<Key>();
            foreach(var key in keyList)
            {
                if (key.endMeasureNumber >= start && key.startMeasureNumber <= end)
                {
                    fragmentKeyList.Add(key);
                }
            }

            return fragmentKeyList;
        }

        /// <summary>
        /// 这个小节一共有多少拍
        /// </summary>
        /// <param name="measure"></param>
        /// <returns></returns>
        private static double GetDurationInMeasure(XElement measure)
        {
            var beats = measure.Descendants("beats").FirstOrDefault();

            //当前小节的节拍号和之前一样
            if (beats == null)
            {
                return -1;
            }
            var beatsValue = double.Parse(beats.Value);

            var beatType = measure.Descendants("beat-type").FirstOrDefault();
            if (beatType == null)
            {
                return -1;
            }
            var beatTypeValue = double.Parse(beatType.Value);

            //如果beats=3,beat-type=4，返回3
            return 4 / beatTypeValue * beatsValue;
        }

        private static Note ParseNote(XElement note, double division, int measureNumber)
        {
            //忽略倚音
            var isGrace = note.Descendants("grace");
            if (isGrace.Any())
            {
                return null;
            }

            var position = note.Attribute("default-x");
            
            var duration = double.Parse(note.Descendants("duration").First().Value);
            duration /= division;

            //it is a rest
            if (position == null)
            {
                return new Note("rest", duration, 0, -1, measureNumber, string.Empty);
            }

            var step = note.Descendants("step").First().Value;
            var octave = note.Descendants("octave").First().Value;
            var pitch = step + octave;
            var pitchPos = NoteHelper.noteDic.Where(n => n.name == pitch).First().id;

            //if this note has accidental
            var acc = string.Empty;
            var accidentalNode = note.Descendants("accidental");
            if (accidentalNode.Any())
            {
                acc = accidentalNode.First().Value;
                switch (acc)
                {
                    case "sharp":
                        pitchPos++;
                        break;
                    case "sharp-sharp":
                        pitchPos += 2;
                        break;
                    case "flat":
                        pitchPos--;
                        break;
                    case "flat-flat":
                        pitchPos -= 2;
                        break;
                    default:
                        break;
                }
            }
            
            var realNoteAfterAccidental = NoteHelper.GetNoteByPosition(pitchPos);

            //探测同音高连线，要将这两个音归并为一个，否则会多出了一个纯一度
            var tieNode = note.Descendants("tie").FirstOrDefault();     
            
            //普通的情况
            if (tieNode == null)
            {
                Note n = new Note(realNoteAfterAccidental, duration, double.Parse(position.Value), pitchPos, measureNumber, acc);
                if (note.Descendants("mordent").Any() || note.Descendants("inverted-mordent").Any()
                    ||note.Descendants("schleifer").Any())
                {
                    n.isMordent = true;
                }
                if (note.Descendants("turn").Any())
                {
                    n.isTurn = true;
                }
                if (note.Descendants("trill-mark").Any())
                {
                    n.isTrill = true;
                }
                return n;
            }
            //这和上面的音是同一个音
            else
            {
                var slurStatus = tieNode.Attribute("type").Value;
                return new Note(realNoteAfterAccidental, duration, double.Parse(position.Value), pitchPos, measureNumber, acc, slurStatus);
            }
        }

        private static List<Result> MergeResults(List<Result> results)
        {
            var mergedResults = new List<Result>();
            var groups = results.OrderBy(r => r.position).GroupBy(r => r.position);

            foreach(var g in groups)
            {
                var key = g.Key;
                var notes = results.Where(r => r.position == key);
                var information = string.Join(",", notes.Select(n => n.information));
                mergedResults.Add(new Result(key, information, notes.SelectMany(n => n.notes).ToList()));
            }
            return mergedResults;
        }

        public static int GetNumOfMeasure(string filename)
        {
            //Guard clause
            if (!File.Exists(filename))
            {
                throw new Exception($"File {filename} not found!");
            }
            Console.WriteLine(filename);
            XElement rootNode = XElement.Load(filename);

            //Find part node, it contains all measures
            IEnumerable<XElement> measures = from target in rootNode.Descendants("measure")
                                          select target;
            return measures.Count();
        }
    }

    public class NoteDic
    {
        public int id;
        public string name;
        public NoteDic(int i, string n)
        {
            id = i;
            name = n;
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

    public class Accidental
    {
        public string acc;
        public string pitch;
    }
}

