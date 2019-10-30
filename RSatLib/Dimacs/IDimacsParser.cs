using System.IO;
using System.Threading.Tasks;
using RSatLib.Core;

namespace RSatLib.Dimacs
{
  public interface IDimacsParser
  {
    Task<Sat> ParseCnf(Stream stream);
  }
}

