using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicXMLBasedCalc
{
    public class Scale
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

        public Scale(string m, int f)
        {
            scaleNotes = new List<string>();
            mode = m;
            fifth = f;

            //通过五度关系定出主音位置
            //纯五度有7个半音
            var numOfSemitone = 7;

            //通过音程关系定出所有音
            baseNoteName = "C4";
            baseNoteName = NoteHelper.GetNote(baseNoteName, numOfSemitone * fifth);
            scaleNotes.Add(baseNoteName.Substring(0, baseNoteName.Length - 1));

            //如果超出了C0-B8范围
            if (baseNoteName.Last() == '8') baseNoteName = baseNoteName.Replace('8', '4');
            AddKeys(baseNoteName);

            //现在scaleNotes应该有7个音
            //为了handle关系小调和大调的转换（此时谱子无需显式写出），需要把小调也加入进去
            //小调大部分音和大调相同，除了最后一个音大调没有，是大调主音上面减五度
            var extraNoteFromMinor = NoteHelper.GetNote(baseNoteName, 8);
            scaleNotes.Add(extraNoteFromMinor.Substring(0, extraNoteFromMinor.Length - 1));
        }

        //找到调所有的音,这里认为是大调
        //这里不区分大小调了,因为同主音大小调交替在乐谱中不表示,无法侦测出来
        private void AddKeys(string baseNoteName)
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

        public bool InKey(Note note)
        {
            //得到唱名，忽略音高
            var name = note.pitch.Substring(0, note.pitch.Length - 1);
            return scaleNotes.Contains(name);
        }

        //调性修正:给定一个调和一个音,得出该音被修正之后的音高字符串表示
        //例如给定E大调和G,则它应该被修正为G#
        public Note KeyFix(Note note)
        {
            var newNote = note;
            if (fifth == 0) return note;
            int notePitchHeight = int.Parse(note.pitch.Last().ToString());

            if (note.pitch.Contains("C") && !note.pitch.Contains("C#"))
            {
                if (fifth >= 2)
                {
                    newNote.pitch = "C#" + notePitchHeight;
                    newNote.id++;
                }
                if(fifth <= -6)
                {
                    notePitchHeight--;
                    newNote.pitch = "B" + notePitchHeight;
                    newNote.id--;
                }
            }
            if (note.pitch.Contains("D") && !note.pitch.Contains("D#"))
            {
                if (fifth >= 4)
                {
                    newNote.pitch = "D#" + notePitchHeight; newNote.id++;
                }
                if (fifth <= -4)
                {
                    newNote.pitch = "C#" + notePitchHeight; newNote.id--;
                }
            }
            if (note.pitch.Contains("E"))
            {
                if (fifth >= 6)
                {
                    newNote.pitch = "F" + notePitchHeight; newNote.id++;
                }
                if (fifth <= -2)
                {
                    newNote.pitch = "D#" + notePitchHeight; newNote.id--;
                }
            }
            if (note.pitch.Contains("F") && !note.pitch.Contains("F#"))
            {
                if (fifth >= 1)
                {
                    newNote.pitch = "F#" + notePitchHeight; newNote.id++;
                }
                if (fifth <= -7)
                {
                    newNote.pitch = "E" + notePitchHeight; newNote.id--;
                }
            }
            if (note.pitch.Contains("G") && !note.pitch.Contains("G#"))
            {
                if (fifth >= 3)
                {
                    newNote.pitch = "G#" + notePitchHeight; newNote.id++;
                }
                if (fifth <= -5)
                {
                    newNote.pitch = "F#" + notePitchHeight; newNote.id--;
                }
            }
            if (note.pitch.Contains("A") && !note.pitch.Contains("A#"))
            {
                if (fifth >= 5)
                {
                    newNote.pitch = "A#" + notePitchHeight; newNote.id++;
                }
                if (fifth <= -3)
                {
                    newNote.pitch = "G#" + notePitchHeight; newNote.id--;
                }
            }
            if (note.pitch.Contains("B"))
            {
                if (fifth >= 7)
                {
                    notePitchHeight++;
                    newNote.pitch = "C" + notePitchHeight; newNote.id++;
                }
                if (fifth <= -1)
                {
                    newNote.pitch = "A#" + notePitchHeight; newNote.id--;
                }
            }
            return newNote;
        }
    }
}
