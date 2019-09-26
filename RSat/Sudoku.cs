using System.Linq;
using RSat.Core;

namespace RSat
{
  public class Sudoku
  {
    private const int ROWS = 9;
    private const int COLUMNS = 9;
    private const int VALUES = 9;

    public static void Run()
    {
      var solver = new Sat();
      addVariables(solver);
    }

    private static void addVariables(Sat solver)
    {
      var variableNames = from row in Enumerable.Range(0, ROWS)
        from column in Enumerable.Range(0, COLUMNS)
        from value in Enumerable.Range(0, VALUES)
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