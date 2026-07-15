using System;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// Represents a single fielder's action on a play, linked to the specific runner it
    /// affected. A row only exists when tied to an actual out or error on that runner —
    /// a fielder's throw that results in neither (for example, a failed rundown) is not
    /// recorded. See spec/game-event.md, "Data Model" section.
    /// </summary>
    public class GameEventFieldingCredit : Entity
    {
        /// <summary>
        /// Foreign key to the game event (play).
        /// </summary>
        public int GameEventId { get; set; }

        /// <summary>
        /// Foreign key to the runner this fielding action pertains to.
        /// </summary>
        public int GameEventRunnerId { get; set; }

        /// <summary>
        /// Foreign key to the person (the fielder).
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// The type of fielding credit.
        /// </summary>
        public FieldingCreditType CreditType { get; set; }

        /// <summary>
        /// Order of this fielder's involvement in a relay (for example, an assist before a putout).
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// The fielding position (1-9) the person was playing at the moment of this credit,
        /// denormalized from the current lineup/substitution state at the time of the play.
        /// </summary>
        public byte Position { get; set; }
    }
}
