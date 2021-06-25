using System;

namespace MCTech.Fss.Client.Exceptions
{

  public class MCTechException : Exception
  {
    public MCTechException(string message)
      : base(message)
    {
    }
  }
}