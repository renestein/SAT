using System;
using System.Linq;
using System.Threading.Tasks;
using RSat.Core;

namespace RSat
{
  public class Sudoku
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
      var solver = new Sat(SimpleDPLLStrategy.Solve);
      addVariables(solver);
      addRowRule(solver);
      addColumnRule(solver);
      addBoxRule(solver);
      addOneValueInCelRule(solver);
      encodeSudokuGame(solver);
      if (solver.Solve())
      {
        Console.WriteLine(solver.FoundModel);
      }
      else
      {
        Console.WriteLine("No solution found!");
      }
    }

    private static void encodeSudokuGame(Sat solver)
    {
      solver.AddClausule(solver.GetVariable(getVariableName(0, 0, 8)));
      solver.AddClausule(solver.GetVariable(getVariableName(0, 1, 9)));
      solver.AddClausule(solver.GetVariable(getVariableName(0, 2, 4)));
      solver.AddClausule(solver.GetVariable(getVariableName(0, 7, 5)));
      solver.AddClausule(solver.GetVariable(getVariableName(0, 8, 1)));
      solver.AddClausule(solver.GetVariable(getVariableName(1, 2, 7)));
      solver.AddClausule(solver.GetVariable(getVariableName(1, 5, 3)));
      solver.AddClausule(solver.GetVariable(getVariableName(1, 7, 6)));
      solver.AddClausule(solver.GetVariable(getVariableName(1, 8, 9)));
      solver.AddClausule(solver.GetVariable(getVariableName(2, 1, 6)));
      solver.AddClausule(solver.GetVariable(getVariableName(2, 3, 5)));
      solver.AddClausule(solver.GetVariable(getVariableName(2, 5, 4)));
      solver.AddClausule(solver.GetVariable(getVariableName(3, 1, 3)));
      solver.AddClausule(solver.GetVariable(getVariableName(3, 2, 8)));
      solver.AddClausule(solver.GetVariable(getVariableName(3, 3, 4)));
      solver.AddClausule(solver.GetVariable(getVariableName(3, 4, 5)));
      solver.AddClausule(solver.GetVariable(getVariableName(3, 5, 1)));
      solver.AddClausule(solver.GetVariable(getVariableName(4, 0, 2)));
      solver.AddClausule(solver.GetVariable(getVariableName(4, 5, 6)));
      solver.AddClausule(solver.GetVariable(getVariableName(4, 6, 8)));
      solver.AddClausule(solver.GetVariable(getVariableName(4, 8, 5)));
      solver.AddClausule(solver.GetVariable(getVariableName(5, 0, 6)));
      solver.AddClausule(solver.GetVariable(getVariableName(5, 5, 2)));
      solver.AddClausule(solver.GetVariable(getVariableName(5, 6, 7)));
      solver.AddClausule(solver.GetVariable(getVariableName(6, 0, 3)));
      solver.AddClausule(solver.GetVariable(getVariableName(6, 1, 8)));
      solver.AddClausule(solver.GetVariable(getVariableName(6, 3, 1)));
      solver.AddClausule(solver.GetVariable(getVariableName(6, 4, 7)));
      solver.AddClausule(solver.GetVariable(getVariableName(6, 5, 5)));
      solver.AddClausule(solver.GetVariable(getVariableName(7, 0, 4)));
      solver.AddClausule(solver.GetVariable(getVariableName(7, 3, 3)));
      solver.AddClausule(solver.GetVariable(getVariableName(7, 4, 6)));
      solver.AddClausule(solver.GetVariable(getVariableName(7, 5, 9)));
      solver.AddClausule(solver.GetVariable(getVariableName(7, 6, 1)));
      solver.AddClausule(solver.GetVariable(getVariableName(7, 8, 8)));
      solver.AddClausule(solver.GetVariable(getVariableName(8, 1, 1)));
      solver.AddClausule(solver.GetVariable(getVariableName(8, 6, 5)));
      solver.AddClausule(solver.GetVariable(getVariableName(8, 7, 7)));
    }

    private static void addOneValueInCelRule(Sat solver)
    {
      var clausules = (from row in Enumerable.Range(0, ROWS)
                       from column in Enumerable.Range(0, COLUMNS)
                       from value in Enumerable.Range(1, NUMBER_OF_VALUES)
                       select new { row, column, value })
                  .GroupBy(triad => new { triad.row, triad.column })
                  .Select(grouping => grouping.Select(triad => solver.GetVariable(getVariableName(triad.row, triad.column, triad.value)))).ToArray();


      foreach (var clausule in clausules)
      {
        //At least one
        var literals = clausule.Select(variable => (Literal)variable).ToArray();
        solver.AddClausule(literals);
        //at most one
        var atMostOneClausules = clausule.Zip(clausule.Skip(1)).Select(pair => new[] { ~pair.First, ~pair.Second }).ToArray();
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
                      .GroupBy(triad => new { BoxR = triad.row / 3, BoxC = triad.column / 3 })
                      .Select(grouping =>
                                grouping.Select(triad =>
                                                  (Literal)solver.GetVariable(getVariableName(triad.row, triad.column,
                                                                                               triad.value))));

      foreach (var clausule in clausules)
      {
        solver.AddClausule(clausule.ToArray());
      }
    }

    private static void addColumnRule(Sat solver)
    {
      var clausules = (from column in Enumerable.Range(0, COLUMNS)
                       from value in Enumerable.Range(1, NUMBER_OF_VALUES)
                       from row in Enumerable.Range(0, ROWS)
                       select new { row, column, value })
                      .GroupBy(triad => triad.column)
                      .Select(grouping =>
                                grouping.Select(triad =>
                                                  (Literal)solver.GetVariable(getVariableName(triad.row, grouping.Key,
                                                                                               triad.value))));

      foreach (var clausule in clausules)
      {
        solver.AddClausule(clausule.ToArray());
      }
    }

    private static void addRowRule(Sat solver)
    {
      var clausules = (from row in Enumerable.Range(0, ROWS)
                       from column in Enumerable.Range(0, COLUMNS)
                       from value in Enumerable.Range(1, NUMBER_OF_VALUES)
                       select new { row, column, value })
                      .GroupBy(triad => triad.row)
                      .Select(grouping =>
                                grouping.Select(triad =>
                                                  (Literal)solver.GetVariable(getVariableName(grouping.Key,
                                                                                               triad.column,
                                                                                               triad.value))));

      foreach (var clausule in clausules)
      {
        solver.AddClausule(clausule.ToArray());
      }
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
  }
}