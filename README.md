A toy implementation of a simple SAT solver in the C#. 
* Supports DPLL algorithm.
* Supports DIMACS format
* Simple Sudoku solver

``` 
 var sat = await Sat.FromFile(dimacsFilePath).ConfigureAwait(false);

var isSatisfiable = sat.Solve();
Console.WriteLine(sat.FoundModel);
Assert.IsTrue(isSatisfiable);

 ```

