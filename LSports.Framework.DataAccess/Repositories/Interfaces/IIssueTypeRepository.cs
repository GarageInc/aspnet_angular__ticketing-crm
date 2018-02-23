using System;
using System.Collections.Generic;
using System.Linq;
using LSports.Framework.Models.CustomClasses;

namespace LSports.Framework.DataAccess.Repositories.Interfaces
{
    public interface IIssueTypeRepository
    {
        IList<IssueType> GetList();


        IssueType Insert(IssueType issueType);


        IssueType Update(IssueType issueType);


        void Delete(int id);

        bool IsIssueTypeNameUnique(string name, int id);
    }
}