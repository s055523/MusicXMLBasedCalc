using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicXMLBasedCalc
{
    public static class NoteHelper
    {
        public static List<NoteDic> noteDic;

        static NoteHelper()
        {
            noteDic = new List<NoteDic>();
            int index = 1;
            for (int i = 0; i < 9; i++)
            {
                noteDic.Add(new NoteDic(index, "C" + i));
                index++;
                noteDic.Add(new NoteDic(index, "C#" + i));
                index++;
                noteDic.Add(new NoteDic(index, "D" + i));
                index++;
                noteDic.Add(new NoteDic(index, "D#" + i));
                index++;
                noteDic.Add(new NoteDic(index, "E" + i));
                index++;
                noteDic.Add(new NoteDic(index, "F" + i));
                index++;
                noteDic.Add(new NoteDic(index, "F#" + i));
                index++;
                noteDic.Add(new NoteDic(index, "G" + i));
                index++;
                noteDic.Add(new NoteDic(index, "G#" + i));
                index++;
                noteDic.Add(new NoteDic(index, "A" + i));
                index++;
                noteDic.Add(new NoteDic(index, "A#" + i));
                index++;
                noteDic.Add(new NoteDic(index, "B" + i));
                index++;
            }
        }

        public static string GetNoteByPosition(int pitchPos)
        {
            return noteDic.First(n => n.id == pitchPos).name;
        }

        public static string GetNote(string baseName, int numOfSemitones)
        {
            var baseId = noteDic.First(n => n.name == baseName).id;
            baseId += numOfSemitones;
            return GetNoteByPosition(baseId);
        }
    }
}
