using System.Collections.Generic;

namespace RSat.Core
{
  public class LiteralsToClausesMap
  {
    public LiteralsToClausesMap(string name) : this(name,
                                                    new List<Clause>(),
                                                    new List<Clause>())
    {
    }

    public LiteralsToClausesMap(string variableName,
                                List<Clause> clausesWithPositiveLiterals,
                                List<Clause> clausesWithNegativeLiterals)
    {
      VariableName = variableName;
      ClausesWithPositiveLiterals = clausesWithPositiveLiterals;
      ClausesWithNegativeLiterals = clausesWithNegativeLiterals;
    }

    public string VariableName
    {
      get;
    }

    public List<Clause> ClausesWithPositiveLiterals
    {
      get;
    }

    public List<Clause> ClausesWithNegativeLiterals
    {
      get;
    }

    public Literal? TryGetPureLiteral(Variables variables)
    {

      if (ClausesWithPositiveLiterals.Count == 0 && ClausesWithNegativeLiterals.Count > 1)
      {
        return ClausesWithNegativeLiterals.Count == 1 && ClausesWithNegativeLiterals[0].IsUnitClause()
          ? null
          : ~variables[VariableName];
      }

      if (ClausesWithNegativeLiterals.Count == 0 && ClausesWithPositiveLiterals.Count > 1)
      {
        return ClausesWithPositiveLiterals.Count == 1 && ClausesWithPositiveLiterals[0].IsUnitClause()
          ? null
          : (Literal)variables[VariableName];
      }

      return null;
    }
  }
}