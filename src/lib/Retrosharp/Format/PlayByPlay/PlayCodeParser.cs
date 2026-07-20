using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Retrosharp.Contract.GameEvent;

namespace Retrosharp.Format.PlayByPlay
{
    /// <summary>
    /// Parses one Retrosheet play-code string (for example, "S9/G34" or "64(1)3/GDP") into a
    /// <see cref="ParsedPlay"/>. Pure, no I/O -- see spec/game-event.md "Data Model" and
    /// spec/phase-1-build-plan.md Step 6a. Resolving fielder numbers and base occupants to
    /// actual Person rows happens downstream (Step 6b), once lineup/substitution state is
    /// available; this parser only knows "the runner who started on 2nd," not who that is.
    /// </summary>
    public static class PlayCodeParser
    {
        public static ParsedPlay Parse(string rawEventText, string countField, string pitchSequence)
        {
            if (string.IsNullOrWhiteSpace(rawEventText))
                throw new PlayCodeParseException(rawEventText ?? string.Empty, "Play code is empty.");

            var (balls, strikes) = ParseCount(rawEventText, countField);
            var fouls = CountFoulsWithTwoStrikes(pitchSequence);

            var dotIndex = rawEventText.IndexOf('.');
            var beforeAdvances = dotIndex >= 0 ? rawEventText[..dotIndex] : rawEventText;
            var advancesRaw = dotIndex >= 0 ? rawEventText[(dotIndex + 1)..] : null;

            // Paren-aware: a "/" can appear *inside* a parenthesized annotation (for example
            // "PO2(E1/TH)"), and must not be treated as a modifier separator there.
            var slashParts = SplitRespectingParens(beforeAdvances, '/');
            var primaryCode = slashParts[0];
            var modifiers = slashParts.Skip(1).ToList();

            var runners = new Dictionary<BaseState, MutableRunner>();
            var (eventType, isFieldedOutPendingTrajectory) = ParsePrimaryCode(primaryCode, rawEventText, runners);

            BattedBallType? battedBallType = null;
            var isSacHit = false;
            var isSacFly = false;
            ApplyModifiers(modifiers, ref battedBallType, ref isSacHit, ref isSacFly);

            if (isFieldedOutPendingTrajectory)
            {
                // A fielded out's EventType depends on trajectory, which isn't known until the
                // modifiers are parsed: a ground ball fielded and thrown out is a GroundOut,
                // while anything caught in the air (fly, line drive, pop up) is a FlyOut --
                // EventType alone doesn't distinguish those, BattedBallType does. Every fielded
                // out in the real reference files carried a trajectory modifier; a missing one
                // is treated as a data gap rather than silently guessed.
                eventType = battedBallType switch
                {
                    Contract.GameEvent.BattedBallType.GroundBall => GameEventType.GroundOut,
                    Contract.GameEvent.BattedBallType.LineDrive or Contract.GameEvent.BattedBallType.FlyBall or Contract.GameEvent.BattedBallType.PopUp => GameEventType.FlyOut,
                    _ => throw new PlayCodeParseException(rawEventText, "Fielded-out code has no trajectory modifier (G/L/F/P/BG/BP) to determine GroundOut vs FlyOut.")
                };
            }

            if (advancesRaw is { Length: > 0 })
            {
                foreach (var segment in SplitRespectingParens(advancesRaw, ';'))
                    ApplyAdvanceSegment(segment, rawEventText, runners);
            }

            return new ParsedPlay
            {
                EventType = eventType,
                BattedBallType = battedBallType,
                IsSacHit = isSacHit,
                IsSacFly = isSacFly,
                Balls = balls,
                Strikes = strikes,
                FoulBallsWithTwoStrikes = fouls,
                RawEventText = rawEventText,
                Runners = runners.Values.Select(r => r.ToParsedRunnerAdvance()).ToList()
            };
        }

        private static (byte Balls, byte Strikes) ParseCount(string rawEventText, string countField)
        {
            if (countField is not { Length: 2 } || !char.IsDigit(countField[0]) || !char.IsDigit(countField[1]))
                throw new PlayCodeParseException(rawEventText, $"Count field '{countField}' is not two digits.");

            return ((byte)(countField[0] - '0'), (byte)(countField[1] - '0'));
        }

        /// <summary>
        /// Balls/strikes come from the count field (Retrosheet's own final tally), not from
        /// re-scanning the pitch sequence -- but the count field alone can't tell foul balls
        /// with two strikes apart from any other pitch, since fouls after two strikes don't
        /// change the count. So this scans the pitch sequence purely for that one derived stat.
        /// </summary>
        private static byte CountFoulsWithTwoStrikes(string pitchSequence)
        {
            if (string.IsNullOrEmpty(pitchSequence))
                return 0;

            var strikes = 0;
            byte fouls = 0;

            foreach (var c in pitchSequence)
            {
                switch (c)
                {
                    // Strike-equivalent pitches: called/swinging strike, missed bunt, automatic
                    // strike (pitch-timer violation), foul tip, foul bunt, foul tip on bunt,
                    // swinging strike on a pitchout. Capped at 2 -- a third "strike" character
                    // appearing after 2 is itself a foul-with-two-strikes case below, or ends
                    // the plate appearance (K/X handled by the event code, not pitch scanning).
                    case 'C':
                    case 'S':
                    case 'M':
                    case 'A':
                    case 'T':
                    case 'L':
                    case 'O':
                    case 'K':
                    case 'Q':
                        strikes = Math.Min(strikes + 1, 2);
                        break;

                    // A plain foul ball (and its pitchout equivalent) while already at 2
                    // strikes -- exactly what this column tracks per spec/game-event.md.
                    case 'F':
                    case 'R':
                        if (strikes >= 2)
                            fouls++;
                        break;

                    // Balls, hit-into-play, no-pitch/pickoff markers, and prefix/separator
                    // characters (*, +, ., >) don't affect the strike count.
                    default:
                        break;
                }
            }

            return fouls;
        }

        private static void ApplyModifiers(IReadOnlyList<string> modifiers, ref BattedBallType? battedBallType, ref bool isSacHit, ref bool isSacFly)
        {
            foreach (var modifier in modifiers)
            {
                if (modifier.Length == 0)
                    continue;

                if (battedBallType is null)
                {
                    battedBallType = modifier[0] switch
                    {
                        'G' => Contract.GameEvent.BattedBallType.GroundBall,
                        'L' => Contract.GameEvent.BattedBallType.LineDrive,
                        'F' => Contract.GameEvent.BattedBallType.FlyBall,
                        'P' => Contract.GameEvent.BattedBallType.PopUp,
                        _ => null
                    };
                }

                if (modifier.StartsWith("BG", StringComparison.Ordinal))
                    battedBallType ??= Contract.GameEvent.BattedBallType.GroundBall;
                else if (modifier.StartsWith("BP", StringComparison.Ordinal))
                    battedBallType ??= Contract.GameEvent.BattedBallType.PopUp;

                if (modifier.StartsWith("SH", StringComparison.Ordinal))
                    isSacHit = true;
                else if (modifier.StartsWith("SF", StringComparison.Ordinal))
                    isSacFly = true;
            }
        }

        private sealed class MutableRunner
        {
            public required BaseState StartBase { get; init; }
            public BaseState EndBase { get; set; }
            public bool IsOut { get; set; }
            public bool IsRBI { get; set; }
            public bool IsEarnedRun { get; set; }
            public List<ParsedFieldingCredit> FieldingCredits { get; } = new();

            public ParsedRunnerAdvance ToParsedRunnerAdvance() => new()
            {
                StartBase = StartBase,
                EndBase = EndBase,
                IsOut = IsOut,
                IsRBI = IsRBI,
                IsEarnedRun = IsEarnedRun,
                FieldingCredits = FieldingCredits
            };
        }

        private static BaseState NextBase(BaseState startBase) => startBase switch
        {
            BaseState.BattersBox => BaseState.First,
            BaseState.First => BaseState.Second,
            BaseState.Second => BaseState.Third,
            BaseState.Third => BaseState.Home,
            _ => throw new ArgumentOutOfRangeException(nameof(startBase))
        };

        /// <summary>
        /// Parses a fielder-credit chain such as "643" (assist, assist, putout) or "4E6" (an
        /// embedded error -- "&lt;digit&gt;E&lt;digit&gt;" -- charging that fielder with an error
        /// instead of a putout/assist, the same convention as the primary code's mid-sequence
        /// "4E3" errors, but usable wherever a fielder chain appears: a runner-out advance
        /// annotation like "1X2(4E6)", a fielded-out group, or a PO/POCS/CS credit list.
        /// Confirmed necessary against a real play ("FC4/G34.2-3;1X2(4E6);B-1") where treating
        /// "4E6" as three raw digit characters produced a garbage Position from 'E' itself.
        /// </summary>
        private static List<ParsedFieldingCredit> ParseFielderChain(string chain, string rawEventText)
        {
            var tokens = new List<(byte Position, bool IsError)>();
            var i = 0;
            while (i < chain.Length)
            {
                if (chain[i] == 'E' && i + 1 < chain.Length && char.IsDigit(chain[i + 1]))
                {
                    tokens.Add(((byte)(chain[i + 1] - '0'), true));
                    i += 2;
                }
                else if (char.IsDigit(chain[i]))
                {
                    tokens.Add(((byte)(chain[i] - '0'), false));
                    i++;
                }
                else
                {
                    throw new PlayCodeParseException(rawEventText, $"Unexpected character '{chain[i]}' in fielder chain '{chain}'.");
                }
            }

            var credits = new List<ParsedFieldingCredit>(tokens.Count);
            for (var index = 0; index < tokens.Count; index++)
            {
                var (position, isError) = tokens[index];
                var creditType = isError
                    ? FieldingCreditType.Error
                    : index == tokens.Count - 1 ? FieldingCreditType.Putout : FieldingCreditType.Assist;

                credits.Add(new ParsedFieldingCredit { Position = position, CreditType = creditType, Sequence = index + 1 });
            }

            return credits;
        }

        private static MutableRunner GetOrAddRunner(IDictionary<BaseState, MutableRunner> runners, BaseState startBase)
        {
            if (!runners.TryGetValue(startBase, out var runner))
            {
                runner = new MutableRunner { StartBase = startBase };
                runners[startBase] = runner;
            }

            return runner;
        }

        private static BaseState ParseBaseToken(char token, string rawEventText) => token switch
        {
            'B' => BaseState.BattersBox,
            '1' => BaseState.First,
            '2' => BaseState.Second,
            '3' => BaseState.Third,
            'H' => BaseState.Home,
            _ => throw new PlayCodeParseException(rawEventText, $"Unrecognized base token '{token}'.")
        };

        // ---------------------------------------------------------------------------------
        // Primary code dispatch
        // ---------------------------------------------------------------------------------

        /// <summary>
        /// Parses the primary code segment (everything before the first "/" or "."), adding
        /// runner entries for whatever the primary code itself implies (the batter's default
        /// fate for a batted/pitched event, or the runner(s) explicitly named for a
        /// baserunning-only event like a stolen base). Returns the overall <see cref="GameEventType"/>.
        /// </summary>
        private static (GameEventType EventType, bool IsFieldedOutPendingTrajectory) ParsePrimaryCode(string primaryCode, string rawEventText, IDictionary<BaseState, MutableRunner> runners)
        {
            // "K+CS2(24)" / "K+PB" / "W+SB3" -- a secondary baserunning event bundled onto the
            // primary code within the same plate appearance. The left side determines the
            // overall EventType; the right side contributes its own runner(s) exactly as if it
            // were its own standalone code.
            var plusIndex = primaryCode.IndexOf('+');
            if (plusIndex >= 0)
            {
                var left = primaryCode[..plusIndex];
                var right = primaryCode[(plusIndex + 1)..];
                var result = ParseSingleCode(left, rawEventText, runners);
                ParseSingleCode(right, rawEventText, runners);
                return result;
            }

            // "SB2;SBH" -- simultaneous multiple steals on one play. Only observed joining
            // multiple SB codes together (a double/triple steal); not a general combinator.
            if (primaryCode.Contains(';'))
            {
                (GameEventType EventType, bool IsFieldedOutPendingTrajectory)? result = null;
                foreach (var subCode in primaryCode.Split(';'))
                {
                    // Every sub-code must actually run -- each contributes its own runner(s) to
                    // "runners" as a side effect. Using "??=" directly on the call would
                    // short-circuit and skip calling ParseSingleCode entirely for every
                    // sub-code after the first, silently dropping the rest of a multi-runner
                    // steal (confirmed against a real double steal, "SB3;SB2", in
                    // docs/csv/2025SDN.EVN).
                    var subResult = ParseSingleCode(subCode, rawEventText, runners);
                    result ??= subResult;
                }

                return result!.Value;
            }

            return ParseSingleCode(primaryCode, rawEventText, runners);
        }

        private static (GameEventType EventType, bool IsFieldedOutPendingTrajectory) ParseSingleCode(string code, string rawEventText, IDictionary<BaseState, MutableRunner> runners)
        {
            if (code.Length == 0)
                throw new PlayCodeParseException(rawEventText, "Empty primary code segment.");

            // Fielded out(s): a leading digit names the putout chain. Optional "(n)" groups
            // name each non-batter runner retired on the play by their starting base; any
            // trailing digits left unassigned to a "(n)" group are the batter's own putout.
            // The GroundOut/FlyOut distinction depends on the trajectory modifier, parsed
            // later -- see the pending-trajectory handling in Parse().
            if (char.IsDigit(code[0]))
            {
                var hadError = ParseFieldedOutGroups(code, rawEventText, runners);

                // "4E3"-style codes (an error partway through the sequence, e.g. an
                // errant relay) mean nobody was actually put out -- the notable outcome is
                // the error, not a ground/fly out, and trajectory is irrelevant to it.
                return hadError ? (GameEventType.Error, false) : (GameEventType.GroundOut, true);
            }

            if (code.StartsWith("FLE", StringComparison.Ordinal) && code.Length > 3 && char.IsDigit(code[3]))
            {
                // A foul ball dropped for an error (e.g. "FLE5") -- the plate appearance
                // continues, so unlike a bare "E<n>" the batter never becomes a runner at all;
                // there's no base-state change to record, just the error itself.
                return (GameEventType.Error, false);
            }

            if (code.StartsWith("HR", StringComparison.Ordinal))
            {
                AddBatterRunner(runners, BaseState.Home, isOut: false);
                return (GameEventType.HomeRun, false);
            }

            if (code.StartsWith("HP", StringComparison.Ordinal))
            {
                AddBatterRunner(runners, BaseState.First, isOut: false);
                return (GameEventType.HitByPitch, false);
            }

            if (code.Length > 0 && code[0] == 'H' && code != "HP")
            {
                // Bare "H" for home run, per spec/game-event.md's "HR or H".
                AddBatterRunner(runners, BaseState.Home, isOut: false);
                return (GameEventType.HomeRun, false);
            }

            if (code.StartsWith("S", StringComparison.Ordinal) && !code.StartsWith("SB", StringComparison.Ordinal) && !code.StartsWith("SH", StringComparison.Ordinal))
            {
                AddBatterRunner(runners, BaseState.First, isOut: false);
                return (GameEventType.Single, false);
            }

            if (code.StartsWith("D", StringComparison.Ordinal) && !code.StartsWith("DI", StringComparison.Ordinal))
            {
                AddBatterRunner(runners, BaseState.Second, isOut: false);
                return (GameEventType.Double, false);
            }

            if (code.StartsWith("T", StringComparison.Ordinal) && !code.StartsWith("TH", StringComparison.Ordinal))
            {
                AddBatterRunner(runners, BaseState.Third, isOut: false);
                return (GameEventType.Triple, false);
            }

            if (code.StartsWith("IW", StringComparison.Ordinal) || code == "I")
            {
                AddBatterRunner(runners, BaseState.First, isOut: false);
                return (GameEventType.IntentionalWalk, false);
            }

            if (code == "WP")
                return (GameEventType.WildPitch, false);

            if (code.StartsWith("W", StringComparison.Ordinal))
            {
                AddBatterRunner(runners, BaseState.First, isOut: false);
                return (GameEventType.Walk, false);
            }

            if (code.StartsWith("K", StringComparison.Ordinal))
            {
                AddBatterRunner(runners, BaseState.First, isOut: true);
                return (GameEventType.Strikeout, false);
            }

            if (code.StartsWith("E", StringComparison.Ordinal) && code.Length > 1 && char.IsDigit(code[1]))
            {
                var runner = AddBatterRunner(runners, BaseState.First, isOut: false);
                runner.FieldingCredits.Add(new ParsedFieldingCredit
                {
                    Position = (byte)(code[1] - '0'),
                    CreditType = FieldingCreditType.Error,
                    Sequence = 1
                });
                return (GameEventType.Error, false);
            }

            if (code.StartsWith("FC", StringComparison.Ordinal))
            {
                AddBatterRunner(runners, BaseState.First, isOut: false);
                return (GameEventType.FieldersChoice, false);
            }

            if (code.StartsWith("POCS", StringComparison.Ordinal))
            {
                ParseCaughtStealingLike(code, "POCS", rawEventText, runners);
                return (GameEventType.PickoffCaughtStealing, false);
            }

            if (code.StartsWith("CS", StringComparison.Ordinal))
            {
                ParseCaughtStealingLike(code, "CS", rawEventText, runners);
                return (GameEventType.CaughtStealing, false);
            }

            if (code.StartsWith("PO", StringComparison.Ordinal))
            {
                // Picked off while holding the base (not attempting to advance) -- out "at"
                // the same base they started on, unless the parenthetical is an error
                // annotation ("E<n>", for example "PO2(E1/TH)" -- a throwing error on the
                // pickoff attempt), in which case the runner is safe, not out. This is a
                // different parenthetical grammar than a fielded-out's "(<fielders>)" chain --
                // treating "E1/TH" as a raw fielder-digit chain (as opposed to a structured
                // error annotation) produced garbage Position values from non-digit
                // characters ('E', '/', 'T', 'H'), confirmed against a real occurrence in
                // docs/csv/2025SDN.EVN; PO<base> was flagged in Step 6a's own notes as
                // implemented but never exercised against real data.
                var baseChar = code[2];
                var startBase = ParseBaseToken(baseChar, rawEventText);
                var runner = GetOrAddRunner(runners, startBase);
                var parenStart = code.IndexOf('(');
                if (parenStart >= 0)
                {
                    var parenEnd = code.IndexOf(')', parenStart);
                    var annotation = code[(parenStart + 1)..parenEnd];

                    if (annotation.Length >= 2 && annotation[0] == 'E' && char.IsDigit(annotation[1]))
                    {
                        runner.EndBase = startBase;
                        runner.IsOut = false;
                        runner.FieldingCredits.Add(new ParsedFieldingCredit
                        {
                            Position = (byte)(annotation[1] - '0'),
                            CreditType = FieldingCreditType.Error,
                            Sequence = 1
                        });
                    }
                    else
                    {
                        runner.EndBase = startBase;
                        runner.IsOut = true;
                        runner.FieldingCredits.AddRange(ParseFielderChain(annotation, rawEventText));
                    }
                }
                else
                {
                    runner.EndBase = startBase;
                    runner.IsOut = true;
                }

                return (GameEventType.Pickoff, false);
            }

            if (code.StartsWith("SB", StringComparison.Ordinal))
            {
                var startBase = code[2] switch
                {
                    '2' => BaseState.First,
                    '3' => BaseState.Second,
                    'H' => BaseState.Third,
                    _ => throw new PlayCodeParseException(rawEventText, $"Unrecognized stolen-base target '{code}'.")
                };
                var runner = GetOrAddRunner(runners, startBase);
                runner.EndBase = NextBase(startBase);
                runner.IsOut = false;
                return (GameEventType.StolenBase, false);
            }

            if (code == "PB")
                return (GameEventType.PassedBall, false);

            if (code == "BK")
                return (GameEventType.Balk, false);

            if (code == "DI")
                return (GameEventType.DefensiveIndifference, false);

            if (code == "OA")
                return (GameEventType.OtherAdvance, false);

            if (code == "NP")
                return (GameEventType.NoPlay, false);

            if (code == "C")
            {
                // Catcher's interference -- batter awarded first. Not observed in the real
                // reference files; implemented per Retrosheet's documented convention.
                AddBatterRunner(runners, BaseState.First, isOut: false);
                return (GameEventType.CatcherInterference, false);
            }

            throw new PlayCodeParseException(rawEventText, $"Unrecognized primary code '{code}'.");
        }

        /// <summary>
        /// A runner out attempting to steal or on a pickoff throw during a steal attempt
        /// shares the same "<prefix><base>(fielders)" shape for both CS and POCS.
        /// </summary>
        private static void ParseCaughtStealingLike(string code, string prefix, string rawEventText, IDictionary<BaseState, MutableRunner> runners)
        {
            var rest = code[prefix.Length..];
            var targetBase = rest[0] switch
            {
                '2' => BaseState.Second,
                '3' => BaseState.Third,
                'H' => BaseState.Home,
                _ => throw new PlayCodeParseException(rawEventText, $"Unrecognized caught-stealing target in '{code}'.")
            };
            var startBase = targetBase switch
            {
                BaseState.Second => BaseState.First,
                BaseState.Third => BaseState.Second,
                BaseState.Home => BaseState.Third,
                _ => throw new PlayCodeParseException(rawEventText, $"Unrecognized caught-stealing target in '{code}'.")
            };

            var runner = GetOrAddRunner(runners, startBase);
            runner.EndBase = targetBase;
            runner.IsOut = true;

            var parenStart = rest.IndexOf('(');
            if (parenStart < 0)
                throw new PlayCodeParseException(rawEventText, $"Caught-stealing code '{code}' is missing its fielder chain.");

            var parenEnd = rest.IndexOf(')', parenStart);
            runner.FieldingCredits.AddRange(ParseFielderChain(rest[(parenStart + 1)..parenEnd], rawEventText));
        }

        /// <summary>
        /// Adds (or updates) the batter's own runner entry for whatever the primary code
        /// implies by default -- overridden later if the advance section explicitly gives a
        /// "B-x"/"BXx" segment.
        /// </summary>
        private static MutableRunner AddBatterRunner(IDictionary<BaseState, MutableRunner> runners, BaseState endBase, bool isOut)
        {
            var runner = GetOrAddRunner(runners, BaseState.BattersBox);
            runner.EndBase = endBase;
            runner.IsOut = isOut;
            return runner;
        }

        /// <summary>
        /// Parses a fielded-out primary code's digit/parenthetical groups, e.g. "16(1)3" --
        /// fielders 1 then 6 retire the runner who started at 1st (parenthetical), and the
        /// trailing unassigned "3" retires the batter. Every out here is a force: the runner
        /// (batter or otherwise) is out at the next base up from where they started, matching
        /// every example in spec/game-event.md's Data Model section.
        /// </summary>
        private static bool ParseFieldedOutGroups(string code, string rawEventText, IDictionary<BaseState, MutableRunner> runners)
        {
            var current = new StringBuilder();
            var hadError = false;
            var i = 0;
            while (i < code.Length)
            {
                var c = code[i];
                if (char.IsDigit(c))
                {
                    current.Append(c);
                    i++;
                }
                else if (c == '(')
                {
                    var close = code.IndexOf(')', i);
                    if (close < 0)
                        throw new PlayCodeParseException(rawEventText, $"Unbalanced '(' in fielded-out code '{code}'.");

                    var runnerStartBase = ParseBaseToken(code[i + 1], rawEventText);
                    AssignFieldedOutGroup(runners, runnerStartBase, current.ToString(), rawEventText);
                    current.Clear();
                    i = close + 1;
                }
                else if (c == 'E' && i + 1 < code.Length && char.IsDigit(code[i + 1]))
                {
                    // An error partway through a fielding sequence, e.g. "4E3" -- fielder 4
                    // assists, fielder 3 is charged the error instead of recording a putout, so
                    // the runner (the batter, in every real example) reaches safely rather than
                    // being put out.
                    AssignFieldedOutErrorGroup(runners, current.ToString(), code[i + 1], rawEventText);
                    current.Clear();
                    hadError = true;
                    i += 2;
                }
                else
                {
                    // Not part of the digit/parenthetical grammar (e.g. this "code" actually
                    // started with a letter and was misrouted here) -- shouldn't happen since
                    // callers only reach this method when code[0] is a digit, but keep this a
                    // loud failure rather than silently dropping trailing characters.
                    throw new PlayCodeParseException(rawEventText, $"Unexpected character '{c}' in fielded-out code '{code}'.");
                }
            }

            if (current.Length > 0)
                AssignFieldedOutGroup(runners, BaseState.BattersBox, current.ToString(), rawEventText);

            return hadError;
        }

        private static void AssignFieldedOutGroup(IDictionary<BaseState, MutableRunner> runners, BaseState startBase, string digits, string rawEventText)
        {
            if (digits.Length == 0)
                throw new PlayCodeParseException(rawEventText, "Fielded-out group has no fielder digits.");

            var runner = GetOrAddRunner(runners, startBase);
            runner.EndBase = NextBase(startBase);
            runner.IsOut = true;
            runner.FieldingCredits.AddRange(ParseFielderChain(digits, rawEventText));
        }

        private static void AssignFieldedOutErrorGroup(IDictionary<BaseState, MutableRunner> runners, string assistDigits, char errorFielderDigit, string rawEventText)
        {
            var runner = GetOrAddRunner(runners, BaseState.BattersBox);
            runner.EndBase = BaseState.First;
            runner.IsOut = false;

            var sequence = 1;
            foreach (var digit in assistDigits)
            {
                runner.FieldingCredits.Add(new ParsedFieldingCredit
                {
                    Position = (byte)(digit - '0'),
                    CreditType = FieldingCreditType.Assist,
                    Sequence = sequence++
                });
            }

            runner.FieldingCredits.Add(new ParsedFieldingCredit
            {
                Position = (byte)(errorFielderDigit - '0'),
                CreditType = FieldingCreditType.Error,
                Sequence = sequence
            });
        }

        // ---------------------------------------------------------------------------------
        // Advance segment parsing (the "." section, ";"-separated)
        // ---------------------------------------------------------------------------------

        /// <summary>
        /// Parses one "&lt;start&gt;-&lt;end&gt;" (safe) or "&lt;start&gt;X&lt;end&gt;(fielders)" (out)
        /// advance segment, including trailing "(...)" annotation groups: a fielder chain for
        /// an out, "(E&lt;n&gt;...)" for an error charged during a safe advance, "(NR)"/"(NORBI)" to
        /// deny an otherwise-implied RBI, and "(UR)" to mark an otherwise-implied earned run as
        /// unearned. Explicit segments always override whatever the primary code implied by
        /// default for that runner.
        /// </summary>
        private static void ApplyAdvanceSegment(string segment, string rawEventText, IDictionary<BaseState, MutableRunner> runners)
        {
            if (segment.Length == 0)
                return;

            var parenStart = segment.IndexOf('(');
            var core = parenStart >= 0 ? segment[..parenStart] : segment;
            var annotations = parenStart >= 0 ? ExtractParenGroups(segment[parenStart..]) : new List<string>();

            var isOut = core.Contains('X');
            var separatorIndex = isOut ? core.IndexOf('X') : core.IndexOf('-');
            if (separatorIndex < 0)
                throw new PlayCodeParseException(rawEventText, $"Advance segment '{segment}' has no '-' or 'X' separator.");

            var startBase = ParseBaseToken(core[0], rawEventText);
            var endBase = ParseBaseToken(core[separatorIndex + 1], rawEventText);

            var runner = GetOrAddRunner(runners, startBase);
            runner.EndBase = endBase;
            runner.IsOut = isOut;

            if (isOut)
            {
                var fielderGroup = annotations.FirstOrDefault(a => a.Length > 0 && char.IsDigit(a[0]));
                if (fielderGroup is null)
                    throw new PlayCodeParseException(rawEventText, $"Out advance '{segment}' is missing its fielder chain.");

                runner.FieldingCredits.Clear();
                runner.FieldingCredits.AddRange(ParseFielderChain(fielderGroup, rawEventText));

                // "1X2(4E6)" -- an "X" advance whose fielder chain's last credit is an error
                // rather than a putout means the throw that would have completed the out was
                // itself misplayed, so the runner is actually safe. Confirmed against a real
                // play ("FC4/G34.2-3;1X2(4E6);B-1"): treating this runner as out would leave
                // the half-inning with a 3rd out mid-sequence, yet the same real file's next
                // play in that same half-inning has the following batter still coming to the
                // plate -- an impossible fourth out. The base itself ("2") is still correct;
                // only whether they're out changes.
                if (runner.FieldingCredits.Count > 0 && runner.FieldingCredits[^1].CreditType == FieldingCreditType.Error)
                    runner.IsOut = false;
            }
            else if (endBase == BaseState.Home)
            {
                // A scored run is an RBI and earned by default; only an explicit annotation
                // denies either -- confirmed empirically against the real reference files,
                // where "(RBI)" itself never appears but "(NR)"/"(UR)" do.
                runner.IsRBI = true;
                runner.IsEarnedRun = true;
            }

            foreach (var annotation in annotations)
            {
                if (annotation.Length >= 2 && annotation[0] == 'E' && char.IsDigit(annotation[1]))
                {
                    var slash = annotation.IndexOf('/');
                    var fielderDigits = slash >= 0 ? annotation[1..slash] : annotation[1..];
                    foreach (var fielderChar in fielderDigits)
                    {
                        if (!char.IsDigit(fielderChar))
                            break;

                        runner.FieldingCredits.Add(new ParsedFieldingCredit
                        {
                            Position = (byte)(fielderChar - '0'),
                            CreditType = FieldingCreditType.Error,
                            Sequence = runner.FieldingCredits.Count + 1
                        });
                    }
                }
                else if (annotation is "NR" or "NORBI")
                {
                    runner.IsRBI = false;
                }
                else if (annotation is "UR" or "TUR")
                {
                    // "TUR" (team unearned run) distinguishes team-charged from
                    // individually-charged unearned runs; the schema doesn't need that
                    // distinction, so both map to the same IsEarnedRun = false.
                    runner.IsEarnedRun = false;
                }
                else if (annotation.Length > 0 && char.IsDigit(annotation[0]))
                {
                    // Already consumed as the out fielder chain above.
                }
                else
                {
                    throw new PlayCodeParseException(rawEventText, $"Unrecognized advance annotation '({annotation})' in '{segment}'.");
                }
            }
        }

        /// <summary>
        /// Splits on <paramref name="delimiter"/>, ignoring any occurrence inside "(...)" --
        /// needed because annotations like "(E1/TH)" can contain the same characters used to
        /// separate modifiers ("/") or advances (";") elsewhere in the code.
        /// </summary>
        private static List<string> SplitRespectingParens(string text, char delimiter)
        {
            var parts = new List<string>();
            var current = new StringBuilder();
            var depth = 0;

            foreach (var c in text)
            {
                if (c == '(')
                    depth++;
                else if (c == ')')
                    depth--;

                if (c == delimiter && depth == 0)
                {
                    parts.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            parts.Add(current.ToString());
            return parts;
        }

        private static List<string> ExtractParenGroups(string text)
        {
            var groups = new List<string>();
            var i = 0;
            while (i < text.Length)
            {
                if (text[i] != '(')
                {
                    i++;
                    continue;
                }

                var close = text.IndexOf(')', i);
                if (close < 0)
                    throw new PlayCodeParseException(text, $"Unbalanced '(' in '{text}'.");

                groups.Add(text[(i + 1)..close]);
                i = close + 1;
            }

            return groups;
        }
    }
}
