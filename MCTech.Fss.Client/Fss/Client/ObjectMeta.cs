using System.Net;
using System.Collections.Specialized;

namespace MCTech.Fss.Client
{
  public class ObjectMeta
  {
    public NameValueCollection Meta { get; set; }
    public NameValueCollection Headers { get; set; }
    public HttpStatusCode StatusCode { get; set; }
  }
}