using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MusicXMLBasedCalc.Tests
{
    [TestClass]
    public class IntervalTest
    {
        [TestMethod]
        public void test_interval()
        {
            var newInterval = new Interval(new Note("C1", 1, 1, 1, ""), new Note("F#1", 1, 1, 2, "sharp"));
            
            Assert.AreEqual(IntervalCatagories.augmentFourth, newInterval.intervalCategory);
            Assert.AreEqual(ConsonanceCatagories.perfectDisonnace, newInterval.consonanceCategory);
        }
    }
}
