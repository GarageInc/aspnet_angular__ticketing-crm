using System.Linq;
using LSports.Framework.Models.CustomClasses;
using LSports.Framework.DataAccess.Repositories.Interfaces;

namespace LSports.Framework.DataAccess.Repositories
{
    public class FileRepository : IFileRepository
    {
        public void Delete(int id)
        {

        }

        public File GetFile(int id)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var q = (from file in model.tic_Files
                         where file.Id == id
                         select new File
                         {
                             Id = file.Id,
                             Name = file.Name,
                             Path = file.Path
                         }).FirstOrDefault();

                return q;
            }
        }

        public File Insert(File file)
        {
            using (var model = new gb_ts_stagingEntities())
            {
                var newRecord = new tic_Files()
                {
                    Name = file.Name,
                    Path = file.Path
                };

                model.tic_Files.Add(newRecord);
                model.SaveChanges();
                file.Id = newRecord.Id;

                return file;
            }
        }
    }
}
