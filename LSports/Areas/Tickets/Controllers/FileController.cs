using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using LSports.Framework.DataAccess.Repositories;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Framework.Models.CustomClasses;
using File = LSports.Framework.Models.CustomClasses.File;

namespace LSports.Areas.Tickets.Controllers
{
    public class FileController : Controller
    {
        // GET: File
        private readonly IFileRepository _fileRepository;


        public FileController() : this(new FileRepository())
        {
            
        }

        public FileController(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }


        private string StorageRoot
        {
            get { return Path.Combine(Server.MapPath("~/Files")); }
        }


        //DONT USE THIS IF YOU NEED TO ALLOW LARGE FILES UPLOADS
        [HttpGet]
        public void Delete(int id)
        {
            var f = _fileRepository.GetFile(id);

            var filename = f.Name;

            var filePath = Path.Combine(Server.MapPath("~/Files"), filename);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        //DONT USE THIS IF YOU NEED TO ALLOW LARGE FILES UPLOADS
        [HttpGet]
        public void Download(int id)
        {
            var f = _fileRepository.GetFile(id);

            var filename = f.Name;

            var filePath = Path.Combine(Server.MapPath("~/Files"), filename);

            var context = HttpContext;

            if (System.IO.File.Exists(filePath))
            {
                context.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + filename + "\"");
                context.Response.ContentType = "application/octet-stream";
                context.Response.ClearContent();
                context.Response.WriteFile(filePath);
            }
            else
                context.Response.StatusCode = 404;
        }

        //DONT USE THIS IF YOU NEED TO ALLOW LARGE FILES UPLOADS
        [HttpPost]
        public ActionResult UploadFiles()
        {
            var r = new List<ViewDataUploadFilesResult>();

            foreach (string file in Request.Files)
            {
                var statuses = new List<ViewDataUploadFilesResult>();
                var headers = Request.Headers;

                if (string.IsNullOrEmpty(headers["X-File-Name"]))
                {
                    UploadWholeFile(Request, statuses);
                }
                else
                {
                    UploadPartialFile(headers["X-File-Name"], Request, statuses);
                }


                JsonResult result = Json(statuses);
                result.ContentType = "text/plain";
                result.MaxJsonLength = 104857600;

                return result;
            }

            return Json(r);
        }

        private string EncodeFile(string fileName)
        {
            return Convert.ToBase64String(System.IO.File.ReadAllBytes(fileName));
        }

        //DONT USE THIS IF YOU NEED TO ALLOW LARGE FILES UPLOADS
        //Credit to i-e-b and his ASP.Net uploader for the bulk of the upload helper methods - https://github.com/i-e-b/jQueryFileUpload.Net
        private void UploadPartialFile(string fileName, HttpRequestBase request, List<ViewDataUploadFilesResult> statuses)
        {
            if (request.Files.Count != 1) throw new HttpRequestValidationException("Attempt to upload chunked file containing more than one fragment per request");
            var file = request.Files[0];
            var inputStream = file.InputStream;

            var fullName = Path.Combine(StorageRoot, Path.GetFileName(fileName));

            using (var fs = new FileStream(fullName, FileMode.Append, FileAccess.Write))
            {
                var buffer = new byte[1024];

                var l = inputStream.Read(buffer, 0, 1024);
                while (l > 0)
                {
                    fs.Write(buffer, 0, l);
                    l = inputStream.Read(buffer, 0, 1024);
                }
                fs.Flush();
                fs.Close();
            }

            var f = _fileRepository.Insert(new File { Name = file.FileName, Path = "" });

            statuses.Add(new ViewDataUploadFilesResult()
            {
                Name = fileName,
                Size = file.ContentLength,
                Type = file.ContentType,
                Url = Url.Action("Download", "File", new { id = f.Id }),
                DeleteUrl = Url.Action("Delete", "File", new { id = f.Id }),
                ThumbnailUrl = @"data:image/png;base64," + EncodeFile(fullName),
                DeleteType = "GET",
            });
        }

        //DONT USE THIS IF YOU NEED TO ALLOW LARGE FILES UPLOADS
        //Credit to i-e-b and his ASP.Net uploader for the bulk of the upload helper methods - https://github.com/i-e-b/jQueryFileUpload.Net
        private void UploadWholeFile(HttpRequestBase request, List<ViewDataUploadFilesResult> statuses)
        {
            for (int i = 0; i < request.Files.Count; i++)
            {
                var file = request.Files[i];

                var fullPath = Path.Combine(StorageRoot, Path.GetFileName(file.FileName));

                file.SaveAs(fullPath);

                var f = _fileRepository.Insert(new File { Name = file.FileName, Path = "" });

                statuses.Add(new ViewDataUploadFilesResult()
                {
                    FileId = f.Id,
                    Name = file.FileName,
                    Size = file.ContentLength,
                    Type = file.ContentType,
                    Url = Url.Action("Download", "File", new { id = f.Id }),
                    DeleteUrl = Url.Action("Delete", "File", new { id = f.Id }),
                    ThumbnailUrl = @"data:image/png;base64," + EncodeFile(fullPath),
                    DeleteType = "GET",
                });


            }
        }
    }
}