using System;
using System.Collections.Generic;

namespace RSat.Core
{
  public class Sat
  {
    private readonly List<Literal[]> _clausules;
    private readonly IDictionary<string, Variable> _variablesMap;

    public Sat()
    {
      _variablesMap = new Dictionary<string, Variable>();
      _clausules = new List<Literal[]>();
    }

    public void CreateVariable(string name)
    {
      var variable = new Variable(name);
      _variablesMap.Add(name, variable);
    }

    public Variable GetVariable(string name)
    {
      return _variablesMap[name];
    }

    public void AddLiterals(params Literal[] literals)
    {
      if (literals == null)
      {
        throw new ArgumentNullException(nameof(literals));
      }

      _clausules.Add(literals);
    }

    public bool Solve()
    {
      var results = generateModels();

      foreach (var result in results)
      {
        Console.WriteLine(result);
      }

      return false;
    }

    private IEnumerable<Model> generateModels()
    {
      const int VALUATIONS = 2;
      var variablesMapCount = _variablesMap.Count;

      var numberOfModels = (long) Math.Pow(VALUATIONS, variablesMapCount);
      for (var modelIndex = 0L; modelIndex < numberOfModels; modelIndex++)
      {
        var modelValues = new ModelValue[variablesMapCount];
        var varIndex = 0;
        foreach (var varName in _variablesMap.Keys)
        {
          var isVarTrue = (modelIndex & (1L << varIndex)) == 1;
          modelValues[varIndex] = new ModelValue(varName, isVarTrue);
          varIndex++;
        }

        yield return new Model(modelIndex, modelValues);
      }
    }
  }
}