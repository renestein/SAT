using System;
using RSat.Core;

namespace RSat
{
    class Program
    {
        static void Main(string[] args)
        {
          var solver = new Sat();
          solver.CreateVariable("A ");
          solver.CreateVariable("B");
          solver.CreateVariable("C");
          solver.Solve();
          Console.ReadLine();
        }
    }
}
