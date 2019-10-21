using NUnit.Framework;
using RSat.Dimacs;

namespace RSatTest
{
  [TestFixture]
  public class DimacsParserTest : IDimacsParserTest
  {
    protected override IDimacsParser CreateParser()
    {
      return new DimacsParser();
    }
  }
}