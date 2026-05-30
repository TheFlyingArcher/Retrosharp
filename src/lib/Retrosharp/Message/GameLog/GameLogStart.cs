using System;
using System.Collections.Generic;
using System.Text;

namespace Retrosharp.Message.GameLog
{
    public class GameLogStart : BaseMessage, IMessage
    {
        public GameLogStart() { }

        /// <summary>
        /// The baseball season in which the game log is to be processed.
        /// </summary>
        public int SeasonYear { get; set; }

        /// <summary>
        /// The file path of the game log to be processed.
        /// </summary>
        public string FilePath { get; set; }
    }
}
