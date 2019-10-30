using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RSatLib.Core;

namespace RSatLib.Dimacs
{
  public class DimacsParser : IDimacsParser
  {
    private const char COMMENT = 'c';
    private const string INVALID_FORMAT_ERROR = "Invalid format of the data.";
    private const string INVALID_FORMAT_EXPRESSION_ERROR = "Invalid format of the expression line.";
    private const string INVALID_FORMAT_CLAUSE_ERROR = "Invalid format of the clause {0}.";
    private static readonly char[] EMPTY_CHAR_ARRAY = new char[0];
    public static readonly IDimacsParser Default = new DimacsParser();

    public async Task<Sat> ParseCnf(Stream stream)
    {
      if (stream == null)
      {
        throw new ArgumentNullException(nameof(stream));
      }

      if (stream.Length == 0 || !stream.CanRead)
      {
        throw new ArgumentException(nameof(stream));
      }

      using var reader = new StreamReader(stream);
      var parseResult = await enumerateLines(reader)
                              .Where(line => !String.IsNullOrEmpty(line) && !line[0].Equals(COMMENT))
                              .AggregateAsync(new ParserState(new Sat(SimpleDPLLStrategy.Solve)),
                                              parseLine);
      parseResult.ThrowIfInvalidFormat();
      return parseResult.Sat;
    }

    private ParserState parseLine(ParserState state,
                                  string line) =>
      state.ParserStep switch
      {
        ParserStep.WaitingForExpressionFormat => parseExpressionFormat(state, line)
          .With(ParserStep.ExpectingFirstClause),
        ParserStep.ExpectingFirstClause => parseClause(state, line).With(ParserStep.ExpectingNextClause),
        ParserStep.ExpectingNextClause => parseClause(state, line),
        _ => throw new InvalidOperationException()
      };

    private ParserState parseClause(ParserState parserState,
                                    string line)
    {
      const char END_CHAR = '0';
      var literals = line.TrimEnd(END_CHAR).Split(EMPTY_CHAR_ARRAY, StringSplitOptions.RemoveEmptyEntries);
      if (literals.Length == 0)
      {
        return parserState;
      }

      var sat = parserState.Sat;
      var clause = literals.Select((stringLiteral,
                                    index) =>
                        {
                          if (!Int32.TryParse(stringLiteral, out var intLiteral) ||
                              intLiteral == 0)
                          {
                            throw new ArgumentException(String.Format(INVALID_FORMAT_CLAUSE_ERROR, line), "stream");
                          }

                          var variable = sat.GetVariable(Math.Abs(intLiteral).ToString());
                          return intLiteral > 0
                            ? variable
                            : ~variable;
                        }).ToArray();

      sat.AddClause(clause);
      return parserState.With(readClauses: parserState.ReadClauses + 1);
    }

    private ParserState parseExpressionFormat(ParserState parserState,
                                              string line)
    {
      //p FORMAT VARIABLES CLAUSES
      const string EXPECTED_LINE_PREFIX = "p";
      const string EXPECTED_FORMAT = "cnf";
      const int PREFIX_INDEX = 0;
      const int FORMAT_INDEX = 1;
      const int VARIABLES_INDEX = 2;
      const int CLAUSES_INDEX = 3;
      const int EXPECTED_PARTS_COUNT = 4;


      var parts = line.Split(EMPTY_CHAR_ARRAY, StringSplitOptions.RemoveEmptyEntries);
      if (parts.Length != EXPECTED_PARTS_COUNT ||
          !parts[PREFIX_INDEX].Equals(EXPECTED_LINE_PREFIX, StringComparison.OrdinalIgnoreCase) ||
          !parts[FORMAT_INDEX].Equals(EXPECTED_FORMAT, StringComparison.OrdinalIgnoreCase) ||
          !Int32.TryParse(parts[VARIABLES_INDEX], out var variablesCount) ||
          variablesCount <= 0 ||
          !Int32.TryParse(parts[CLAUSES_INDEX], out var clausesCount) ||
          clausesCount <= 0)
      {
        throw new ArgumentException(INVALID_FORMAT_EXPRESSION_ERROR, "stream");
      }

      var sat = parserState.Sat;
      createVariables(variablesCount, sat);
      return parserState.With(expectedClauses: clausesCount);
    }

    private static void createVariables(int variablesCount,
                                        Sat sat)
    {
      for (var i = 1; i <= variablesCount; i++)
      {
        sat.CreateVariable(i.ToString());
      }
    }


    private async IAsyncEnumerable<string> enumerateLines(StreamReader reader)
    {
      string? line;
      while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
      {
        yield return line.Trim();
      }
    }

    private enum ParserStep
    {
      None = 0,
      WaitingForExpressionFormat,
      ExpectingFirstClause,
      ExpectingNextClause
    }

    private struct ParserState
    {
      public ParserState(Sat sat)
      {
        Sat = sat ?? throw new ArgumentNullException(nameof(sat));
        ParserStep = ParserStep.WaitingForExpressionFormat;
        ExpectedClauses = 0;
        ReadClauses = 0;
      }

      public Sat Sat
      {
        get;
        private set;
      }

      public ParserStep ParserStep
      {
        get;
        private set;
      }

      public int ExpectedClauses
      {
        get;
        private set;
      }

      public int ReadClauses
      {
        get;
        private set;
      }

      public ParserState With(ParserStep? parserStep = null,
                              int? expectedClauses = null,
                              int? readClauses = null)
      {
        return new ParserState(Sat)
        {
          ParserStep = parserStep ?? ParserStep,
          ExpectedClauses = expectedClauses ?? ExpectedClauses,
          ReadClauses = readClauses ?? ReadClauses,
          Sat = Sat
        };
      }

      public void ThrowIfInvalidFormat()
      {
        if (ParserStep != ParserStep.ExpectingNextClause || ReadClauses != ExpectedClauses)
        {
          throw new ArgumentException(INVALID_FORMAT_ERROR, "stream");
        }
      }
    }
  }
}