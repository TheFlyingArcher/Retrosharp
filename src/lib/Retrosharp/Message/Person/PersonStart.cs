namespace Retrosharp.Message.Person
{
    /// <summary>
    /// Starts a Person import: download Retrosheet's biographical-data archive, extract
    /// <c>biofile0.csv</c>, and upsert Person rows from it. Placed on the bus by
    /// <c>POST /api/person/import</c>. See spec/person.md and spec/retrosheet-auto-download.md.
    /// </summary>
    public class PersonStart : BaseMessage, IMessage
    {
        public PersonStart() { }
    }
}
