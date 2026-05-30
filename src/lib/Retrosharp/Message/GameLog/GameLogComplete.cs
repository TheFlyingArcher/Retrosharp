using System;
using System.Collections.Generic;
using System.Text;

namespace Retrosharp.Message.GameLog
{
    public class GameLogComplete :BaseMessage, IMessage
    {
        public GameLogComplete() { }

        /// <summary>
        /// The baseball season in which the game log was processed.
        /// </summary>
        public int SeasonYear { get; set; }

        /// <summary>
        /// Number of records processed in the game log.
        /// This can be used for logging, auditing, or triggering subsequent actions based on the volume of data processed.
        /// </summary>
        public int RecordsProcessed { get; set; }
    }
}
