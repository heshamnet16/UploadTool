﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileMerge;
using System.Collections.Generic;
using System.IO;

namespace UploadForm.Tests
{
    [TestClass]
    public class TestMerger
    {
        private Dictionary<string, byte[]> testDict = new Dictionary<string, byte[]>();
        private string token = ".part_";
        private void AddValues()
        {
            for(int i=52;i<=100;i++)
            {
                //lab 25 How to implement Validation using Angular and MVC.mp4.part_2.88
                string fileName1 = "lab 25 How to implement Validation using Angular and MVC.mp4" + token + i + ".105";
                testDict.Add(fileName1, new byte[] { });
            }
            for (int i = 0; i <= 50; i++)
            {
                //lab 25 How to implement Validation using Angular and MVC.mp4.part_2.88
                string fileName1 = "lab 25 How to implement Validation using Angular and MVC.mp4" + token + i + ".105";
                testDict.Add(fileName1, new byte[] { });
            }

            string fileName = "lab 25 How to implement Validation using Angular and MVC.mp4" + token + 104 + ".105";
            testDict.Add(fileName, new byte[] { });
            fileName = "lab 25 How to implement Validation using Angular and MVC.mp4" + token + 105 + ".105";
            testDict.Add(fileName, new byte[] { });
        }
        [TestMethod]
        public void TestgetFileNumber()
        {
            Merger m = new Merger(this.testDict , "", ".part_",'.');
            int ret = m.getFileNumber("lab 25 How to implement Validation using Angular and MVC.mp4.part_2.88");
            Assert.AreEqual(2, ret);
        }
        [TestMethod]
        public void TestFullFileNameWithoutToken()
        {
            Merger m = new Merger(this.testDict, "", ".part_",'.');
            string ret = m.getFullFileNameWithoutToken("lab 25 How to implement Validation using Angular and MVC.mp4.part_2.88");
            Assert.AreEqual("lab 25 How to implement Validation using Angular and MVC.mp4", ret);
        }
        [TestMethod]
        public void TestgetAllFilesCount()
        {
            Merger m = new Merger(this.testDict, "", ".part_",'.');
            int ret = m.getAllFilesCount("lab 25 How to implement Validation using Angular and MVC.mp4.part_2.8");
            Assert.AreEqual(8, ret);
        }
        [TestMethod]
        public void TestCheck()
        {
            AddValues();
            Merger m = new Merger(this.testDict, "", ".part_", '.');
            bool ret = m.CheckTheFiles();
            Assert.AreEqual(true, ret);
        }
        [TestMethod]
        public void TestgetFileNameWithoutExtension()
        {
            string ret = Merger.getFileNameWithoutExtension("lab 25 How to implement Validation using Angular and MVC.mp4.part_2.88",".part_");
            Assert.AreEqual("lab 25 How to implement Validation using Angular and MVC", ret);
        }
        [TestMethod]
        public void TestchangeFileNum()
        {
            AddValues();
            Merger m = new Merger(this.testDict, "", ".part_", '.');
            string ret = m.changeFileNum("lab 25 How to implement Validation using Angular and MVC.mp4.part_2.88", 56);
            Assert.AreEqual("lab 25 How to implement Validation using Angular and MVC.mp4.part_56.88", ret);
        }
        [TestMethod]
        public void TestgetCorruptedData()
        {
            AddValues();
            Merger m = new Merger(this.testDict, "", ".part_", '.');
            List<string> ret =  m.getCorruptedData();
            Assert.AreEqual(ret.Count, 51);
        }
    }
}
