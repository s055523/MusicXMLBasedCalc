using MusicXMLBasedCalc.BasicStructures;
using System.Collections.Generic;
using System.Linq;

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

        public Chord main { get; set; }
        public Chord second { get; set; }
        public Chord third { get; set; }
        public Chord subDominant { get; set; }
        public Chord dominant { get; set; }
        public Chord sixth { get; set; }

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

            //如果超出了C0-B8范围
            if (baseNoteName.Last() == '8') baseNoteName = baseNoteName.Replace('8', '4');
            
            //小调的话把根音调成大调下方小三度
            if (mode == "minor")
            {
                baseNoteName = new Note(NoteHelper.GetNote(baseNoteName, -3)).pitch;
            }

            scaleNotes.Add(baseNoteName.Substring(0, baseNoteName.Length - 1));

            AddKeys(baseNoteName, mode);

            var baseNote = new Note(scaleNotes[0] + "4");
            var secondNote = new Note(scaleNotes[1] + "4");
            var thirdNote = new Note(scaleNotes[2] + "4");
            var fourthNote = new Note(scaleNotes[3] + "4");
            var fifthNote = new Note(scaleNotes[4] + "4");
            var sixthNote = new Note(scaleNotes[5] + "4");
            
            //建立和弦
            if (mode == "major")
            {
                main = ChordHelper.BuildThree(baseNote, ChordThreeCategory.major);
                second = ChordHelper.BuildThree(secondNote, ChordThreeCategory.minor);
                third = ChordHelper.BuildThree(thirdNote, ChordThreeCategory.minor);
                subDominant = ChordHelper.BuildThree(fourthNote, ChordThreeCategory.major);
                dominant = ChordHelper.BuildThree(fifthNote, ChordThreeCategory.major);
                sixth = ChordHelper.BuildThree(sixthNote, ChordThreeCategory.minor);
            }
            else
            {
                main = ChordHelper.BuildThree(baseNote, ChordThreeCategory.minor);
                second = ChordHelper.BuildThree(secondNote, ChordThreeCategory.diminished);
                third = ChordHelper.BuildThree(thirdNote, ChordThreeCategory.major);
                subDominant = ChordHelper.BuildThree(fourthNote, ChordThreeCategory.minor);
                dominant = ChordHelper.BuildThree(fifthNote, ChordThreeCategory.major);
                sixth = ChordHelper.BuildThree(sixthNote, ChordThreeCategory.major);
            }
        }

        //找到调所有的音
        private void AddKeys(string baseNoteName, string mode)
        {
            if (mode == "major" || mode == string.Empty)
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

                //关系小调
                var extraNote = NoteHelper.GetNote(nextNoteName, -3);
                scaleNotes.Add(extraNote.Substring(0, extraNote.Length - 1));
            }
            else if (mode == "minor")
            {
                //小调：全半全全半3半
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

                //关系大调
                var extraNote = NoteHelper.GetNote(nextNoteName, -1);
                scaleNotes.Add(extraNote.Substring(0, extraNote.Length - 1));
            }
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
