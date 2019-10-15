using System;

namespace RSat.Sudoku
{
  public class CellValue : IEquatable<CellValue>
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

    public bool Equals(CellValue other)
    {
      if (ReferenceEquals(null, other))
      {
        return false;
      }

      if (ReferenceEquals(this, other))
      {
        return true;
      }

      return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
      {
        return false;
      }

      if (ReferenceEquals(this, obj))
      {
        return true;
      }

      if (obj.GetType() != this.GetType())
      {
        return false;
      }

      return Equals((CellValue) obj);
    }

    public override int GetHashCode()
    {
      return Value;
    }

    public static bool operator ==(CellValue left,
                                   CellValue right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(CellValue left,
                                   CellValue right)
    {
      return !Equals(left, right);
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