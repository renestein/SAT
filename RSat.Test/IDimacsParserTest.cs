using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using RSatLib.Dimacs;

namespace RSatTest
{
  public abstract class IDimacsParserTest
  {
    protected abstract IDimacsParser CreateParser();

    [TestCaseSource(typeof(DimacsTestHelper), nameof(DimacsTestHelper.GenerateDimacsFilesPath))]
    public async Task ParseCnf_When_Valid_DIMACS_File_Then_Returns_Sat(string dimacsFilePath)
    {
      var fileStream = File.Open(dimacsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
      var parser = CreateParser();

      var sat = await parser.ParseCnf(fileStream).ConfigureAwait(false);

      Assert.That(sat, Is.Not.Null);
    }

   
  }
}