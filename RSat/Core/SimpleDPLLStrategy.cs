using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using Clausules =
  System.Collections.Immutable.ImmutableList<System.Collections.Immutable.ImmutableList<RSat.Core.Literal>>;
using VariablesMap = System.Collections.Immutable.ImmutableDictionary<string, RSat.Core.Variable>;

namespace RSat.Core
{
  public static class SimpleDPLLStrategy
  {
    public static Model? Solve(Clausules clausules,
                               VariablesMap variablesMap)
    {
      const int INITIAL_LEVEL = 0;
      var solverStack = ImmutableStack<SolverState>.Empty;
      var initialSolverState = new SolverState(clausules,
                                               variablesMap,
                                               INITIAL_LEVEL,
                                               true);

      solverStack = solverStack.Push(initialSolverState);
      while (!solverStack.IsEmpty)
      {
        
        solverStack = solverStack.Pop(out var currentState);
        Trace.WriteLine($"Iteration depth: {currentState.Depth}");

        if (hasContradictions(currentState.Clausules))
        {
          Trace.WriteLine("Contradiction found. Backtracking...");
          break;
        }

        if (isConsistentSetOfLiterals(currentState.Clausules))
        {
          Trace.WriteLine("Found model...");
          return new Model(0, generateModelValues(currentState.Clausules, currentState.VariablesMap));
        }

        if (hasEmptyClausule(currentState.Clausules))
        {
          Trace.WriteLine("Empty clausule found. Backtracking...");
          continue;
        }

      
        var (afterPropagateClausules, afterPropagateVariableMap) = propagateUnitClausules(currentState.Clausules, currentState.VariablesMap);
        var (afterPureLiteralClausules, afterPureLiteralVariableMap) = handlePureLiterals(afterPropagateClausules, afterPropagateVariableMap);


        var chosenLiteral = chooseNewLiteral(afterPureLiteralClausules,
                                             afterPureLiteralVariableMap);



        var somethingChanged = chosenLiteral.IsValid ||
                               !ReferenceEquals(afterPureLiteralClausules, currentState.Clausules) ||
                               !ReferenceEquals(afterPureLiteralVariableMap, currentState.VariablesMap);
        if (!somethingChanged)
        {
          Trace.WriteLine("Nothing changed. Backtracking...");
          continue;
        }

        if (!chosenLiteral.IsValid)
        {
          Trace.WriteLine($"Out of literals");
          solverStack =
            solverStack.Push(new
                               SolverState(afterPureLiteralClausules,
                                           afterPureLiteralVariableMap,
                                           currentState.Depth + 1,
                                           false));
        }
        else
        {
          Trace.WriteLine($"Chosen literal {chosenLiteral.Name}");
          var variableForChosenLiteral = afterPureLiteralVariableMap[chosenLiteral.Name];
          solverStack =
            solverStack.Push(new
                               SolverState(afterPureLiteralClausules.Add(ImmutableList<Literal>.Empty.Add(~variableForChosenLiteral)),
                                           afterPureLiteralVariableMap,
                                           currentState.Depth + 1,
                                          true));
          solverStack = solverStack.Push(new
                                           SolverState(afterPureLiteralClausules.Add(ImmutableList<Literal>.Empty.Add(variableForChosenLiteral)),
                                                       afterPureLiteralVariableMap,
                                                       currentState.Depth + 1,
                                                      true));
        }
      }

      return null;
    }

    private static bool hasContradictions(Clausules clausules)
    {
      return clausules.Where(clausule => clausule.Count == 1).Select(clausule => clausule[0])
                      .GroupBy(clausule => clausule.Name)
                      .Any(groupedClausules => groupedClausules.Distinct().Count() != 1);
    }

    private static IEnumerable<ModelValue> generateModelValues(Clausules clausules,
                                                               VariablesMap variablesMap)
    {
      var modelValues = clausules
                      .Select(clausule =>
                      {
                        var literal = clausule[0];
                        return new ModelValue(literal.Name, literal.IsTrue);
                      }).Distinct().ToArray();

      var retModelValues = modelValues.Concat(variablesMap
                                              .Keys.Where(varName => !modelValues.Any(modelValue =>
                                                                                        modelValue
                                                                                          .Name.Equals(varName)))
                                              .Select(varName => new ModelValue(varName, true))).ToArray();
      return retModelValues;
    }

    private static Literal chooseNewLiteral(Clausules clausules,
                                           ImmutableDictionary<string, Variable> variablesMap)
    {
      var selectedLiteral = (from clausule in clausules
                             from literal in clausule
                             where variablesMap[literal.Name].NoneValuesUsed()
                             select literal).FirstOrDefault();

      return selectedLiteral;
    }

    private static (Clausules, VariablesMap) handlePureLiterals(Clausules clausules,
                                                ImmutableDictionary<string, Variable> variablesMap)
    {
      var allLiterals = (from clausule in clausules
                         from literal in clausule
                         select literal).ToArray();


      var pureLiteralsInClausules =
        allLiterals.Where(literal => allLiterals.All(nextLiteral =>
                                                      !nextLiteral.Name.Equals(literal.Name) ||
                                                      nextLiteral.IsTrue == literal.IsTrue)).Distinct();
      var newClausules = clausules;
      var newVariablesMap = variablesMap;
      foreach (var pureLiteral in pureLiteralsInClausules)
      {
        //if (newVariablesMap[pureLiteral.Name].AnyValueUsed())
        //{
        //  continue;
        //}

        Trace.WriteLine($"Trying pure literal strategy: {pureLiteral}");
        newVariablesMap = newVariablesMap.SetItem(pureLiteral.Name, pureLiteral.IsTrue
          ? variablesMap[pureLiteral.Name].TryTrueValue()
          : variablesMap[pureLiteral.Name].TryFalseValue());

        var toDeleteClausules = newClausules.Where(clausule => clausule.Contains(pureLiteral));
        newClausules = newClausules.RemoveRange(toDeleteClausules);
        var pureLiteralClausule = ImmutableList<Literal>.Empty.Add(pureLiteral);
        newClausules = newClausules.Add(pureLiteralClausule);
      }

      return (newClausules, newVariablesMap);

    }

    private static bool hasEmptyClausule(Clausules clausules)
    {
      return clausules.Any(clausule => !clausule.Any());
    }

    private static (Clausules, VariablesMap) propagateUnitClausules(Clausules clausules,
                                                    ImmutableDictionary<string, Variable> variablesMap)
    {
      var newClausules = clausules;
      var newVariableMap = variablesMap;
      while (true)
      {
        var toPropagateUnitClausules = newClausules
                                       .Where(clausule => clausule.Count == 1 &&
                                                          !newVariableMap[clausule[0].Name].AnyValueUsed())
                                       .Select(clausule => clausule[0])
                                       .Distinct()
                                       .ToArray();

        if (!toPropagateUnitClausules.Any())
        {
          break;
        }

        foreach (var unitClausule in toPropagateUnitClausules)
        {
          if(newVariableMap[unitClausule.Name].AnyValueUsed())
          {
            continue;
          }
          Trace.WriteLine($"Trying unit propagation of the clausule {unitClausule}");
          newVariableMap = newVariableMap.SetItem(unitClausule.Name, unitClausule.IsTrue
                                                    ? variablesMap[unitClausule.Name].TryTrueValue()
                                                    : variablesMap[unitClausule.Name].TryFalseValue());

          var toModifyClausules =
            newClausules.Where(clausule => clausule.Any(literal => literal.Name.Equals(unitClausule.Name) &&
                                                                   literal.IsTrue != unitClausule.IsTrue));
          newClausules = newClausules.RemoveRange(toModifyClausules);
          var modifiedClausules =
            toModifyClausules.Select(clausule => clausule
                                                 .Where(literal => literal.Name != unitClausule.Name ||
                                                                   literal.Equals(unitClausule))
                                                 .ToImmutableList());
          newClausules = newClausules.AddRange(modifiedClausules);
        }
      }

      return (newClausules, newVariableMap);
    }


    private static bool isConsistentSetOfLiterals(Clausules clausules)
    {
      return !clausules.Any() || clausules.All(clausule => clausule.Count == 1) &&
             clausules.Select(clausule => clausule[0])
                      .GroupBy(clausule => clausule.Name)
                      .All(groupedClausules => groupedClausules.Distinct().Count() == 1);
    }


    private struct SolverState
    {
      public SolverState(Clausules clausules,
                         ImmutableDictionary<string, Variable> variablesMap,
                         int depth,
                         bool literalAdded)
      {
        Clausules = clausules;
        VariablesMap = variablesMap;
        Depth = depth;
        LiteralAdded = literalAdded;
      }

      public Clausules Clausules
      {
        get;
      }

      public VariablesMap VariablesMap
      {
        get;
      }

      public int Depth
      {
        get;
      }

      public bool LiteralAdded
      {
        get;
      }
    }
  }
}