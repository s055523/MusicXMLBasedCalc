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
            var song = new Song(inputFile, "", 0, 10);
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
            var dC = ChordHelper.BuildThree(new Note("G1"), ChordThreeCategory.major);
            var sC = ChordHelper.BuildThree(new Note("F1"), ChordThreeCategory.major);
            var ret = new List<string>();
            for (int i = 1; i < 10; i++)
            {
                for(int j = 0; j < song.division; j++)
                {                   
                    songNotes = song.songNotes.Where(s => s.position >= j && s.position < j+1).SelectMany(s => s.notes);
                    notes = songNotes.Where(s => s.measureNumber == i);

                    if (!notes.Any()) continue;

                    var result = "小节" + i + ", 第" + j + "拍:";

                    //T
                    var degreeOfConT = ChordHelper.DegreeOfConsonance(notes.ToList(), c);
                    
                    //D
                    var degreeOfConD = ChordHelper.DegreeOfConsonance(notes.ToList(), dC);

                    //S
                    var degreeOfConS = ChordHelper.DegreeOfConsonance(notes.ToList(), sC);
                    
                    if(degreeOfConT < 0.8 && degreeOfConD < 0.8 && degreeOfConS < 0.8)
                    {
                        result+=("none");
                    }

                    else if (degreeOfConT > degreeOfConD && degreeOfConT > degreeOfConS)
                    {
                        result += ("T"); 
                    }
                    else if (degreeOfConD > degreeOfConT && degreeOfConD > degreeOfConS)
                    {
                        result += ("D"); 
                    }
                    else if (degreeOfConS > degreeOfConT && degreeOfConS > degreeOfConD)
                    {
                        result += ("S"); 
                    }
                    ret.Add(result);
                }
            }
        }
    }
}


