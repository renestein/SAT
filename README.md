[![Build Status](https://dev.azure.com/rene0884/RSat/_apis/build/status/renestein.SAT?branchName=master)](https://dev.azure.com/rene0884/RSat/_build/latest?definitionId=1&branchName=master)

A toy implementation of a simple SAT solver in the C#.

* Supports DPLL algorithm.
* Supports DIMACS format
* Simple Sudoku solver

Solving problem described in the DIMACS file.
``` 
 var sat = await Sat.FromFile(dimacsFilePath).ConfigureAwait(false);

var isSatisfiable = sat.Solve();
Console.WriteLine(sat.FoundModel);
Assert.IsTrue(isSatisfiable);
```



Hard sudoku, try it.

![Sudoku](https://snipboard.io/CfnwtH.jpg "Sudoku")


Encoding Sudoku rules.
```
    public static void RunNotFunSudoku()
    {
      var board = new SudokuBoard
      {
        [0, 1] = new CellValue(2),

        [1, 3] = new CellValue(6),
        [1, 8] = new CellValue(3),

        [2, 1] = new CellValue(7),
        [2, 2] = new CellValue(4),
        [2, 4] = new CellValue(8),

        [3, 5] = new CellValue(3),
        [3, 8] = new CellValue(2),

        [4, 1] = new CellValue(8),
        [4, 4] = new CellValue(4),
        [4, 7] = new CellValue(1),

        [5, 0] = new CellValue(6),
        [5, 3] = new CellValue(5),

        [6, 4] = new CellValue(1),
        [6, 6] = new CellValue(7),
        [6, 7] = new CellValue(8),

        [7, 0] = new CellValue(5),
        [7, 5] = new CellValue(9),

        [8, 7] = new CellValue(4),

      };

      Run(board);
   }
```
 Solved sudoku.
 
 ![Solved sudoku](https://snipboard.io/dbPcBW.jpg "Sudoku")

