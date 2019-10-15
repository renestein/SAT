using System;
using System.Collections.Generic;
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
      var solver = new Sat(SimpleDPLLStrategy.Solve);
      addVariables(solver);
      addRowRule(solver);
      addColumnRule(solver);
      addBoxRule(solver);
      addOneValueInCellRule(solver);
      encodeSudokuGame(solver);

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

    private static void encodeSudokuGame(Sat solver)
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

    private static void addOneValueInCellRule(Sat solver)
    {
      var clausules = (from row in Enumerable.Range(0, ROWS)
                       from column in Enumerable.Range(0, COLUMNS)
                       from value in Enumerable.Range(1, NUMBER_OF_VALUES)
                       select new { row, column, value })
                  .GroupBy(triad => new { triad.row, triad.column })
                  .Select(grouping => grouping.Select(triad => solver.GetVariable(getVariableName(triad.row, triad.column, triad.value)))).ToArray();


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
  }
}