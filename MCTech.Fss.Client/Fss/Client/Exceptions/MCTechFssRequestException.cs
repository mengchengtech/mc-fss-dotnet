namespace MCTech.Fss.Client.Exceptions
{
  public class MCTechFssRequestException : MCTechException
  {
    public FssClientError Error { get; private set; }

    public MCTechFssRequestException(string message, FssClientError error)
      : base(message)
    {
      this.Error = error;
    }
  }
}