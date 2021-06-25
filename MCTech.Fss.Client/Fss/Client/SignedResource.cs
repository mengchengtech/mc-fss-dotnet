using System;
using System.Collections.Specialized;

namespace MCTech.Fss.Client
{
  public class SignedResource
  {
    public string Signature { get; set; }
    public NameValueCollection SubResource { get; set; }

    public long? Expires { get; set; }

    public string FormatDate { get; set; }
  }
}