using System;
using System.Diagnostics;
using RSat.Core;

namespace RSat
{
  class Program
  {
    static void Main(string[] args)
    {
      Trace.Listeners.Add(new ConsoleTraceListener());
     //runSimpleSolver();
     Sudoku.Run();
     //solveMoreFormulas();

      Console.ReadLine();
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
      solver.AddClausule(~solver.GetVariable("A"), solver.GetVariable("B"), solver.GetVariable("E"));
      solver.AddClausule(solver.GetVariable("A"), ~solver.GetVariable("B"));
      solver.AddClausule(solver.GetVariable("A"), ~solver.GetVariable("E"));
      solver.AddClausule(~solver.GetVariable("E"), solver.GetVariable("D"));
      solver.AddClausule(~solver.GetVariable("C"), ~solver.GetVariable("F"), ~solver.GetVariable("B"));
      solver.AddClausule(~solver.GetVariable("E"), solver.GetVariable("B"));
      solver.AddClausule(~solver.GetVariable("B"), solver.GetVariable("F"));
      solver.AddClausule(~solver.GetVariable("B"), solver.GetVariable("C"));

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
      solver.AddClausule(~solver.GetVariable("B"), solver.GetVariable("A"));
      solver.AddClausule(~solver.GetVariable("A"), solver.GetVariable("B"));
      solver.AddClausule(solver.GetVariable("A"));
      solver.AddClausule(~solver.GetVariable("A"));
        
      //solver.AddClausule(solver.GetVariable("B"));
      //solver.AddClausule(solver.GetVariable("A"));
      //solver.AddClausule(solver.GetVariable("A"));
      //solver.AddClausule(solver.GetVariable("A"),  ~solver.GetVariable("B"));
      // solver.AddClausule(~solver.GetVariable("A"),  solver.GetVariable("B"));
      ////solver.CreateVariable("C");
      //////solver.CreateVariable("D");
      ////solver.AddClausule(solver.GetVariable("A"), solver.GetVariable("B"), ~solver.GetVariable("A"));
      ////solver.AddClausule(solver.GetVariable("A"), solver.GetVariable("B"), ~solver.GetVariable("C"));
      //solver.AddClausule(solver.GetVariable("D"));
      //solver.AddClausule(solver.GetVariable("C"));
      //solver.AddClausule(~solver.GetVariable("C"), ~solver.GetVariable("D"));
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
