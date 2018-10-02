using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicXMLBasedCalc
{
    public class Key
    {
        //升降号的数目
        public int fifth { get; set; }

        //大调还是小调
        public string mode { get; set; }

        //调的主音
        public string baseNoteName { get; set; }

        //调中包括哪些音
        public List<string> scaleNotes { get; set; }

        public int startMeasureNumber { get; set; }

        public int endMeasureNumber { get; set; }

        public Key(string m, int f)
        {
            scaleNotes = new List<string>();
            mode = m;
            fifth = f;

            //通过五度关系定出主音位置
            var numOfSemitone = 7;

            //通过音程关系定出所有音
            if (mode == "major" || mode == string.Empty)
            {
                baseNoteName = "C4";
                baseNoteName = NoteHelper.GetNote(baseNoteName, numOfSemitone * fifth);
                scaleNotes.Add(baseNoteName.Substring(0, baseNoteName.Length - 1));

                if (baseNoteName.Last() == '7') baseNoteName = baseNoteName.Replace('7', '4');
                if (baseNoteName.Last() == '8') baseNoteName = baseNoteName.Replace('8', '4');
                AddMajorKeys(baseNoteName);

                //现在scaleNotes应该有7个音
                //为了handle关系小调和大调的转换（此时谱子无需显式写出），需要把小调也加入进去
                //小调大部分音和大调相同，除了最后一个音大调没有，是大调主音上面减五度
                var extraNoteFromMinor = NoteHelper.GetNote(baseNoteName, 8);
                scaleNotes.Add(extraNoteFromMinor.Substring(0, extraNoteFromMinor.Length - 1));
            }
            else
            {
                baseNoteName = "A4";
                baseNoteName = NoteHelper.GetNote(baseNoteName, numOfSemitone * fifth);
                scaleNotes.Add(baseNoteName.Substring(0, baseNoteName.Length - 1));
                AddMinorKeys(baseNoteName);

                //大调大部分音和小调相同，除了小调主音上面的大七度
                var extraNoteFromMajor = NoteHelper.GetNote(baseNoteName, 10);
                scaleNotes.Add(extraNoteFromMajor.Substring(0, extraNoteFromMajor.Length - 1));
            }
        }

        private void AddMajorKeys(string baseNoteName)
        {
            //大调：全全半全全全半
            var nextNoteName = baseNoteName;
            nextNoteName = NoteHelper.GetNote(nextNoteName, 2);
            scaleNotes.Add(nextNoteName.Substring(0, nextNoteName.Length - 1));
            nextNoteName = NoteHelper.GetNote(nextNoteName, 2);
            scaleNotes.Add(nextNoteName.Substring(0, nextNoteName.Length - 1));
            nextNoteName = NoteHelper.GetNote(nextNoteName, 1);
            scaleNotes.Add(nextNoteName.Substring(0, nextNoteName.Length - 1));
            nextNoteName = NoteHelper.GetNote(nextNoteName, 2);
            scaleNotes.Add(nextNoteName.Substring(0, nextNoteName.Length - 1));
            nextNoteName = NoteHelper.GetNote(nextNoteName, 2);
            scaleNotes.Add(nextNoteName.Substring(0, nextNoteName.Length - 1));
            nextNoteName = NoteHelper.GetNote(nextNoteName, 2);
            scaleNotes.Add(nextNoteName.Substring(0, nextNoteName.Length - 1));
        }

        private void AddMinorKeys(string baseNoteName)
        {
            //和声小调
            var nextNoteName = baseNoteName;
            nextNoteName = NoteHelper.GetNote(nextNoteName, 2);
            scaleNotes.Add(nextNoteName.Substring(0, nextNoteName.Length - 1));
            nextNoteName = NoteHelper.GetNote(nextNoteName, 1);
            scaleNotes.Add(nextNoteName.Substring(0, nextNoteName.Length - 1));
            nextNoteName = NoteHelper.GetNote(nextNoteName, 2);
            scaleNotes.Add(nextNoteName.Substring(0, nextNoteName.Length - 1));
            nextNoteName = NoteHelper.GetNote(nextNoteName, 2);
            scaleNotes.Add(nextNoteName.Substring(0, nextNoteName.Length - 1));
            nextNoteName = NoteHelper.GetNote(nextNoteName, 1);
            scaleNotes.Add(nextNoteName.Substring(0, nextNoteName.Length - 1));
            nextNoteName = NoteHelper.GetNote(nextNoteName, 3);
            scaleNotes.Add(nextNoteName.Substring(0, nextNoteName.Length - 1));
        }

        public bool InKey(Note note)
        {
            //得到唱名，忽略音高
            var name = note.pitch.Substring(0, note.pitch.Length - 1);
            return scaleNotes.Contains(name);
        }

        public string KeyFix(Note note)
        {
            if (fifth == 0) return note.pitch;
            int notePitchHeight = int.Parse(note.pitch.Last().ToString());

            if (note.pitch.Contains("C") && !note.pitch.Contains("C#"))
            {
                if (fifth >= 2)
                {
                    return "C#" + notePitchHeight;
                }
                if(fifth <= -6)
                {
                    notePitchHeight--;
                    return "B" + notePitchHeight;
                }
            }
            if (note.pitch.Contains("D") && !note.pitch.Contains("D#"))
            {
                if (fifth >= 4)
                {
                    return "D#" + notePitchHeight;
                }
                if (fifth <= -4)
                {
                    return "C#" + notePitchHeight;
                }
            }
            if (note.pitch.Contains("E"))
            {
                if (fifth >= 6)
                {
                    return "F" + notePitchHeight;
                }
                if (fifth <= -2)
                {
                    return "D#" + notePitchHeight;
                }
            }
            if (note.pitch.Contains("F") && !note.pitch.Contains("F#"))
            {
                if (fifth >= 1)
                {
                    return "F#" + notePitchHeight;
                }
                if (fifth <= -7)
                {
                    return "E" + notePitchHeight;
                }
            }
            if (note.pitch.Contains("G") && !note.pitch.Contains("G#"))
            {
                if (fifth >= 3)
                {
                    return "G#" + notePitchHeight;
                }
                if (fifth <= -5)
                {
                    return "F#" + notePitchHeight;
                }
            }
            if (note.pitch.Contains("A") && !note.pitch.Contains("A#"))
            {
                if (fifth >= 5)
                {
                    return "A#" + notePitchHeight;
                }
                if (fifth <= -3)
                {
                    return "G#" + notePitchHeight;
                }
            }
            if (note.pitch.Contains("B"))
            {
                if (fifth >= 7)
                {
                    notePitchHeight++;
                    return "C" + notePitchHeight;
                }
                if (fifth <= -1)
                {
                    return "A#" + notePitchHeight;
                }
            }
            return note.pitch;
        }
    }
}
