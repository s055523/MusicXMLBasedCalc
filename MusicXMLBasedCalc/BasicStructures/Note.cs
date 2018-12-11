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

        //音本身的位置（例如C0就是1，C0#就是2，等等）
        public int id { get; set; }

        //音在哪个小节
        public int measureNumber { get; set; }

        public int frequency;

        //是否是连音线(tie)的开始和结束,不是普通的連綫（slur)
        public tieStatus tieStatus;

        //长经过句的真正时值
        public double timeModification { get; set; }

        public bool isMordent { get; set; }
        public bool isTrill { get; set; }
        public bool isTurn { get; set; }

        public Note(string p)
        {
            id = NoteHelper.GetNoteIdByPitch(p);
            pitch = p;
            duration = 1;
        }

        public Note(string p, double d, double pos, int m, string slur = "")
        {
            pitch = p;
            duration = d;
            position = pos;
            id = NoteHelper.GetNoteIdByPitch(pitch);
            measureNumber = m;
            frequency = GetApproxFrequency();

            switch (slur)
            {
                case "start":
                    tieStatus = tieStatus.start;
                    break;
                case "stop":
                    tieStatus = tieStatus.stop;
                    break;
                case "continue":
                    tieStatus = tieStatus.cont;
                    break;
                default:
                    tieStatus = tieStatus.none;
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

        public void Play()
        {
            Console.Beep(frequency, 1000);
        }
    }

    public enum tieStatus
    {
        start, cont, stop, none
    }
}