using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MusicXMLBasedCalc.Tests
{
    [TestClass]
    public class ScaleTest
    {
        [TestMethod]
        public void test_scale_notes()
        {
            //E大调
            var newScale = new Scale("major", 6);
            
            Assert.AreEqual(newScale.baseNoteName.Substring(0, 2), "F#");
            Assert.AreEqual(string.Join(",", newScale.scaleNotes), "F#,G#,A#,B,C#,D#,F,D");
        }
    }
}
