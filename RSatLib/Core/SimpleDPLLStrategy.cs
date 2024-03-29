﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

//Naive, inefficient (LINQ, Immutable collections), dirty.
namespace RSatLib.Core
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


      var solverStack = ImmutableStack<SolverState>.Empty;
      var initialSolverState = new SolverState(initialClauses,
                                               variablesMap,
                                               INITIAL_LEVEL,
                                               true);

      solverStack = solverStack.Push(initialSolverState);
      while (!solverStack.IsEmpty)
      {

        solverStack = solverStack.Pop(out var currentState);

#if PROGRESS_TRACE
        Trace.WriteLine($"Iteration depth: {currentState.Depth}");
#endif
        var clauses = currentState.Clauses;
        var variables = currentState.VariablesMap;


        if (isConsistentSetOfLiterals(clauses))
        {
#if PROGRESS_TRACE
          Trace.WriteLine("Found model...");
#endif
          return new Model(0, generateModelValues(clauses, variables));
        }

        if (hasEmptyClause(clauses))
        {
#if PROGRESS_TRACE
          Trace.WriteLine("Empty clause found. Backtracking...");
#endif

          continue;
        }

        var unitClauseRuleResult = propagateUnitClauses(clauses, variables);
        if (unitClauseRuleResult == ClauseSet.ClauseOperationResult.MinOneEmptyClausuleFound)
        {
#if PROGRESS_TRACE
          Trace.WriteLine("Empty clause (after unit rule) found. Backtracking...");
#endif

          continue;
        }

        handlePureLiterals(clauses, variables);
        
        var chosenLiteral = chooseNewLiteral(clauses,
                                             variables);

        if (chosenLiteral == null)
        {
          if (hasEmptyClause(clauses))
          {

#if PROGRESS_TRACE
            Trace.WriteLine("Empty clause found. Backtracking...");
#endif

            continue;
          }

#if PROGRESS_TRACE
          Trace.WriteLine("Found model...");
#endif

          return new Model(0, generateModelValues(clauses, variables));
        }
#if PROGRESS_TRACE
        Trace.WriteLine($"Chosen literal {chosenLiteral.Name}");
#endif


        var newClauseNeg = clauses.CloneWithClause(new Clause(new List<Literal> { ~chosenLiteral }));
        var newClausePos = clauses.CloneWithClause(new Clause(new List<Literal> { chosenLiteral }));


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

      return null;
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

#if PROGRESS_TRACE
      Trace.WriteLine($"Trying pure literal strategy: {pureLiteral}");
#endif

        variablesMap.SetToValue(pureLiteral.Name, pureLiteral.IsTrue);

        hasPureLiterals = true;
        removeClausulesSatisfiedByLiteral(clauses, pureLiteral);
      }

      return hasPureLiterals;

    }

    private static void removeClausulesSatisfiedByLiteral(ClauseSet clausules,
                                                                 Literal literal)
    {
#if PROGRESS_TRACE
      Trace.WriteLine($"Deleting clausules with literal: {literal}");
#endif
      clausules.DeleteClausesWithLiteral(literal);

#if PROGRESS_TRACE
      Trace.WriteLine($"Remaining ClausulesSet: {clausules.ClausesCount}");
#endif

    }

    private static bool hasEmptyClause(ClauseSet clauses)
    {
      return clauses.HasEmptyClause();
    }

    private static ClauseSet.ClauseOperationResult propagateUnitClauses(ClauseSet clauses,
                                             Variables variablesMap)
    {

      var unitClausuleRuleResult = ClauseSet.ClauseOperationResult.OperationNotUsed;
      while (true)
      {
        var toPropagateUnitClause = clauses.SelectUnitClause(variablesMap);
        if (toPropagateUnitClause == null)
        {
          break;
        }

        unitClausuleRuleResult = ClauseSet.ClauseOperationResult.OperationSuccess;

        var singleLiteral = toPropagateUnitClause.FirstLiteral;

#if PROGRESS_TRACE
    Trace.WriteLine($"Trying unit propagation of the clause with literal: {singleLiteral}");
#endif


        variablesMap.SetToValue(singleLiteral.Name, singleLiteral.IsTrue);
        removeClausulesSatisfiedByLiteral(clauses, singleLiteral);
        clauses.AddClause(toPropagateUnitClause);
        var deleteLiteralResult = clauses.DeleteLiteralFromClauses(~singleLiteral);
        if (deleteLiteralResult == ClauseSet.ClauseOperationResult.MinOneEmptyClausuleFound)
        {
          return deleteLiteralResult;
        }
      }

      return unitClausuleRuleResult;
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
    }
  }
}