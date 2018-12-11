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

        [TestMethod]
        public void test_interval_consonance()
        {
            //Arrange
            var inputFile = @"D:\新西兰学习生活\大学上课\乐谱数据备份\浪漫\【浪漫】布格缪勒Agitato.musicxml";

            //Act
            var song = new Song(inputFile, "");
            song.Parse();
            song.IntervalAnalysis();
            song.SongAnalysis();



        }
    }
}
