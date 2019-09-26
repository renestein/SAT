using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

    public IEnumerable<Model> FoundModels
    {
      get;
      private set;
    } = Enumerable.Empty<Model>();

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
      var models = generateModels();

      FoundModels = models.Where(model => model.IsModelFor(_clausules)).ToArray();
      return FoundModels.Any();
    }


    private IEnumerable<Model> generateModels()
    {
      const int VALUATIONS = 2;
      var variablesMapCount = _variablesMap.Count;
      var singleLiterals = _clausules.Where(literals => literals.Length == 1)
                                     .Select(literals => literals[0])
                                     .ToImmutableArray();
      var numberOfModels = (long) Math.Pow(VALUATIONS, variablesMapCount);
      for (var modelIndex = 0L; modelIndex < numberOfModels; modelIndex++)
      {
        var modelValues = new ModelValue[variablesMapCount];
        var varIndex = 0;
        foreach (var varName in _variablesMap.Keys)
        {
          var isVarTrue = (modelIndex & (1L << varIndex)) != 0;
          modelValues[varIndex] = new ModelValue(varName, isVarTrue);
          varIndex++;
        }

        if (modelValues.Any(modelValue =>
                              singleLiterals.Any(literal => literal.Name.Equals(modelValue.Name) &&
                                                            modelValue.IsTrue != literal.IsTrue)))
        {
          continue;
        }

        var model = new Model(modelIndex, modelValues);
        //Console.WriteLine(model);

        yield return model;
      }
    }
  }
}