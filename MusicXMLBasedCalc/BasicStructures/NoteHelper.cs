using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace MusicXMLBasedCalc
{
    public static class NoteHelper
    {
        //C0 - B8
        public static List<NoteDic> noteDic;

        static NoteHelper()
        {
            //初始化所有音的表示
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

        /// <summary>
        /// 输入音的位置,得到音的字符串表示(例如输入1得到C#0)
        /// </summary>
        /// <param name="pitchPos"></param>
        /// <returns></returns>
        public static string GetNoteByPosition(int pitchPos)
        {
            return noteDic.First(n => n.id == pitchPos).name;
        }

        public static int GetNoteIdByPitch(string pitch)
        {
            if (pitch == "rest") return -1;
            return noteDic.First(n => n.name == pitch).id;
        }

        /// <summary>
        /// 获得一个音,它和某个指定的音相距numOfSemitones个半音
        /// </summary>
        /// <param name="baseName">指定的音</param>
        /// <param name="numOfSemitones"></param>
        /// <returns></returns>
        public static string GetNote(string baseName, int numOfSemitones)
        {
            var baseId = noteDic.First(n => n.name == baseName).id;
            baseId += numOfSemitones;
            return GetNoteByPosition(baseId);
        }

        /// <summary>
        /// 从XML中解析音符
        /// </summary>
        /// <param name="note">XML</param>
        /// <param name="division">musicxml的一个属性，表示一拍的长度，例如如果division=32,则一个XML中duration=64的音符就是两拍</param>
        /// <param name="measureNumber"></param>
        /// <returns></returns>
        public static Note ParseNote(XElement note, int measureNumber, double division)
        {
            //忽略倚音
            var isGrace = note.Descendants("grace");
            if (isGrace.Any())
            {
                return null;
            }

            var defaultx = note.Attribute("default-x");

            //解析出音符的时值
            var duration = double.Parse(note.Descendants("duration").First().Value);
            duration /= division;

            //defaultx不存在，则it is a rest
            if (defaultx == null)
            {
                return new Note("rest", duration, 0, measureNumber);
            }

            //音名，例如B
            var step = note.Descendants("step").First().Value;

            //高度，例如4
            var octave = note.Descendants("octave").First().Value;

            //加起来就是音高
            var pitch = step + octave;
            var pitchPos = NoteHelper.noteDic.First(n => n.name == pitch).id;

            //利用alter来判别是否需要调整，它包括了调和临时记号的情况
            var alternote = note.Descendants("alter").FirstOrDefault();
            if (alternote != null)
            {
                int value = int.Parse(alternote.Value);
                pitchPos += value;
            }

            //调整之后的音的pitch
            pitch = GetNoteByPosition(pitchPos);

            //探测同音高连线，要将这两个音归并为一个，否则会多出了一个纯一度
            var tieNode = note.Descendants("tie").FirstOrDefault();

            //普通的情况
            if (tieNode == null)
            {
                Note n = new Note(pitch, duration, double.Parse(defaultx.Value), measureNumber);
                if (note.Descendants("mordent").Any() || note.Descendants("inverted-mordent").Any()
                    || note.Descendants("schleifer").Any())
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

                var timeModification = note.Descendants("time-modification").FirstOrDefault();
                if (timeModification != null)
                {
                    var actual = double.Parse(note.Descendants("actual-notes").First().Value);
                    var normal = double.Parse(note.Descendants("normal-notes").First().Value);
                    n.timeModification = actual / normal;
                }

                return n;
            }
            //这和上面的音是同一个音
            else
            {
                var slurStatus = tieNode.Attribute("type").Value;
                var n = new Note(pitch, duration, double.Parse(defaultx.Value), measureNumber, slurStatus);

                return n;
            }
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
}
