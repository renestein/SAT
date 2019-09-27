using System.Collections.Generic;
using System.Collections.Immutable;

namespace RSat.Core
{
  public static class SimpleDPLLStrategy
  {
    public static Model? Solve(ImmutableList<ImmutableArray<Literal>> clausules,
                               IDictionary<string, Variable> variablesMap)
    {
      ImmutableStack<SolverState> _operationStack = ImmutableStack<SolverState>.Empty;

      return null;
    }


    private class SolverState
    {

    }
  }
}