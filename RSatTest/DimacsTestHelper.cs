using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace RSatTest
{
  public class DimacsTestHelper
  {
    public static IEnumerable<string> GenerateDimacsFilesPath()
    {
      const string DIMACS_FILES__PATH = "DIMACS_Samples/";
      var fullDimacsPath = Path.Combine(TestContext.CurrentContext.TestDirectory, DIMACS_FILES__PATH);
      return from dir in Directory.EnumerateDirectories(fullDimacsPath)
             let fullInnerDimacsPath = Path.Combine(fullDimacsPath, dir)
             from file in Directory.GetFiles(fullInnerDimacsPath)
             select file;
    }
  }
}