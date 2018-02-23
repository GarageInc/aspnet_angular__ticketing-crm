using System.IO;
using LSports.Framework.DataAccess.Repositories;
using LSports.Framework.DataAccess.Repositories.Interfaces;
using LSports.Services.Interfaces;
using File = LSports.Framework.Models.CustomClasses.File;

namespace LSports.Services
{
    public class FileService : IFileService
    {

        private readonly IFileRepository _fileRepository;

        public FileService() : this(new FileRepository())
        {

        }

        public FileService(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        public File SaveFile(string fileName, MemoryStream stream)
        {
            var filePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Files"), fileName);

            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                stream.WriteTo(file);
            }

            var fileData = _fileRepository.Insert(new File
            {
                Name = fileName,
                Path = ""
            });

            return fileData;
        }
    }
}
