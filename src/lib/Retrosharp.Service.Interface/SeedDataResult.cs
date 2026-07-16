using System;

namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Reports how many seed data records were added versus already present, per table.
    /// </summary>
    public class SeedDataResult
    {
        public int FranchisesAdded { get; set; }

        public int FranchisesSkipped { get; set; }

        public int BallparksAdded { get; set; }

        public int BallparksSkipped { get; set; }
    }
}
