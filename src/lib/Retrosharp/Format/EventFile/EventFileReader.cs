using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using CsvHelper;
using CsvHelper.Configuration;

namespace Retrosharp.Format.EventFile
{
    /// <summary>
    /// Reads a Retrosheet play-by-play event file (.EVN/.EVA) into a sequence of
    /// <see cref="EventFileGame"/>s. Unlike <see cref="GameLog"/>/<see cref="BioFile"/>, an
    /// event file's records don't share one fixed column shape -- the first field is a
    /// record-type discriminator ("id", "info", "start", "play", "sub", "com", "data", or one
    /// of the "*adj" adjustment types), each with its own field count. This uses CsvHelper's
    /// low-level <see cref="CsvParser"/> to tokenize each line (quote-aware -- "com" records
    /// legitimately contain literal commas inside quoted text) rather than a fixed
    /// <c>ClassMap</c>, which only works for a single, uniform row shape.
    /// </summary>
    public static class EventFileReader
    {
        public static IEnumerable<EventFileGame> ReadGames(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The file at path '{filePath}' was not found.");

            // "BadDataFound = null" disables CsvHelper's strict RFC4180 check for content
            // trailing a closing quote before the next delimiter -- confirmed necessary
            // against a real "com" record in docs/csv/2025SEA.EVA
            // (com,"balls and strikes (Correa was in the on-deck circle)".), which has a
            // stray period after the closing quote. The field itself still parses correctly;
            // only the strict-mode warning is suppressed.
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { BadDataFound = null };
            using var reader = new StreamReader(filePath);
            using var parser = new CsvParser(reader, config);

            GameBuilder? current = null;

            while (parser.Read())
            {
                var row = parser.Record;
                if (row == null || row.Length == 0)
                    continue;

                var recordType = row[0];

                if (recordType == "id")
                {
                    if (current != null)
                        yield return current.Build(filePath);

                    current = new GameBuilder(row.Length > 1 ? row[1] : throw MissingField(filePath, "id"));
                    continue;
                }

                if (current == null)
                    throw new EventFileParseException(filePath, $"Encountered a '{recordType}' record before any 'id' record.");

                switch (recordType)
                {
                    case "version":
                        break;

                    case "info":
                        current.ApplyInfo(row, filePath);
                        break;

                    case "start":
                    {
                        var (retrosheetId, name, isHomeTeam, battingOrder, position) = ParseLineupFields(row, filePath);
                        current.Records.Add(new StartRecord
                        {
                            RetrosheetId = retrosheetId,
                            Name = name,
                            IsHomeTeam = isHomeTeam,
                            BattingOrder = battingOrder,
                            Position = position
                        });
                        break;
                    }

                    case "sub":
                    {
                        var (retrosheetId, name, isHomeTeam, battingOrder, position) = ParseLineupFields(row, filePath);
                        current.Records.Add(new SubRecord
                        {
                            RetrosheetId = retrosheetId,
                            Name = name,
                            IsHomeTeam = isHomeTeam,
                            BattingOrder = battingOrder,
                            Position = position
                        });
                        break;
                    }

                    case "play":
                        current.Records.Add(ParsePlayRecord(row, filePath));
                        break;

                    case "com":
                        current.Records.Add(new ComRecord { CommentText = RequireField(row, 1, filePath, "com") });
                        break;

                    case "data":
                        current.Records.Add(new DataRecord
                        {
                            DataType = RequireField(row, 1, filePath, "data"),
                            RetrosheetId = RequireField(row, 2, filePath, "data"),
                            Value = RequireField(row, 3, filePath, "data")
                        });
                        break;

                    case "badj":
                    case "padj":
                    case "ladj":
                    case "radj":
                    case "presadj":
                        current.Records.Add(new AdjustmentRecord
                        {
                            AdjustmentTypeCode = recordType,
                            RetrosheetId = RequireField(row, 1, filePath, recordType),
                            Value = RequireField(row, 2, filePath, recordType)
                        });
                        break;

                    default:
                        throw new EventFileParseException(filePath, $"Unrecognized event-file record type '{recordType}'.");
                }
            }

            if (current != null)
                yield return current.Build(filePath);
        }

        private static (string RetrosheetId, string Name, bool IsHomeTeam, byte BattingOrder, byte Position) ParseLineupFields(string[] row, string filePath)
        {
            return (
                RequireField(row, 1, filePath, "start/sub"),
                RequireField(row, 2, filePath, "start/sub"),
                RequireField(row, 3, filePath, "start/sub") == "1",
                byte.Parse(RequireField(row, 4, filePath, "start/sub"), CultureInfo.InvariantCulture),
                byte.Parse(RequireField(row, 5, filePath, "start/sub"), CultureInfo.InvariantCulture));
        }

        private static PlayRecord ParsePlayRecord(string[] row, string filePath)
        {
            return new PlayRecord
            {
                Inning = byte.Parse(RequireField(row, 1, filePath, "play"), CultureInfo.InvariantCulture),
                IsHomeTeamBatting = RequireField(row, 2, filePath, "play") == "1",
                RetrosheetId = RequireField(row, 3, filePath, "play"),
                CountField = RequireField(row, 4, filePath, "play"),
                PitchSequence = row.Length > 5 ? row[5] : string.Empty,
                RawEventText = RequireField(row, 6, filePath, "play")
            };
        }

        private static string RequireField(string[] row, int index, string filePath, string recordType)
        {
            if (row.Length <= index)
                throw new EventFileParseException(filePath, $"A '{recordType}' record is missing expected field {index}.");

            return row[index];
        }

        private static EventFileParseException MissingField(string filePath, string recordType) =>
            new(filePath, $"A '{recordType}' record is missing its value field.");

        private sealed class GameBuilder
        {
            public GameBuilder(string gameId)
            {
                GameId = gameId;
            }

            public string GameId { get; }

            public string? HomeTeamCode { get; private set; }

            public string? VisitingTeamCode { get; private set; }

            public DateTime? GameDate { get; private set; }

            public byte? GameNumber { get; private set; }

            public List<EventFileRecord> Records { get; } = new();

            public void ApplyInfo(string[] row, string filePath)
            {
                if (row.Length < 3)
                    return;

                switch (row[1])
                {
                    case "hometeam":
                        HomeTeamCode = row[2];
                        break;
                    case "visteam":
                        VisitingTeamCode = row[2];
                        break;
                    case "date":
                        GameDate = DateTime.ParseExact(row[2], "yyyy/MM/dd", CultureInfo.InvariantCulture);
                        break;
                    case "number":
                        GameNumber = byte.Parse(row[2], CultureInfo.InvariantCulture);
                        break;
                }
            }

            public EventFileGame Build(string filePath)
            {
                if (HomeTeamCode == null || VisitingTeamCode == null || GameDate == null || GameNumber == null)
                    throw new EventFileParseException(filePath,
                        $"Game '{GameId}' is missing one or more required 'info' records (hometeam/visteam/date/number).");

                return new EventFileGame
                {
                    GameId = GameId,
                    HomeTeamCode = HomeTeamCode,
                    VisitingTeamCode = VisitingTeamCode,
                    GameDate = GameDate.Value,
                    GameNumber = GameNumber.Value,
                    Records = Records
                };
            }
        }
    }
}
