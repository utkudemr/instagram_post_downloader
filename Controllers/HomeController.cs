using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public WebClient webClient;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {

            return View();
        }

       

        public RootObject1 GetPostInfo(string postUrl)
        {
            try
            {
                var request = HttpRequestBuilder.Get($"{postUrl}?__a=1");
                request.Referer = postUrl;
                request.Headers["X-Requested-With"] = "XMLHttpRequest";
                request.Timeout = 10000; // 10 seconds
                using (var response = request.GetResponse() as HttpWebResponse)
                using (var responseStream = response.GetResponseStream())
                using (var gzipStream = new GZipStream(responseStream, CompressionMode.Decompress))
                using (var streamReader = new StreamReader(gzipStream))
                {
                    var data = streamReader.ReadToEnd();
                    return JsonConvert.DeserializeObject<RootObject1>(data);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public string UrlCheck(string url)
        {
            var newurl = url;
            if (url.Contains("web_copy_link"))
            {
                int idx = url.LastIndexOf('/');
                if (idx != -1)
                {
                    newurl = url.Substring(0, idx);
                }
            }
            else if (url.Contains("@"))
            {
                int idx = url.LastIndexOf('@');
                if (idx != -1)
                {
                    newurl = url.Substring(0, idx);
                }
            }
            return newurl;
        }

        [HttpPost]
        public JsonResult Post(string url)
        {

            var type = "";
            url = UrlCheck(url);
            var postInfo =GetPostInfo(url);
            var convertedUrl = "";
            if (postInfo.graphql.shortcode_media.video_url == null)
            {
                convertedUrl = postInfo.graphql.shortcode_media.display_url;
                type = "image";
            }
            else
            {
                convertedUrl = postInfo.graphql.shortcode_media.video_url;
                type = "video";
            }

            return Json(new
            {
                response = true,
                url = convertedUrl,
                type = type,
                owner = postInfo.graphql.shortcode_media.owner,
                caption = postInfo.graphql.shortcode_media.edge_media_to_caption.edges[0].node.text,
                multiPhotos = postInfo.graphql.shortcode_media.edge_sidecar_to_children.edges

            });
        }













    }
}
