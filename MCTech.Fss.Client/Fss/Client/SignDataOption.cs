using System.Collections.Specialized;

namespace MCTech.Fss.Client
{
  public class SignDataOption
  {
    public FssOperation Method { get; set; }
    public string Key { get; set; }

    public string ContentType { get; set; }

    public NameValueCollection Metadata { get; set; }
  }
}