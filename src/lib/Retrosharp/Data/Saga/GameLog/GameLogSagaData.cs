using System;
using System.Collections.Generic;
using System.Text;

namespace Retrosharp.Data.Saga.GameLog
{
    public class GameLogSagaData : BaseSagaData
    {
        public int SeasonYear { get; set; }

        /// <summary>
        /// Number of records processed so far.
        /// This is used to track progress and determine when the saga is complete.
        /// </summary>
        public int RecordsProcessed { get; set; }
    }
}
