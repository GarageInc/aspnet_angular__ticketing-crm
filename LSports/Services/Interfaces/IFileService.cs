using LSports.Framework.Models.CustomClasses;

namespace LSports.Services.Interfaces
{
    public interface IFileService
    {
        File SaveFile(string fileName, System.IO.MemoryStream stream);
    }
}
