using System.Collections.Generic;
using System.Linq;

namespace MusicXMLBasedCalc.BasicStructures
{
    public static class ChordHelper
    {
        /// <summary>
        /// 建立基于根音n的和弦
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static Chord BuildThree(Note n, ChordThreeCategory ctc)
        {
            var c = new Chord();
            c.notes = new List<Note>();
            c.notes.Add(n);
            switch (ctc)
            {
                case ChordThreeCategory.major:
                    c.name = "major3";
                    c.notes.Add(new Note(NoteHelper.GetNote(n.pitch, 4)));
                    c.notes.Add(new Note(NoteHelper.GetNote(n.pitch, 7)));
                    return c;
                case ChordThreeCategory.minor:
                    c.name = "minor3";
                    c.notes.Add(new Note(NoteHelper.GetNote(n.pitch, 3)));
                    c.notes.Add(new Note(NoteHelper.GetNote(n.pitch, 7)));
                    return c;
                case ChordThreeCategory.augment:
                    c.name = "augment3";
                    c.notes.Add(new Note(NoteHelper.GetNote(n.pitch, 4)));
                    c.notes.Add(new Note(NoteHelper.GetNote(n.pitch, 8)));
                    return c;
                case ChordThreeCategory.diminished:
                    c.name = "diminished3";
                    c.notes.Add(new Note(NoteHelper.GetNote(n.pitch, 3)));
                    c.notes.Add(new Note(NoteHelper.GetNote(n.pitch, 6)));
                    return c;
                default:
                    return c;
            }
        }

        public static Chord BuildSeven(Note n)
        {
            var c = new Chord();
            c.notes = new List<Note>();
            c.notes.Add(n);
            c.notes.Add(new Note(NoteHelper.GetNote(n.pitch, 4)));
            c.notes.Add(new Note(NoteHelper.GetNote(n.pitch, 7)));
            c.notes.Add(new Note(NoteHelper.GetNote(n.pitch, 10)));
            return c;
        }

        /// <summary>
        /// 输入N个音，输出其中和谐音程所占的比例
        /// </summary>
        /// <returns></returns>
        public static double DegreeOfConsonance(List<Note> notes, Chord ct)
        {
            var intervalList = new List<Interval>();
            var count = notes.Count;

            //小节内本身的音符
            for(int i = 0; i < count; i++)
            {
                if (notes[i].duration > 1)
                {
                    notes[i].duration = 1;
                }

                //和弦本身的音符
                foreach(var cn in ct.notes)
                {
                    var interval = new Interval(notes[i], cn);
                    intervalList.Add(interval);
                }
            }
            var conWeightCount = intervalList.Where(il => il.consonanceCategory == ConsonanceCatagories.perfectConsonance ||
            il.consonanceCategory == ConsonanceCatagories.imperfectConsonance ||
            il.consonanceCategory == ConsonanceCatagories.medianConsonance ||
            il.consonanceCategory == ConsonanceCatagories.imperfectDissonance).Sum(i => i.weight);

            double intervalWeightCount = intervalList.Sum(i => i.weight);

            //return (double)consonanceIntervals.Count() / (double)intervalList.Count;
            if (intervalWeightCount == 0) return 0;

            return conWeightCount / intervalWeightCount;
        }

        public static List<string> ChordAnalysis(int start, int end, List<Result> songNotes, List<Scale> scaleList, 
            double division, int numOfMeasure)
        {
            var ret = new List<string>();
            if (start == -1) start = 1;
            if (end == -1) end = numOfMeasure;

            for (int i = start; i <= end; i++)
            {
                //当前小节所在的调性
                var currentScale = scaleList.First(s => s.startMeasureNumber <= i && s.endMeasureNumber >= i);
                var firstNote = currentScale.baseNoteName;

                for (int j = 0; j < division; j++)
                {
                    var allChordCon = new List<ChordDegreeOfConsonance>();
                    var partNotes = songNotes.Where(s => s.position >= j && s.position < j + 1).SelectMany(s => s.notes);
                    var notes = partNotes.Where(s => s.measureNumber == i);

                    var result = "小节" + i + ", 第" + j + "拍:";

                    //音符时值不够多
                    if (!notes.Any() || notes.Sum(n => n.duration) <= 1)
                    {
                        //ret.Add(result + "rest");
                        continue;
                    }

                    //T
                    var d = ChordHelper.DegreeOfConsonance(notes.ToList(), currentScale.main);
                    allChordCon.Add(new ChordDegreeOfConsonance("T", d));

                    //D
                    d = ChordHelper.DegreeOfConsonance(notes.ToList(), currentScale.dominant);
                    allChordCon.Add(new ChordDegreeOfConsonance("V", d));

                    //S
                    d = ChordHelper.DegreeOfConsonance(notes.ToList(), currentScale.subDominant);
                    allChordCon.Add(new ChordDegreeOfConsonance("IV", d));

                    d = ChordHelper.DegreeOfConsonance(notes.ToList(), currentScale.second);
                    allChordCon.Add(new ChordDegreeOfConsonance("II", d));

                    d = ChordHelper.DegreeOfConsonance(notes.ToList(), currentScale.sixth);
                    allChordCon.Add(new ChordDegreeOfConsonance("VI", d));

                    d = ChordHelper.DegreeOfConsonance(notes.ToList(), currentScale.third);
                    allChordCon.Add(new ChordDegreeOfConsonance("III", d));

                    if (allChordCon.Where(a => a.degreeOfConsonance > 0.8).Count() == 0)
                    {
                        //var lastChord = ret.Last().Split(':')[1];
                        //ret.Add(result + lastChord);
                        ret.Add(result + "none");
                    }
                    else
                    {
                        allChordCon.Sort((a, b) => a.degreeOfConsonance.CompareTo(b.degreeOfConsonance));

                        //特别规则1: 当II和IV冲突时优先选择IV
                        if (allChordCon.Last().chordName == "II" &&
                            allChordCon.First(a => a.chordName == "IV").degreeOfConsonance == allChordCon.Last().degreeOfConsonance)
                        {
                            ret.Add(result + "IV");
                        }
                        else
                        {
                            ret.Add(result + allChordCon.Last().chordName);
                        }
                    }
                }
            }
            return ret;
        }
    }

    public class ChordDegreeOfConsonance
    {
        public string chordName;
        public double degreeOfConsonance;
        public ChordDegreeOfConsonance(string c, double d)
        {
            chordName = c; degreeOfConsonance = d;
        }
    }
}
