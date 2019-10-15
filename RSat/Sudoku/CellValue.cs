using System;

namespace RSat.Sudoku
{
  public class CellValue
  {
    public const int UNDEFINED_VALUE = -1;
    public const int MIN_VALUE = 1;
    public const int MAX_VALUE = 9;

    public static readonly CellValue Unknown = new CellValue();
    public CellValue(int value)
    {
      if (value < MIN_VALUE || value > MAX_VALUE)
      {
        throw new ArgumentOutOfRangeException(nameof(value));
      }

      Value = value;
    }

    private CellValue()
    {
      Value = UNDEFINED_VALUE;
    }
    public int Value
    {
      get;
    }

    public static implicit operator int(CellValue cellValue)
    {
      if (cellValue == null)
      {
        throw new ArgumentNullException(nameof(cellValue));
      }

      return cellValue.Value;
    }
  }
}