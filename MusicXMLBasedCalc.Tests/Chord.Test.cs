using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using MusicXMLBasedCalc.BasicStructures;
using System.Collections.Generic;

namespace MusicXMLBasedCalc.Tests
{
    [TestClass]
    public class ChordTest
    {
        [TestMethod]
        public void test_chord()
        {
            //建立一个c1上的大三和弦（c1e1g1）
            var c = ChordHelper.BuildThree(new Note("C1"), ChordThreeCategory.major);
            var chordNoteList = c.notes.Select(n => n.pitch).ToList();
            Assert.AreEqual("E1", chordNoteList[1]);
            Assert.AreEqual("G1", chordNoteList[2]);

            var inputFile = @"D:\新西兰学习生活\大学上课\乐谱数据\古典\【古典】莫扎特C大调奏鸣曲K.545第一乐章.musicxml";
            var song = new Song(inputFile, "");
            song.Parse();

            //第一小节第1拍
            var songNotes = song.songNotes.Where(s => s.position < 1).SelectMany(s => s.notes);
            var notes = songNotes.Where(s => s.measureNumber == 1);
            
            var degreeOfCon = ChordHelper.DegreeOfConsonance(notes.ToList(), c);
            Assert.AreEqual(1, degreeOfCon);

            //第一小节第2拍
            songNotes = song.songNotes.Where(s => s.position >= 1 && s.position < 2).SelectMany(s => s.notes);
            notes = songNotes.Where(s => s.measureNumber == 1);

            degreeOfCon = ChordHelper.DegreeOfConsonance(notes.ToList(), c);
            Assert.AreEqual(1, degreeOfCon);

            //第2小节第1拍
            songNotes = song.songNotes.Where(s => s.position < 1).SelectMany(s => s.notes);
            notes = songNotes.Where(s => s.measureNumber == 2);
            
            degreeOfCon = ChordHelper.DegreeOfConsonance(notes.ToList(), c);
            Assert.AreNotEqual(1, degreeOfCon);

            //第2小节第2拍
            songNotes = song.songNotes.Where(s => s.position >= 1 && s.position < 2).SelectMany(s => s.notes);
            notes = songNotes.Where(s => s.measureNumber == 2);
            
            degreeOfCon = ChordHelper.DegreeOfConsonance(notes.ToList(), c);
            Assert.AreNotEqual(1, degreeOfCon);

            //遍历
            var ret = ChordHelper.ChordAnalysis(1, song.numOfMeasures, song.songNotes, song.scaleList, song.division, song.numOfMeasures);
            double non1 = (double)ret.Where(r => r.Contains("none")).Count() / (double)ret.Count;

            inputFile = @"D:\新西兰学习生活\大学上课\乐谱数据\现代\【现代】普罗科菲耶夫罗密欧与朱丽叶阳台场景.musicxml";
            var song2 = new Song(inputFile, "");
            song2.Parse();
            var ret2 = ChordHelper.ChordAnalysis(1, song2.numOfMeasures, song2.songNotes, song2.scaleList, song2.division, song2.numOfMeasures);
            double non2 = (double)ret2.Where(r => r.Contains("none")).Count() / (double)ret2.Count;

            Assert.AreEqual(true, non1 < non2);
        }
    }
}


