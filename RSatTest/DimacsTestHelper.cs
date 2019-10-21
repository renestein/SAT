using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace RSatTest
{
  public class DimacsTestHelper
  {
    private const string IGNORE_FILE_PREFIX = "ignore";
    private const string UNSATISFIABLE_FILE_PREFIX = "unsat";
    public static IEnumerable<string> GenerateDimacsFilesPath()
    {
      const string DIMACS_FILES__PATH = "DIMACS_Samples/";
      var fullDimacsPath = Path.Combine(TestContext.CurrentContext.TestDirectory, DIMACS_FILES__PATH);
      return from dir in Directory.EnumerateDirectories(fullDimacsPath)
             let fullInnerDimacsPath = Path.Combine(fullDimacsPath, dir)
             from file in Directory.GetFiles(fullInnerDimacsPath)
             select file;
    }

    public static IEnumerable<string> GenerateSatisfiableDimacsFilesPath()
    {

      return GenerateDimacsFilesPath().Where(path =>
      {
        var fileName = Path.GetFileName(path);
        return !fileName.StartsWith(IGNORE_FILE_PREFIX, StringComparison.OrdinalIgnoreCase) &&
               !fileName.StartsWith(UNSATISFIABLE_FILE_PREFIX, StringComparison.OrdinalIgnoreCase);
      });
    }

    
    public static IEnumerable<string> GenerateUnsatisfiableDimacsFilesPath()
    {

      return GenerateDimacsFilesPath().Where(path =>
      {
        var fileName = Path.GetFileName(path);
        return fileName.StartsWith(UNSATISFIABLE_FILE_PREFIX, StringComparison.OrdinalIgnoreCase);
      });
    }
  }
}