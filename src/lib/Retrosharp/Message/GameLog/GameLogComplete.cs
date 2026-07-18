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
        /// Number of games added to the database.
        /// </summary>
        public int GamesAdded { get; set; }

        /// <summary>
        /// Number of games skipped because they were already present, matched by natural key.
        /// </summary>
        public int GamesSkipped { get; set; }
    }
}
