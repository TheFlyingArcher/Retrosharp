using System;

namespace Retrosharp.Contract.GameEvent
{
    /// <summary>
    /// The type of fielding credit a person received on a specific play, in relation to a
    /// specific runner (see <see cref="GameEventRunner"/>).
    /// </summary>
    public enum FieldingCreditType
    {
        Putout,
        Assist,
        Error
    }
}
