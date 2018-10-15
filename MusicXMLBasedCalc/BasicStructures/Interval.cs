using System;

namespace MusicXMLBasedCalc
{
    public class Interval
    {
        public Note baseNote;
        public Note upperNote;
        public int measureNumber;

        //音程含有多少个半音，例如小二度的length=1
        public int length;

        public bool isBiggerThanOctave;

        public IntervalCatagories intervalCategory;
        public ConsonanceCatagories consonanceCategory;

        //音程两个音的时值的乘积
        public double weight;

        public Interval(Note a, Note b, double w = 1)
        {
            baseNote = a;
            upperNote = b;
            weight = w * a.duration * b.duration;
            (length, intervalCategory, consonanceCategory) = GetIntervalDetails(a, b);
            isBiggerThanOctave = Math.Abs(b.id - a.id) > 12;
        }

        public static (int, IntervalCatagories, ConsonanceCatagories) GetIntervalDetails(Note a, Note b)
        {
            IntervalCatagories intervalCategory = 0;
            ConsonanceCatagories consonanceCategory = ConsonanceCatagories.perfectConsonance;

            //real length
            int length = b.id - a.id;

            var distance = Math.Abs(length);
            while (distance > 12)
            {
                distance -= 12;
            }

            //https://en.wikipedia.org/wiki/Consonance_and_dissonance
            switch (distance)
            {
                //unison and octave
                case 0:
                    intervalCategory = IntervalCatagories.unison;
                    consonanceCategory = ConsonanceCatagories.perfectConsonance;
                    break;
                case 12:
                    intervalCategory = IntervalCatagories.octave;
                    consonanceCategory = ConsonanceCatagories.perfectConsonance;
                    break;
                //fourth and fifth
                case 5:
                    intervalCategory = IntervalCatagories.perfectFourth;
                    consonanceCategory = ConsonanceCatagories.medianConsonance;
                    break;
                case 7:
                    intervalCategory = IntervalCatagories.perfectFifth;
                    consonanceCategory = ConsonanceCatagories.medianConsonance;
                    break;
                //minor and major third
                case 3:
                    intervalCategory = IntervalCatagories.minorThird;
                    consonanceCategory = ConsonanceCatagories.imperfectConsonance;
                    break;
                case 4:
                    intervalCategory = IntervalCatagories.majorThird;
                    consonanceCategory = ConsonanceCatagories.imperfectConsonance;
                    break;
                //major sixth and minor seventh
                case 9:
                    intervalCategory = IntervalCatagories.majorSixth;
                    consonanceCategory = ConsonanceCatagories.imperfectDissonance;
                    break;
                case 8:
                    intervalCategory = IntervalCatagories.minorSixth;
                    consonanceCategory = ConsonanceCatagories.imperfectDissonance;
                    break;
                //major second and minor sixth
                case 2:
                    intervalCategory = IntervalCatagories.majorSecond;
                    consonanceCategory = ConsonanceCatagories.medianDissonance;
                    break;
                case 10:
                    intervalCategory = IntervalCatagories.minorSeventh;
                    consonanceCategory = ConsonanceCatagories.medianDissonance;
                    break;
                //semitone, tritone, major seventh
                case 1:
                    intervalCategory = IntervalCatagories.minorSecond;
                    consonanceCategory = ConsonanceCatagories.perfectDisonnace;
                    break;
                case 6:
                    intervalCategory = IntervalCatagories.augmentFourth;
                    consonanceCategory = ConsonanceCatagories.perfectDisonnace;
                    break;
                case 11:
                    intervalCategory = IntervalCatagories.majorSeventh;
                    consonanceCategory = ConsonanceCatagories.perfectDisonnace;
                    break;
            }
            return (length, intervalCategory, consonanceCategory);
        }

        public void Play()
        {
            Console.Beep(baseNote.frequency, 500);
            Console.Beep(upperNote.frequency, 500);
        }
    }

    //音程的种类
    public enum IntervalCatagories
    {
        unison, minorSecond, majorSecond, minorThird, majorThird, perfectFourth, augmentFourth,
        perfectFifth, minorSixth, majorSixth, minorSeventh, majorSeventh, octave
    }

    //音程和谐的种类
    public enum ConsonanceCatagories
    {
        perfectConsonance, medianConsonance, imperfectConsonance, imperfectDissonance, medianDissonance, perfectDisonnace
    }
}
