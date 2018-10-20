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
        public static ChordThree BuildThree(Note n, ChordThreeCategory ctc)
        {
            var c = new ChordThree();
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

        /// <summary>
        /// 输入N个音，输出其中和谐音程所占的比例
        /// </summary>
        /// <returns></returns>
        public static double DegreeOfConsonance(List<Note> notes, ChordThree ct)
        {
            notes.AddRange(ct.notes);
            var intervalList = new List<Interval>();
            var count = notes.Count;
            for(int i=0; i< count-1; i++)
            {
                for(int j = i + 1; j < count; j++)
                {
                    var interval = new Interval(notes[i], notes[j]);
                    intervalList.Add(interval);
                }
            }
            var consonanceIntervals = intervalList.Where(il => il.consonanceCategory == ConsonanceCatagories.perfectConsonance ||
            il.consonanceCategory == ConsonanceCatagories.imperfectConsonance ||
            il.consonanceCategory == ConsonanceCatagories.medianConsonance ||
            il.consonanceCategory == ConsonanceCatagories.imperfectDissonance);

            return (double)consonanceIntervals.Count() / (double)intervalList.Count;
        }
    }
}
