using System.Collections.Generic;

namespace MusicXMLBasedCalc.BasicStructures
{
    public class Chord
    {
        public string name { get; set; }
        public List<Note> notes { get; set; }
    }

    public enum ChordThreeCategory
    {
        major, minor, augment, diminished
    }
}
