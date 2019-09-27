using System;
using RSat.Core;

namespace RSat
{
  class Program
  {
    static void Main(string[] args)
    {
      runSimpleSolver();
      // Sudoku.Run();

      Console.ReadLine();
    }

    private static void runSimpleSolver()
    {
      var solver = new Sat();
      solver.CreateVariable("A");
      solver.CreateVariable("B");
      //solver.AddClausule(solver.GetVariable("A"),  ~solver.GetVariable("B"));
      //solver.AddClausule(~solver.GetVariable("A"),  solver.GetVariable("B"));
      solver.CreateVariable("C");
      solver.AddClausule(solver.GetVariable("A"), solver.GetVariable("B"), ~solver.GetVariable("A"));
      solver.AddClausule(solver.GetVariable("A"), solver.GetVariable("B"), ~solver.GetVariable("C"));
      if (solver.Solve())
      {
        Console.WriteLine("Model found!");
        Console.WriteLine(solver.FoundModel);
      }
      else
      {
        Console.WriteLine("No model found!");
      }

    }
  }
}
