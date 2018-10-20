using System;

namespace MusicXMLBasedCalc
{
    public class Note
    {
        public readonly double semitone = Math.Pow(2, 0.0833);
        public const double A4 = 440;

        //音高的字符串表示
        public string pitch { get; set; }

        //时值
        public double duration { get; set; }

        //音在谱子上的物理位置
        public double position { get; set; }

        //音本身的位置（例如C0就是1，D0就是2，等等）
        public int id { get; set; }

        //音在哪个小节
        public int measureNumber { get; set; }

        //该音符是否被临时记号修饰,如果是,则pitch可能需要进行调整
        public string accidental { get; set; }

        public int frequency;

        //是否是连线的开始和结束
        public SlurStatus slurStatus;

        //这个音属于哪个声部
        public int voice { get; set; }

        public bool isMordent { get; set; }
        public bool isTrill { get; set; }
        public bool isTurn { get; set; }

        public Note(string p)
        {
            id = NoteHelper.GetNoteIdByPitch(p);
            pitch = p;
        }

        public Note(string p, double d, double pos, int m, string acc = "", string slur = "")
        {
            pitch = p;
            duration = d;
            position = pos;
            id = NoteHelper.GetNoteIdByPitch(pitch);
            measureNumber = m;
            accidental = acc;
            frequency = GetApproxFrequency();

            switch (slur)
            {
                case "start":
                    slurStatus = SlurStatus.start;
                    break;
                case "stop":
                    slurStatus = SlurStatus.stop;
                    break;
                case "continue":
                    slurStatus = SlurStatus.cont;
                    break;
                default:
                    slurStatus = SlurStatus.none;
                    break;
            }
        }

        public override string ToString()
        {
            return pitch + "|" + duration;
        }

        private int GetApproxFrequency()
        {
            var distanceToA4 = this.id - 57;
            return int.Parse(Math.Round(Math.Pow(semitone, distanceToA4) * A4).ToString());
        }

        /// <summary>
        /// 如果这个音有临时记号，获得它原本的音
        /// </summary>
        /// <returns></returns>
        public string GetOriginalPitchForAccidental()
        {
            switch (accidental)
            {
                case "sharp":
                    return NoteHelper.GetNote(this.pitch, -1);
                case "sharp-sharp":
                    return NoteHelper.GetNote(this.pitch, -2);
                case "flat":
                    return NoteHelper.GetNote(this.pitch, 1);
                case "flat-flat":
                    return NoteHelper.GetNote(this.pitch, 2);
                default:
                    return pitch;
            }
        }

        public void Play()
        {
            Console.Beep(frequency, 1000);
        }
    }

    public enum SlurStatus
    {
        start, cont, stop, none
    }
}