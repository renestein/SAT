using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RSat.Core;

namespace RSat.Sudoku
{
  public class SudokuEngine
  {
    private const int ROWS = 9;
    private const int COLUMNS = 9;
    private const int NUMBER_OF_VALUES = 9;
    private const int BOX_ROWS = 3;
    private const int BOX_COLUMNS = 3;
    private static readonly int[] BOX_ROWS_INDICES = { 0, 3, 6 };
    private static readonly int[] BOX_COLUMN_INDICES = { 0, 3, 6 };

    public static void Run()
    {
      var solver = prepareSolver();
      rawEncodeSudokuGame(solver);

      if (solver.Solve())
      {
        var modelValues = solver.FoundModel!.ModelValues.Where(mv => mv.IsTrue).OrderBy(mv => mv.Name);
        foreach (var modelValue in modelValues)
        {
          Console.WriteLine(modelValue.ToString());
        }
      }
      else
      {
        Console.WriteLine("No solution found!");
      }
    }

    public static void Run(SudokuBoard board)
    {
      const int ROW_INDEX = 0;
      const int COLUMN_INDEX = 1;
      const int VALUE_INDEX = 2;
      if (board == null)
      {
        throw new ArgumentNullException(nameof(board));
      }

      var solver = prepareSolver();
      Console.WriteLine("Solving board...");
      encodeBoardToSolverFormat(solver, board);
      dumpBoard(board);
      var stopWatch = new Stopwatch();
      stopWatch.Start();
      if (solver.Solve())
      {
        stopWatch.Stop();
        var modelValues = solver.FoundModel!.ModelValues;
        modelValuesToBoardValues(board, modelValues, ROW_INDEX, COLUMN_INDEX, VALUE_INDEX);

        Console.WriteLine($"Found solution! Elapsed time (ms): {stopWatch.ElapsedMilliseconds}");
        dumpBoard(board);
      }
      else
      {
        Console.WriteLine($"No solution found! Elapsed time (ms): {stopWatch.ElapsedMilliseconds})");
      }
    }

    public static void RunNotFunSudoku()
    {
      var board = new SudokuBoard
      {
        [0, 1] = new CellValue(2),

        [1, 3] = new CellValue(6),
        [1, 8] = new CellValue(3),

        [2, 1] = new CellValue(7),
        [2, 2] = new CellValue(4),
        [2, 4] = new CellValue(8),

        [3, 5] = new CellValue(3),
        [3, 8] = new CellValue(2),

        [4, 1] = new CellValue(8),
        [4, 4] = new CellValue(4),
        [4, 7] = new CellValue(1),

        [5, 0] = new CellValue(6),
        [5, 3] = new CellValue(5),

        [6, 4] = new CellValue(1),
        [6, 6] = new CellValue(7),
        [6, 7] = new CellValue(8),

        [7, 0] = new CellValue(5),
        [7, 5] = new CellValue(9),

        [8, 7] = new CellValue(4),

      };

      Run(board);
    }

    public static void RunSudoku1()
    {
      var board = new SudokuBoard
      {
        [0, 0] = new CellValue(2),
        [0, 3] = new CellValue(3),

        [1, 0] = new CellValue(8),
        [1, 2] = new CellValue(4),
        [1, 4] = new CellValue(6),
        [1, 5] = new CellValue(2),
        [1, 8] = new CellValue(3),

        [2, 1] = new CellValue(1),
        [2, 2] = new CellValue(3),
        [2, 3] = new CellValue(8),
        [2, 6] = new CellValue(2),

        [3, 4] = new CellValue(2),
        [3, 6] = new CellValue(3),
        [3, 7] = new CellValue(9),

        [4, 0] = new CellValue(5),
        [4, 2] = new CellValue(7),
        [4, 6] = new CellValue(6),
        [4, 7] = new CellValue(2),
        [4, 8] = new CellValue(1),

        [5, 1] = new CellValue(3),
        [5, 2] = new CellValue(2),
        [5, 5] = new CellValue(6),

        [6, 1] = new CellValue(2),
        [6, 5] = new CellValue(9),
        [6, 6] = new CellValue(1),
        [6, 7] = new CellValue(4),

        [7, 0] = new CellValue(6),
        [7, 2] = new CellValue(1),
        [7, 3] = new CellValue(2),
        [7, 4] = new CellValue(5),
        [7, 6] = new CellValue(8),
        [7, 8] = new CellValue(9),

        [8, 5] = new CellValue(1),
        [8, 8] = new CellValue(2)
      };

      Run(board);
    }

    private static void modelValuesToBoardValues(SudokuBoard board,
                                                 IEnumerable<ModelValue> modelValues,
                                                 int ROW_INDEX,
                                                 int COLUMN_INDEX,
                                                 int VALUE_INDEX)
    {
      foreach (var modelValue in modelValues)
      {
        if (modelValue.IsFalse)
        {
          continue;
        }

        var variableParts = getVariableParts(modelValue.Name);
        board[variableParts[ROW_INDEX], variableParts[COLUMN_INDEX]] = new CellValue(variableParts[VALUE_INDEX]);
      }
    }

    private static void dumpBoard(SudokuBoard board)
    {
      const char VERTICAL_SEPARATOR = '|';
      const string INDENT = "\t\t";
      var horizontalSeparator = INDENT + new string('–', (SudokuBoard.COLUMNS * 2) + 1);
      Console.WriteLine(horizontalSeparator);
      for (var i = 0; i < SudokuBoard.ROWS; i++)
      {

        var currentRow = INDENT + VERTICAL_SEPARATOR;

        for (var j = 0; j < SudokuBoard.COLUMNS; j++)
        {
          var cellValue = board[i, j];
          currentRow += cellValue == CellValue.Unknown
            ? $"N{VERTICAL_SEPARATOR}"
            : $"{cellValue.Value.ToString()}{VERTICAL_SEPARATOR}";
        }

        Console.WriteLine(currentRow);
        Console.WriteLine(horizontalSeparator);
      }
    }

    private static void encodeBoardToSolverFormat(Sat solver,
                                                  SudokuBoard board)
    {
      for (var i = 0; i < SudokuBoard.ROWS; i++)
      {
        for (var j = 0; j < SudokuBoard.COLUMNS; j++)
        {
          var cellValue = board[i, j];
          if (cellValue != CellValue.Unknown)
          {
            solver.AddClausule(solver.GetVariable(getVariableName(i, j, cellValue)));
          }
        }
      }
    }

    private static void rawEncodeSudokuGame(Sat solver)
    {
      //https://dingo.sbs.arizona.edu/~sandiway/sudoku/examples.html
      solver.AddClausule(solver.GetVariable(getVariableName(0, 3, 2)));
      solver.AddClausule(solver.GetVariable(getVariableName(0, 4, 6)));
      solver.AddClausule(solver.GetVariable(getVariableName(0, 6, 7)));
      solver.AddClausule(solver.GetVariable(getVariableName(0, 8, 1)));

      solver.AddClausule(solver.GetVariable(getVariableName(1, 0, 6)));
      solver.AddClausule(solver.GetVariable(getVariableName(1, 1, 8)));
      solver.AddClausule(solver.GetVariable(getVariableName(1, 4, 7)));
      solver.AddClausule(solver.GetVariable(getVariableName(1, 7, 9)));

      solver.AddClausule(solver.GetVariable(getVariableName(2, 0, 1)));
      solver.AddClausule(solver.GetVariable(getVariableName(2, 1, 9)));
      solver.AddClausule(solver.GetVariable(getVariableName(2, 5, 4)));
      solver.AddClausule(solver.GetVariable(getVariableName(2, 6, 5)));

      solver.AddClausule(solver.GetVariable(getVariableName(3, 0, 8)));
      solver.AddClausule(solver.GetVariable(getVariableName(3, 1, 2)));
      solver.AddClausule(solver.GetVariable(getVariableName(3, 3, 1)));
      solver.AddClausule(solver.GetVariable(getVariableName(3, 7, 4)));

      solver.AddClausule(solver.GetVariable(getVariableName(4, 2, 4)));
      solver.AddClausule(solver.GetVariable(getVariableName(4, 3, 6)));
      solver.AddClausule(solver.GetVariable(getVariableName(4, 5, 2)));
      solver.AddClausule(solver.GetVariable(getVariableName(4, 6, 9)));

      solver.AddClausule(solver.GetVariable(getVariableName(5, 1, 5)));
      solver.AddClausule(solver.GetVariable(getVariableName(5, 5, 3)));
      solver.AddClausule(solver.GetVariable(getVariableName(5, 7, 2)));
      solver.AddClausule(solver.GetVariable(getVariableName(5, 8, 8)));

      solver.AddClausule(solver.GetVariable(getVariableName(6, 2, 9)));
      solver.AddClausule(solver.GetVariable(getVariableName(6, 3, 3)));
      solver.AddClausule(solver.GetVariable(getVariableName(6, 7, 7)));
      solver.AddClausule(solver.GetVariable(getVariableName(6, 8, 4)));

      solver.AddClausule(solver.GetVariable(getVariableName(7, 1, 4)));
      solver.AddClausule(solver.GetVariable(getVariableName(7, 4, 5)));
      solver.AddClausule(solver.GetVariable(getVariableName(7, 7, 3)));
      solver.AddClausule(solver.GetVariable(getVariableName(7, 8, 6)));

      solver.AddClausule(solver.GetVariable(getVariableName(8, 0, 7)));
      solver.AddClausule(solver.GetVariable(getVariableName(8, 2, 3)));
      solver.AddClausule(solver.GetVariable(getVariableName(8, 4, 1)));
      solver.AddClausule(solver.GetVariable(getVariableName(8, 5, 8)));
    }

    private static Sat prepareSolver()
    {
      var solver = new Sat(SimpleDPLLStrategy.Solve);
      addVariables(solver);
      addRowRule(solver);
      addColumnRule(solver);
      addBoxRule(solver);
      addOneValueInCellRule(solver);
      return solver;
    }

    private static void addOneValueInCellRule(Sat solver)
    {
      var clausules = (from row in Enumerable.Range(0, ROWS)
                       from column in Enumerable.Range(0, COLUMNS)
                       from value in Enumerable.Range(1, NUMBER_OF_VALUES)
                       select new { row, column, value })
                      .GroupBy(triad => new { triad.row, triad.column })
                      .Select(grouping =>
                                grouping.Select(triad =>
                                                  solver.GetVariable(getVariableName(triad.row, triad.column,
                                                                                     triad.value)))).ToArray();


      exactlyOnce(solver, clausules);
    }

    private static void exactlyOnce(Sat solver,
                                    IEnumerable<IEnumerable<Variable>> clausules)
    {
      foreach (var clausule in clausules)
      {
        //At least one
        var literals = clausule.Select(variable => (Literal)variable).ToArray();
        solver.AddClausule(literals);
        //at most one
        var atMostOneClausules = (from var1 in clausule
                                  from var2 in clausule
                                  where var1.Name != var2.Name
                                  select new[] { ~var1, ~var2 }).ToArray();

        foreach (var atMostOneClausule in atMostOneClausules)
        {
          solver.AddClausule(atMostOneClausule);
        }
      }
    }

    private static void addBoxRule(Sat solver)
    {
      var clausules = (from value in Enumerable.Range(1, NUMBER_OF_VALUES)
                       from row in BOX_ROWS_INDICES
                       from column in BOX_COLUMN_INDICES
                       from relRow in Enumerable.Range(0, BOX_ROWS)
                       from relColumn in Enumerable.Range(0, BOX_COLUMNS)
                       select new { row = row + relRow, column = column + relColumn, value })
                      .GroupBy(triad => new { BoxR = triad.row / 3, BoxC = triad.column / 3, triad.value })
                      .Select(grouping =>
                                grouping.Select(triad =>
                                                  solver.GetVariable(getVariableName(triad.row, triad.column,
                                                                                     triad.value)))).ToArray();
      exactlyOnce(solver, clausules);
    }

    private static void addColumnRule(Sat solver)
    {
      var clausules = (from column in Enumerable.Range(0, COLUMNS)
                       from value in Enumerable.Range(1, NUMBER_OF_VALUES)
                       from row in Enumerable.Range(0, ROWS)
                       select new { row, column, value })
                      .GroupBy(triad => new { triad.column, triad.value })
                      .Select(grouping =>
                                grouping.Select(triad =>
                                                  solver.GetVariable(getVariableName(triad.row, grouping.Key.column,
                                                                                     grouping.Key.value)))).ToArray();

      exactlyOnce(solver, clausules);
    }

    private static void addRowRule(Sat solver)
    {
      var clausules = (from row in Enumerable.Range(0, ROWS)
                       from column in Enumerable.Range(0, COLUMNS)
                       from value in Enumerable.Range(1, NUMBER_OF_VALUES)
                       select new { row, column, value })
                      .GroupBy(triad => new { triad.row, triad.value })
                      .Select(grouping =>
                                grouping.Select(triad =>
                                                  solver.GetVariable(getVariableName(grouping.Key.row,
                                                                                     triad.column,
                                                                                     grouping.Key.value)))).ToArray();

      exactlyOnce(solver, clausules);
    }

    private static void addVariables(Sat solver)
    {
      var variableNames = from row in Enumerable.Range(0, ROWS)
                          from column in Enumerable.Range(0, COLUMNS)
                          from value in Enumerable.Range(1, NUMBER_OF_VALUES)
                          select getVariableName(row, column, value);

      foreach (var variableName in variableNames)
      {
        solver.CreateVariable(variableName);
      }
    }

    private static string getVariableName(in int row,
                                          in int column,
                                          in int value)
    {
      return $"{row}-{column}-{value}";
    }

    private static int[] getVariableParts(string variableName)
    {
      return variableName.Split('-')
                         .Select(int.Parse)
                         .ToArray();
    }
  }
}