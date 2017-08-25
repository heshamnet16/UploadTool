using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
namespace FileMerge
{
    public class Merger
    {
        private bool UseStream;
        private bool _DeleteTheFiles;

        public bool DeleteTheFiles
        {
            get { return _DeleteTheFiles; }
            set { _DeleteTheFiles = value; }
        }

        private char _FilesCountSeprator;

        public char FilesCountSeprator
        {
            get { return _FilesCountSeprator; }
            set { _FilesCountSeprator = value; }
        }

        private string _Tocken;

        public string Tocken
        {
            get { return _Tocken; }
            set { _Tocken = value; }
        }

        private Dictionary<string,byte[]> _FilesDictionary;
        public Dictionary<string,byte[]> FilesDictionary
        {
            get { return _FilesDictionary; }
            set { _FilesDictionary = value; }
        }
        private List<string> _FilesList;

        public List<string> FilesList
        {
            get { return _FilesList; }
            set { _FilesList = value; }
        }
        private string _DistinationFile;

        public string DistinationFile
        {
            get { return _DistinationFile; }
            set { _DistinationFile = value; }
        }

        public Merger(Dictionary<string,byte[]> Source,string distinationFile,string token,char filesSeprator)
        {
            this._DistinationFile = distinationFile;
            this._FilesDictionary = Source;
            this.UseStream = true;
            this.Tocken = token;
            this._FilesCountSeprator = filesSeprator;
        }
        public Merger(List<string> Source, string distinationFile,string token, char filesSeprator)
        {
            this._DistinationFile = distinationFile;
            this._FilesList = Source;
            this.UseStream = false;
            this.Tocken = token;
            this._FilesCountSeprator = filesSeprator;
        }
        //private
        public string getFullFileNameWithoutToken(string FileName)
        {
           return  FileName.Substring(0, FileName.IndexOf(this._Tocken));
        }
        public static string getFullFileNameWithoutToken(string FileName,string token)
        {
            return FileName.Substring(0, FileName.IndexOf(token));
        }
        //private
        public int getFileNumber(string FileName)
        {
            int last_ = FileName.LastIndexOf(_Tocken);
            int lastDot = FileName.LastIndexOf(_FilesCountSeprator);
            return Convert.ToInt16( FileName.Substring(last_+this._Tocken.Length,lastDot-(last_ + this._Tocken.Length)));
        }
        //private
        public static int getFileNumber(string FileName,string token,char separator)
        {
            int last_ = FileName.LastIndexOf(token);
            int lastDot = FileName.LastIndexOf(separator);
            return Convert.ToInt16(FileName.Substring(last_ + token.Length, lastDot - (last_ + token.Length)));
        }
        //private
        public static int getAllFilesCount(string fileName,char separator)
        {
            int lastDot = fileName.LastIndexOf(separator);
            return Convert.ToInt16(fileName.Substring(lastDot + 1, fileName.Length - lastDot - 1));
        }
        public int getAllFilesCount(string fileName)
        {
            int lastDot = fileName.LastIndexOf(_FilesCountSeprator);
            return Convert.ToInt16(fileName.Substring(lastDot+1, fileName.Length -  lastDot-1));
        }
        private bool IsComplete()
        {
            List<string> coll;
            if (this.UseStream)
                coll = this._FilesDictionary.Keys.ToList<string>();
            else
                coll = this._FilesList;
            if (coll.Count == getAllFilesCount(coll.First()))
                return true;
            else if (coll.Count == getAllFilesCount(coll.First()) + 1)
                return true;
            else
                return false;
        }
        public bool CheckTheFiles()
        {
            if (!IsComplete())
                return false;
            List<string> coll;
            ArrayList CompleteList = new ArrayList();
            if (this.UseStream)
                coll = this._FilesDictionary.Keys.ToList<string>();
            else
                coll = this._FilesList;
            int Filescount = getAllFilesCount(coll.First());
            for(int i = 0; i <= Filescount;i++)
            {
                CompleteList.Add(i);
            }
            foreach(string key in coll)
            {
                int fileNum = getFileNumber(key);
                if (CompleteList.Contains(fileNum))
                {
                    CompleteList.Remove(fileNum);
                    if (!this.UseStream)
                    {
                        if (!File.Exists(key))
                            return false;
                    }
                }
                else
                    return false;
            }
            if (CompleteList.Count == 0)
                return true;
            else
                return false;
        }
        public void Merge()
        {
            if (this.UseStream)
                MergeFilesDic();
            else
                MergeFilesList();
        }
        private void MergeFilesDic()
        {
            using (FileStream fs = new FileStream(this._DistinationFile, FileMode.Create))
            {
                string FileName = getFullFileNameWithoutToken(this._FilesDictionary.Keys.First());
                int FilesCount = getAllFilesCount(this._FilesDictionary.Keys.First());
                for (int i = 0;i<this._FilesDictionary.Keys.Count;i++)
                {
                    string key = FileName + _Tocken + i.ToString() + _FilesCountSeprator + FilesCount.ToString();
                    byte[] data = _FilesDictionary[key];
                    if (data != null && data.Length > 0)
                    {
                        MemoryStream ChunkStream = new MemoryStream(this._FilesDictionary[key]);
                        ChunkStream.CopyTo(fs);
                    }
                }
            }
        }
        private void MergeFilesList()
        {
            using (FileStream fs = new FileStream(this._DistinationFile, FileMode.Create))
            {
                string FileName = getFullFileNameWithoutToken(this._FilesList.First());
                int FilesCount = getAllFilesCount(this._FilesList.First());
                for (int i = 0; i < this._FilesList.Count; i++)
                {
                    string key = FileName + _Tocken + i.ToString() + _FilesCountSeprator + FilesCount.ToString();
                    using (FileStream ChunkStream = new FileStream(key, FileMode.Open))
                    {
                        if (ChunkStream != null && ChunkStream.Length > 0)
                        {
                            ChunkStream.CopyTo(fs);
                        }
                    }
                }
            }
        }
    }
}
