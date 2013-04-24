using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HomePTServer.Models;
using System.Web.Script.Serialization;

namespace HomePTServer.Controllers
{
    public class ImagesController : Controller
    {
        //
        // GET: /Images/

        public ActionResult Index()
        {
            return View();
        }

        public class ImageArgs
        {
            public string name { get; set; }
        }

        [HttpPost]
        public String GetFullImage(ImageArgs args) {
            Dictionary<string, object> results = new Dictionary<string, object>();
            try {
                results["image"] = Convert.ToBase64String(GetImageData(args.name, false));
            } catch (Exception e) {
                results["error"] = "Invalid image name.";
            }
            return new JavaScriptSerializer().Serialize(results);
        }

        [HttpPost]
        public String GetThumbImage(ImageArgs args) {
            Dictionary<string, object> results = new Dictionary<string, object>();
            try {
                results["image"] = Convert.ToBase64String(GetImageData(args.name, true));
            } catch (Exception e) {
                results["error"] = "Invalid image name.";
            }
            return new JavaScriptSerializer().Serialize(results);
        }

        public static byte[] GetImageData(string imageName, bool thumbSize) {
            return System.IO.File.ReadAllBytes(PTDatabase.PathForImageNamed(imageName, thumbSize));
        }

    }
}
