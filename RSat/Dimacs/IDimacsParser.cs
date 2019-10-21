using System.IO;
using System.Threading.Tasks;
using RSat.Core;

namespace RSat.Dimacs
{
  public interface IDimacsParser
  {
    Task<Sat> ParseCnf(Stream stream);
  }
}

