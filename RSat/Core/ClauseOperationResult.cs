namespace RSat.Core
{
  public partial class ClauseSet
  {
    public enum ClauseOperationResult
    {
       Invalid = 0,
       OperationSuccess,
       OperationNotUsed, 
       MinOneEmptyClausuleFound,

    }
  }
}