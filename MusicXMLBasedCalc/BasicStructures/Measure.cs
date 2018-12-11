using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MusicXMLBasedCalc.BasicStructures
{
    public class Measure
    {
        //如果没有解析到任何东西（例如小节数为x1)
        public bool illegal { get; set; }
        public int number { get; set; }
        public Scale scale { get; set; }
        public List<Note> notes { get; set; }
        public List<Result> noteResults { get; set; }
        public XElement xml { get; set; }
        public double totalDuration { get; set; }
        public List<Accidental> accidentals { get; set; }

        public Measure()
        {
            notes = new List<Note>();
            noteResults = new List<Result>();
        }

        /// <summary>
        /// 解析出一个小节中所有有用的信息,包括音符,调等
        /// </summary>
        /// <param name="measureXML"></param>
        public void Parse(List<Scale> songScaleList, XElement measureXML, double division)
        {
            var ret = new Measure
            {
                xml = measureXML
            };

            //小节数
            var measureNum = measureXML.Attribute("number").Value;

            //现代音乐莫名其妙的含有小节为X1的谱
            if (measureNum.Contains("X"))
            {
                illegal = true;
                return;
            }

            number = int.Parse(measureNum);

            //小节中的位置
            double currentPos = totalDuration;

            //小节中所有临时记号字典
            accidentals = new List<Accidental>();

            var elements = measureXML.Elements();
            double prevDefaultx = 0;
            double prevNoteDuration = 0;

            //开始逐个进行解析
            for (int i = 0; i < elements.Count(); i++)
            {
                if (elements.ElementAt(i).Name == "note")
                {
                    //解析出音的音高和时值
                    var parsedNote = NoteHelper.ParseNote(elements.ElementAt(i), number, division);

                    //倚音的情况
                    if (parsedNote == null) continue;

                    //休止符的情况
                    if (parsedNote.pitch == "rest")
                    {
                        currentPos += parsedNote.duration;
                        continue;
                    }

                    //x轴位置
                    var defaultx = parsedNote.position;

                    //这表示这个音和上一个音音高相同(只要tie就可以了，不用再判断音高)，并且用连线连接了起来
                    if (parsedNote.tieStatus == tieStatus.stop && notes.Any() && notes.Last().tieStatus == tieStatus.start)
                    {
                        Console.WriteLine("侦测到同音高连线。音高：" + notes.Last().pitch + "，小节：" + number);

                        //上一个同音高的音
                        var lastNote = notes.Last();
                        if (lastNote == null) continue;

                        //更新它的时值
                        lastNote.duration += parsedNote.duration;
                        prevNoteDuration += parsedNote.duration;

                        //更新information的值（从上一个音，可能是一个和弦中找出同音高的音，更新它的时值）
                        noteResults.Last().information = string.Join(",", notes.Select(n => n.ToString()));
                    }
                    else
                    {
                        //判別和弦的方法： <chord/>
                        var containsChordNode = elements.ElementAt(i).Descendants("chord").Any();
                        if (containsChordNode == false)
                        {
                            //如果是和弦那么currentPos不变，否则就前进
                            if (defaultx > prevDefaultx && prevDefaultx != 0 && prevNoteDuration != 0)
                            {
                                //defaultx > prevDefault说明和上一个音的位置不同（这里多声部不对齐就暂时没办法，只能改谱子）
                                //currentPos表示现在音符开始时所在节拍的位置
                                //如果prevDefaultx为0说明为小节内第一个音符，所以就不需要加当前音符的时值
                                currentPos += prevNoteDuration;
                            }
                        }

                        notes.Add(parsedNote);
                        noteResults.Add(new Result(currentPos, parsedNote.ToString(), new List<Note> { parsedNote }));

                        prevDefaultx = defaultx;
                        prevNoteDuration = parsedNote.duration;
                    }
                }

                //换声部了，位置归零，声部数加一
                if (elements.ElementAt(i).Name == "backup")
                {
                    currentPos = totalDuration;
                    prevDefaultx = 0;
                    prevNoteDuration = 0;
                }
            }            
        }

        /// <summary>
        /// 这个小节一共有多少拍
        /// </summary>
        /// <param name="measureXML"></param>
        /// <returns></returns>
        public static double GetDurationInMeasure(XElement measureXML)
        {
            var beats = measureXML.Descendants("beats").FirstOrDefault();

            //当前小节的节拍号和之前一样
            if (beats == null)
            {
                return -1;
            }
            var beatsValue = double.Parse(beats.Value);

            var beatType = measureXML.Descendants("beat-type").FirstOrDefault();
            if (beatType == null)
            {
                return -1;
            }
            var beatTypeValue = double.Parse(beatType.Value);

            //如果beats=3,beat-type=4，返回3
            return 4 / beatTypeValue * beatsValue;
        }
    }

    public class Accidental
    {
        public Accidental()
        {
        }

        public string acc { get; set; }
        public string pitch { get; set; }
    }
}
