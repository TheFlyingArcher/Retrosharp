using System;
using System.Collections.Generic;
using System.Text;

namespace Retrosharp.Data
{
    public interface IBattingRepository : IRepository<Contract.Batting.Batting>
    {
        Task<IEnumerable<Contract.Batting.Batting>> GetByPersonId(int personId);

        Task<Contract.Batting.Batting> GetByPersonFranchiseSeason(int personId, int franchiseId, short seasonYear);
    }
}
