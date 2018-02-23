using System.Collections.Generic;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Framework.DataAccess.Repositories.Interfaces
{
    public interface IFileRepository
{
        File GetFile(int id);
        File Insert(File file);
        //File Update(File file);
        void Delete(int id);
    }
}
