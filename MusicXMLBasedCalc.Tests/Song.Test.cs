﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MusicXMLBasedCalc.Tests
{
    [TestClass]
    public class SongTest
    {
        [TestMethod]
        public void test_easy_music_fragment_parser()
        {
            //Arrange
            var inputFile = @"D:\新西兰学习生活\大学上课\Debug\test1.musicxml";

            //Act
            var song = new Song(inputFile, "");

            song.Parse();
            song.IntervalAnalysis();
            song.SongAnalysis();

            //Assert
            //有一个和弦
            Assert.AreEqual(13, song.songNotes.Count);
            Assert.AreEqual(14, song.intervalList.Count);

            Assert.AreEqual("F4|1", song.songNotes[9].information);
            Assert.AreEqual("F#4|1", song.songNotes[10].information);

            Assert.AreEqual(IntervalCatagories.minorSixth, song.intervalList[0].intervalCategory);
        }

        [TestMethod]
        public void test_chords()
        {
            //Arrange
            var inputFile = @"D:\新西兰学习生活\大学上课\Debug\testChord.musicxml";

            //Act
            var song = new Song(inputFile, "");
            song.Parse();
            song.IntervalAnalysis();
            song.SongAnalysis();

            //Assert
            //有2个和弦
            Assert.AreEqual(2, song.songNotes.Count);
            Assert.AreEqual(6 + 3 + 3 * 4, song.intervalList.Count);

            Assert.AreEqual("B3|2,D4|2,F#4|2,C#5|2", song.songNotes[0].information);
            Assert.AreEqual("E4|2,G4|2,B4|2", song.songNotes[1].information);
        }

        [TestMethod]
        public void test_multiple_voices()
        {
            //Arrange
            var inputFile = @"D:\新西兰学习生活\大学上课\Debug\testMultipleVoices.musicxml";

            //Act
            var song = new Song(inputFile, "");
            song.Parse(); song.IntervalAnalysis();
            song.SongAnalysis();

            //Assert
            Assert.AreEqual(9, song.songNotes.Count);
            Assert.AreEqual(5, song.songNotes[0].information.Split(',').Length);
            Assert.AreEqual("G4|1,A#3|24,D#4|24,G4|24,D#2|8", song.songNotes[0].information);
        }

        [TestMethod]
        public void test_ornament()
        {
            //Arrange
            var inputFile = @"D:\新西兰学习生活\大学上课\乐谱数据\巴洛克\【巴洛克】巴赫G大调小步舞曲.musicxml";

            //Act
            var song = new Song(inputFile, "");
            song.Parse(); song.IntervalAnalysis();
            song.SongAnalysis();

            //Assert
            Assert.AreEqual(5, song.NumOfMordent);
        }

        [TestMethod]
        public void test_duration()
        {
            //Arrange
            var inputFile = @"D:\新西兰学习生活\大学上课\乐谱数据\巴洛克\【巴洛克】巴赫《二部创意---NO.13》第13首.musicxml";

            //Act
            var song = new Song(inputFile, "");
            song.Parse(); song.IntervalAnalysis();
            song.SongAnalysis();

            var notes = song.songNotes.SelectMany(n => n.notes);
            var notes16th = notes.Where(n => n.duration == 1).Count();
            var notes8th = notes.Where(n => n.duration == 2).Count();
            var notes4th = notes.Where(n => n.duration == 4).Count();

            //Assert
            Assert.IsTrue(notes16th > 400);
        }

        [TestMethod]
        public void test_key()
        {
            //Arrange
            var inputFile = @"D:\新西兰学习生活\大学上课\乐谱数据\巴洛克\【巴洛克】巴赫《前奏曲》平均律第3首（BWV848）.musicxml";

            //Act
            var song = new Song(inputFile, "");
            song.Parse(); song.IntervalAnalysis();
            song.SongAnalysis();
        }

        [TestMethod]
        public void test_all()
        {
            var inputFile = @"D:\新西兰学习生活\大学上课\测试数据\【古典】库劳C大调小奏鸣曲第三乐章急板.musicxml";

            var song = new Song(inputFile, "");
            song.Parse();
            song.IntervalAnalysis();

            var allnotes = song.songNotes.SelectMany(n => n.notes);
            var notesWithinFirstFiveMeasures = allnotes.Where(n => n.measureNumber < 6);

            Assert.AreEqual(4, notesWithinFirstFiveMeasures.Where(n => n.pitch == "G4").Count());

            song.SongAnalysis(1, 1);

        }
    }
}