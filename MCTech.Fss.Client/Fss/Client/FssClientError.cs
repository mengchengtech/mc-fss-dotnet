using System;
using System.Collections;

namespace MCTech.Fss.Client
{

  public class FssClientError
  {
    private const string PROP_CODE = "Code";
    private const string PROP_MESSAGE = "Message";
    private const string PROP_STRING_TO_SIGN_BYTES = "StringToSignBytes";
    private const string PROP_SIGNATURE_PROVIDED = "SignatureProvided";
    private const string PROP_STRING_TO_SIGN = "StringToSign";
    private const string PROP_ACCESS_KEY_ID = "AccessKeyId";

    private readonly Hashtable map;

    public FssClientError(Hashtable map)
    {
      this.map = map;
    }

    public string getCode()
    {
      return this.getProperty(PROP_CODE);
    }

    public string getMessage()
    {
      return this.getProperty(PROP_MESSAGE);
    }

    public string getStringToSignBytes()
    {
      return this.getProperty(PROP_STRING_TO_SIGN_BYTES);
    }

    public string getSignatureProvided()
    {
      return this.getProperty(PROP_SIGNATURE_PROVIDED);
    }

    public string getStringToSign()
    {
      return this.getProperty(PROP_STRING_TO_SIGN);
    }

    public string getAccessKeyId()
    {
      return this.getProperty(PROP_ACCESS_KEY_ID);
    }

    public string getProperty(string name)
    {
      return (string)this.map[name];
    }
  }
}