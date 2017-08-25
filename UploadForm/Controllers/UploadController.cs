using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FileMerge;
namespace UploadForm.Controllers
{
    public class UploadController : Controller
    {
        public static Dictionary<string, byte[]> DataStream = new Dictionary<string, byte[]>();
        private string token = ".part_";
        public static object LockAttribute = new Object();
        // GET: Upload
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Upload()
        {
            bool ret = true;
            lock (LockAttribute)
            {
                try
                {
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        HttpPostedFileBase hpf = Request.Files[i] as HttpPostedFileBase;
                        if (hpf.ContentLength == 0) continue;
                        string fileName = Path.GetFileName(hpf.FileName);
                        if (DataStream.Keys.Contains(fileName))
                            continue;
                            MemoryStream ChunckStream = new MemoryStream();
                        hpf.InputStream.CopyTo(ChunckStream);
                            DataStream.Add(fileName, ChunckStream.ToArray());                        
                        ChunckStream.Close();
                        ChunckStream.Dispose();
                    }
                }
                catch { ret = false; }
                string TotalFileName;
                Dictionary<string, byte[]> temp;

                    TotalFileName = Merger.getFullFileNameWithoutToken(DataStream.Keys.First(), this.token);
                    temp = DataStream.Where(x => x.Key.Contains(TotalFileName)).ToDictionary(s => s.Key, M => M.Value);
                
                string savedFileName = Path.Combine(Server.MapPath("~/Uploads"), TotalFileName);
                Merger m = new Merger(temp, savedFileName, this.token, '.');
                if (m.CheckTheFiles())
                {
                    m.Merge();
                    List<string> toRemove = new List<string>();
                        foreach (string key in DataStream.Keys)
                        {
                        if (key.Contains(TotalFileName))
                            //DataStream.Remove(key);
                            toRemove.Add(key);
                        }
                    foreach (string key in toRemove)
                    {
                        DataStream.Remove(key);
                    }
                }
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
            lock(LockAttribute)
            {
                string TotalgetFileName = filename;
                Dictionary<string, byte[]> temp = 
                    DataStream.Where(x => x.Key.Contains(TotalgetFileName)).ToDictionary(t => t.Key, y => y.Value);
                foreach(string key in temp.Keys)
                {
                    int Filenum = Merger.getFileNumber(key, this.token, '.');
                    if (Filenum > Max)
                        Max = Filenum;
                }
            }
            return Json(Max, JsonRequestBehavior.AllowGet);
        }

    }
}