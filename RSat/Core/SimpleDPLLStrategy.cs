using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

//Naive, inefficient (LINQ, Immutable collections), dirty.
namespace RSat.Core
{
  public static class SimpleDPLLStrategy
  {
    public static Model? Solve(ClauseSet initialClauses,
                               Variables variablesMap)
    {
      if (initialClauses == null)
      {
        throw new ArgumentNullException(nameof(initialClauses));
      }

      if (variablesMap == null)
      {
        throw new ArgumentNullException(nameof(variablesMap));
      }

      const int INITIAL_LEVEL = 0;

      //preprocessClausules(initialClauses);
   
      var solverStack = ImmutableStack<SolverState>.Empty;
      var initialSolverState = new SolverState(initialClauses,
                                               variablesMap,
                                               INITIAL_LEVEL,
                                               true);

      solverStack = solverStack.Push(initialSolverState);
      while (!solverStack.IsEmpty)
      {

        solverStack = solverStack.Pop(out var currentState);
        Trace.WriteLine($"Iteration depth: {currentState.Depth}");

        var clauses = currentState.Clauses;
        var variables = currentState.VariablesMap;


        if (isConsistentSetOfLiterals(clauses))
        {
          Trace.WriteLine("Found model...");
          return new Model(0, generateModelValues(clauses, variables));
        }

        if (hasEmptyClause(clauses))
        {
          Trace.WriteLine("Empty clause found. Backtracking...");
          continue;
        }

        if (hasContradictions(clauses))
        {
          Trace.WriteLine("Contradiction found. No model...");
        }


        var unitClausePropagated = propagateUnitClauses(clauses, variables);
         var pureLiteralsProcessed = handlePureLiterals(clauses, variables);


        var chosenLiteral = chooseNewLiteral(clauses,
                                             variables);



        var somethingChanged = chosenLiteral != null ||
                                unitClausePropagated ||
                                pureLiteralsProcessed;

        if (!somethingChanged && !currentState.SomethingChangedInPreviousIteration)
        {
          Trace.WriteLine("Nothing changed. Backtracking...");
          continue;
        }

        if (chosenLiteral == null)
        {
          Trace.WriteLine($"Out of literals");
          solverStack =
            solverStack.Push(new
                               SolverState(clauses.Clone(),
                                           variablesMap.Clone(),
                                           currentState.Depth + 1,
                                           false)
                               {
                                 SomethingChangedInPreviousIteration =  somethingChanged
                               });
        }
        else
        {
          Trace.WriteLine($"Chosen literal {chosenLiteral.Name}");

          var newClauseNeg = clauses.CloneWithClause(new Clause(new List<Literal>{~chosenLiteral}));
          var newClausePos = clauses.CloneWithClause(new Clause(new List<Literal>{chosenLiteral}));


          solverStack =
            solverStack.Push(new
                               SolverState(newClauseNeg,
                                           variablesMap.Clone(),
                                           currentState.Depth + 1,
                                          true));
          solverStack = solverStack.Push(new
                                           SolverState(newClausePos,
                                                       variablesMap.Clone(),
                                                       currentState.Depth + 1,
                                                      true));
        }
      }

      return null;
    }

    private static void preprocessClausules(ClauseSet clausules)
    {
      clausules.DeleteTautologies();
    }

    private static bool hasContradictions(ClauseSet clauses)
    {
      return clauses.IsContradiction();
    }

    private static IEnumerable<ModelValue> generateModelValues(ClauseSet clausesSet,
                                                               Variables variablesMap)
    {
      var modelValues = clausesSet.Clauses.Select(clause =>
                      {
                        var literal = clause.FirstLiteral;
                        return new ModelValue(literal.Name, literal.IsTrue);
                      }).Distinct().ToArray();

      var retModelValues = modelValues.Concat(variablesMap
                                              .VariableNames().Where(varName => !modelValues.Any(modelValue =>
                                                                                        modelValue
                                                                                          .Name.Equals(varName)))
                                              .Select(varName => new ModelValue(varName, true))).ToArray();
      return retModelValues;
    }

    private static Literal? chooseNewLiteral(ClauseSet clauses,
                                           Variables variablesMap)
    {
      var selectedLiteral = clauses.SelectUnusedLiteral(variablesMap);

      return selectedLiteral;
    }

    private static bool handlePureLiterals(ClauseSet clauses,
                                                Variables variablesMap)
    {
      var pureLiteralsInClauses = clauses.GetPureLiterals(variablesMap);
      var hasPureLiterals = false;
      foreach (var pureLiteral in pureLiteralsInClauses)
      {
        Trace.WriteLine($"Trying pure literal strategy: {pureLiteral}");
        variablesMap.SetToValue(pureLiteral.Name, pureLiteral.IsTrue);

        hasPureLiterals = true;
        removeClausulesSatisfiedByLiteral(clauses, pureLiteral);
        clauses.AddClause(new Clause(new List<Literal> {pureLiteral}));
        Console.WriteLine($"Remaining ClausesSet: {clauses.ClausesCount}");
      }

      return hasPureLiterals;

    }

    private static void removeClausulesSatisfiedByLiteral(ClauseSet clausules,
                                                                 Literal literal)
    {
      Trace.WriteLine($"Deleting clausules with literal: {literal}");
      clausules.DeleteClausesWithLiteral(literal);
      Console.WriteLine($"Remaining ClausulesSet: {clausules.ClausesCount}");
    }

    private static bool hasEmptyClause(ClauseSet clauses)
    {
      return clauses.HasEmptyClause();
    }

    private static bool propagateUnitClauses(ClauseSet clauses,
                                             Variables variablesMap)
    {

      var unitClausuleProcessed = false;
      while (true)
      {
        var toPropagateUnitClause = clauses.SelectUnitClause(variablesMap);
        if (toPropagateUnitClause == null)
        {
          break;
        }

        unitClausuleProcessed = true;

        var singleLiteral = toPropagateUnitClause.FirstLiteral;
        Trace.WriteLine($"Trying unit propagation of the clause with literal: {singleLiteral}");

        variablesMap.SetToValue(singleLiteral.Name, singleLiteral.IsTrue);
        removeClausulesSatisfiedByLiteral(clauses, singleLiteral);
        clauses.AddClause(toPropagateUnitClause);
        clauses.DeleteLiteralFromClauses(~singleLiteral);
      }

      return unitClausuleProcessed;
    }


    private static bool isConsistentSetOfLiterals(ClauseSet clauses)
    {
      return clauses.IsConsistentSetOfLiterals();
    }


    private struct SolverState
    {
      public SolverState(ClauseSet clauses,
                         Variables variablesMap,
                         int depth,
                         bool literalAdded)
      {
        Clauses = clauses;
        VariablesMap = variablesMap;
        Depth = depth;
        LiteralAdded = literalAdded;
        SomethingChangedInPreviousIteration = true;
      }

      public ClauseSet Clauses
      {
        get;
      }

      public Variables VariablesMap
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

      public bool SomethingChangedInPreviousIteration 
      {
        get;
        set;
      }

    }
  }
}