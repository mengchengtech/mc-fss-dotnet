using System;
using System.Collections;

namespace MCTech.Fss.Client
{
  public class SignedData
  {
    public Uri TargetUrl { get; set; }
    public FssOperation Method { get; set; }
    public string Resource { get; set; }
    public Hashtable Headers { get; set; }
  }
}