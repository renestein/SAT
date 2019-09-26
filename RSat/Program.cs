using System;
using RSat.Core;

namespace RSat
{
  class Program
  {
    static void Main(string[] args)
    {
      //runSimpleSolver();
      Sudoku.Run();
      
      Console.ReadLine();
    }

    private static void runSimpleSolver()
    {
      var solver = new Sat();
      solver.CreateVariable("A");
      solver.CreateVariable("B");
      solver.CreateVariable("C");
      solver.AddLiterals(solver.GetVariable("A"), solver.GetVariable("B"), ~solver.GetVariable("A"));
      solver.AddLiterals(solver.GetVariable("A"), solver.GetVariable("B"), ~solver.GetVariable("C"));
      if (solver.Solve())
      {
        foreach (var model in solver.FoundModels)
        {
          Console.WriteLine(model);
        }
      }
    }
  }
}
