using System;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using RSat.Core;
using RSat.Sudoku;

namespace RSat
{
  class Program
  {
    static void Main(string[] args)
    {
      Trace.Listeners.Add(new ConsoleTraceListener());
      //runSimpleSolver();
      //SudokuEngine.Run();
      //SudokuEngine.RunSudoku1();
      //SudokuEngine.RunNotFunSudoku();
      //SudokuEngine.RunMisaSudoku();
      //solveMoreFormulas();
      solveHard250Sample().Wait();

      Console.ReadLine();
    }

    private static async Task solveHard250Sample()
    {
      var sat = await Sat.FromFile(Path.Combine("../../../../", "RSat.Test/DIMACS_Samples/hard/sat2.cnf"))
                         .ConfigureAwait(false);
      if (sat.Solve())
      {
        Console.WriteLine(sat.FoundModel);
      }
    }

    private static void solveMoreFormulas()
    {
      var solver = new Sat(SimpleDPLLStrategy.Solve);
      solver.CreateVariable("A");
      solver.CreateVariable("B");
      solver.CreateVariable("C");
      solver.CreateVariable("D");
      solver.CreateVariable("E");
      solver.CreateVariable("F");
      solver.AddClause(~solver.GetVariable("A"), solver.GetVariable("B"), solver.GetVariable("E"));
      solver.AddClause(solver.GetVariable("A"), ~solver.GetVariable("B"));
      solver.AddClause(solver.GetVariable("A"), ~solver.GetVariable("E"));
      solver.AddClause(~solver.GetVariable("E"), solver.GetVariable("D"));
      solver.AddClause(~solver.GetVariable("C"), ~solver.GetVariable("F"), ~solver.GetVariable("B"));
      solver.AddClause(~solver.GetVariable("E"), solver.GetVariable("B"));
      solver.AddClause(~solver.GetVariable("B"), solver.GetVariable("F"));
      solver.AddClause(~solver.GetVariable("B"), solver.GetVariable("C"));

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

    private static void runSimpleSolver()
    {
      var solver = new Sat(SimpleDPLLStrategy.Solve);
      solver.CreateVariable("A");
      solver.CreateVariable("B");
      solver.AddClause(solver.GetVariable("B"));
      solver.AddClause(~solver.GetVariable("A"), ~solver.GetVariable("B"), solver.GetVariable("A"));
      solver.AddClause(~solver.GetVariable("B"), ~solver.GetVariable("A"));

      //solver.AddClause(solver.GetVariable("B"));
      //solver.AddClause(solver.GetVariable("A"));
      //solver.AddClause(solver.GetVariable("A"));
      //solver.AddClause(solver.GetVariable("A"),  ~solver.GetVariable("B"));
      // solver.AddClause(~solver.GetVariable("A"),  solver.GetVariable("B"));
      ////solver.CreateVariable("C");
      //////solver.CreateVariable("D");
      ////solver.AddClause(solver.GetVariable("A"), solver.GetVariable("B"), ~solver.GetVariable("A"));
      ////solver.AddClause(solver.GetVariable("A"), solver.GetVariable("B"), ~solver.GetVariable("C"));
      //solver.AddClause(solver.GetVariable("D"));
      //solver.AddClause(solver.GetVariable("C"));
      //solver.AddClause(~solver.GetVariable("C"), ~solver.GetVariable("D"));
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
