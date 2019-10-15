using System;

namespace RSat.Sudoku
{
  public class SudokuBoard
  {
    public const int ROWS = 9;
    public const int COLUMNS = 9;

    private readonly CellValue[,] _board;

    public SudokuBoard()
    {
      _board = new CellValue[ROWS, COLUMNS];
      initBoard();
    }

    public CellValue this[Index row,
                          Index column]
    {
      get => _board[row.Value, column.Value];
      set => _board[row.Value, column.Value] = value;
    }

    private void initBoard()
    {
      for (var i = 0; i < ROWS; i++)
      {
        for (var j = 0; j < COLUMNS; j++)
        {
          this[i, j] = CellValue.Unknown;
        }
      }
    }
  }
}