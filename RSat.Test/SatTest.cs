using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using RSat.Core;

namespace RSatTest
{
  [TestFixture]
  public class SatTest
  {
    private int _listenerIndex;

    [SetUp]
    public virtual void SetUp()
    {
      _listenerIndex = Trace.Listeners.Add(new ConsoleTraceListener());
    }

    [TearDown]
    public virtual void OneTimeTearDown()
    {
      Trace.Listeners.RemoveAt(_listenerIndex);
    }

    [TestCaseSource(typeof(DimacsTestHelper), nameof(DimacsTestHelper.GenerateSatisfiableDimacsFilesPath))]
    public async Task Solve_When_Clauses_Have_Model_Then_Returns_Satisfiable(string dimacsFilePath)
    {
      var sat = await Sat.FromFile(dimacsFilePath).ConfigureAwait(false);

      var isSatisfiable = sat.Solve();
      Console.WriteLine(sat.FoundModel);

      Assert.IsTrue(isSatisfiable);
    }

    
    [TestCaseSource(typeof(DimacsTestHelper), nameof(DimacsTestHelper.GenerateUnsatisfiableDimacsFilesPath))]
    public async Task Solve_When_Clauses_Does_Not_Have_Model_Then_Returns_Unsatisfiable(string dimacsFilePath)
    {
      var sat = await Sat.FromFile(dimacsFilePath).ConfigureAwait(false);

      var isSatisfiable = sat.Solve();

      Assert.IsFalse(isSatisfiable);
    }


  }
}