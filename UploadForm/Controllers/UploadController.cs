using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FileMerge;
namespace UploadForm.Controllers
{
    public class DataSaver
    {
        public static Dictionary<string, byte[]> DataStream = new Dictionary<string, byte[]>();
        public static List<string> FilesNames = new List<string>();
        public static object LockAttribute = new Object();

        private bool _createSubDirectory;

        public bool CreateSubDirectory
        {
            get { return _createSubDirectory; }
            set { _createSubDirectory = value; }
        }

        private string _SaveDirectory;

        public string SaveDirectory
        {
            get
            {
                if (_SaveDirectory.Last() != '\\')
                    _SaveDirectory += '\\';
                return _SaveDirectory;
            }
            set {

                _SaveDirectory = value;
            }
        }

        private bool useMemory;

        public bool UseMemory
        {
            get { return useMemory; }
            set { useMemory = value; }
        }
        public DataSaver(bool useMemory)
        {
            this.useMemory = useMemory;
        }
        private bool AddToMemory(string fileN, MemoryStream data)
        {
            try
            {
                lock (LockAttribute)
                {
                    byte[] Bdata = data.ToArray();
                    if (!DataStream.Keys.Contains(fileN))
                        DataStream.Add(fileN, Bdata);
                    data.Close();
                    data.Dispose();
                }
                return true;
            }
            catch (Exception ex)
            { 
                throw ex;
            }
        }
        private bool AddToHard(string fileN, byte[] data)
        {
            try
            {
                if (this._SaveDirectory == null || this._SaveDirectory.Length < 3)
                    throw new InvalidDataException("you musst set the SaveDirectory property");
                string DirName = Path.GetDirectoryName(this.SaveDirectory);
                if (!Directory.Exists(DirName))
                {
                    if (this._createSubDirectory)
                        Directory.CreateDirectory(DirName);
                    else
                        throw new IOException("Directory is not Created");

                }
                string FullFileName = DirName + fileN;
                using (FileStream Fs = new FileStream(FullFileName, FileMode.Create, FileAccess.Write))
                {
                    Fs.Write(data, 0, data.Length);
                    Fs.Flush();
                    lock (LockAttribute)
                    { FilesNames.Add(FullFileName); }
                }
                return true;
            }catch(Exception ex)
            { throw ex;}
        }

        public bool SaveFile(string FileName, object file)
        {
            if (file is MemoryStream)
            {
               return  AddToMemory(FileName, (MemoryStream)file);
            }
            else if (file is byte[])
            {
                if (this.useMemory)
                {
                    lock (LockAttribute)
                    { DataStream.Add(FileName, (byte[])file); }
                    return true;
                }
                else
                {
                    return AddToHard(FileName, (byte[])file);
                }
            }
            else
                throw new InvalidDataException("you can only passing MemoryStream or byte[] data type");            
        }
        public bool DeleteFile(string FileName)
        {
            if(this.useMemory)
            {
                lock(LockAttribute)
                {
                    if (DataStream.Keys.Contains(FileName))
                    {
                        DataStream.Remove(FileName);
                    }
                }
                return true;
            }
            else
            {
                if(File.Exists(FileName))
                {
                    File.Delete(FileName);
                    lock (LockAttribute)
                        FilesNames.Remove(this.SaveDirectory + FileName);
                    return true;
                }
                else
                {
                    string FullFileName = this.SaveDirectory + FileName;
                    if (File.Exists(FullFileName))
                    {
                        File.Delete(FullFileName);
                        lock(LockAttribute)
                        {
                            FilesNames.Remove(this.SaveDirectory + FileName);
                        }
                    }
                    return true;
                }
            }
        }

        public bool DeleteAllEntries(string FileName)
        {
            if(this.useMemory)
            {
                List<string> toRemove = new List<string>();
                Dictionary<string, byte[]> temp;
                lock (LockAttribute)
                {
                    temp = DataStream.Where(x => x.Key.Contains(FileName)).ToDictionary(k => k.Key, v => v.Value);
                }
                foreach (string key in temp.Keys)
                {
                    if (key.Contains(FileName))
                    {
                        if(DeleteFile(key))
                            toRemove.Add(key);
                    }
                }
                foreach (string key in toRemove)
                {
                    lock (LockAttribute)
                    {
                        DataStream.Remove(key);
                    }
                }
            }
            else
            {
                List<string> temp;
                string dirName;
                lock (LockAttribute)
                {
                    temp = FilesNames.Where(x => x.Contains(FileName)).ToList();
                }
                if (temp != null && temp.Count > 0)
                {
                    dirName = Path.GetDirectoryName(temp.First());
                    foreach (string fileN in temp)
                    {
                        DeleteFile(fileN);
                    }
                    string[] ret = Directory.GetFiles(dirName);
                    if (ret == null || ret.Length == 0)
                        Directory.Delete(dirName);
                }
                else
                    return true;
            }
            return true;
        }
        public int getLastFileChunck(string FileName,string token)
        {
            if(this.useMemory)
            {
                Dictionary<string, byte[]> temp;
                int Max = 0;
                lock (LockAttribute)
                {
                    temp = DataStream.Where(x => x.Key.Contains(FileName)).ToDictionary(s => s.Key, M => M.Value);
                    foreach (string key in temp.Keys)
                    {
                        int Filenum = Merger.getFileNumber(key, token, '.');
                        if (Filenum > Max)
                            Max = Filenum;
                    }
                }
                return Max;
            }
            else
            {
                if (!Directory.Exists(this.SaveDirectory))
                    return 0;
                string[] Files = Directory.GetFiles(this.SaveDirectory);
                int Max = 0;
                foreach(string fileN in Files )
                {
                    int Filenum = Merger.getFileNumber(fileN, token, '.');
                    if (Filenum > Max)
                        Max = Filenum;
                }
                return Max;
            }
        }
    }
    public class UploadController : Controller
    {
        //public static Dictionary<string, byte[]> DataStream = new Dictionary<string, byte[]>();
        //public static List<string> FilesNames = new List<string>();
        private string token = ".part_";
        // GET: Upload
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Upload()
        {
            bool ret = true;
            string fileName = string.Empty;
            DataSaver dtS = new DataSaver(false);
            dtS.CreateSubDirectory = true;
                try
                {
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        HttpPostedFileBase hpf = Request.Files[i] as HttpPostedFileBase;
                        if (hpf.ContentLength == 0) continue;
                        fileName = Path.GetFileName(hpf.FileName);
                        MemoryStream ChunckStream = new MemoryStream();
                        hpf.InputStream.CopyTo(ChunckStream);
                        dtS.SaveDirectory = Server.MapPath("~/Uploads/") + Merger.getFileNameWithoutExtension(fileName,this.token);
                        dtS.SaveFile(fileName, ChunckStream.ToArray());
                        ChunckStream.Close();
                        ChunckStream.Dispose();
                    }
                }
                catch { ret = false; }

                
                string savedFileName = Path.Combine(Server.MapPath("~/Uploads"), Merger.getFullFileNameWithoutToken(fileName,this.token));
                Merger m = new Merger(DataSaver.FilesNames, savedFileName, this.token, '.');
                if (m.CheckTheFiles())
                {
                    m.Merge();
                    dtS.DeleteAllEntries(fileName);
                }
            if (ret)
                return Json("Success :)", JsonRequestBehavior.AllowGet);
            else
                return Json("Falid :(", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult RecievedData()
        {
            string filename = Request.Params["id"];
            int Max = 0;
            DataSaver dts = new DataSaver(false);
            dts.SaveDirectory = Server.MapPath("~/Uploads") + Merger.getFileNameWithoutExtension(filename, this.token);
            Max = dts.getLastFileChunck(filename, this.token);
            return Json(Max, JsonRequestBehavior.AllowGet);
        }

        [HttpDelete]
        public ActionResult ReSetData()
        {
            string ret = "Deleted :)";
            string filename = Request.Params["id"];
            DataSaver dts = new DataSaver(false);
            dts.DeleteAllEntries(filename);
            return Json(ret);
        }

    }
}