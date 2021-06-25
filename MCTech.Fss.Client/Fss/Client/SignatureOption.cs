using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace MCTech.Fss.Client
{
  public class SignatureOption
  {
    /// <summary>
    /// 获取或设置设置REST调用签名中的method信息
    /// </summary>
    public FssOperation Method { get; private set; }
    private readonly string _contentType;

    //public string ContentMd5 { get; private set; }

    /// <summary>
    /// 发出请求的客户端时间
    /// </summary>
    public DateTime? Date { get; set; }

    private long? expires;
    /// <summary>
    /// 发出请求的客户端时间
    /// </summary>
    public long? Expires
    {
      get { return this.expires; }
      set
      {
        this.expires = value;
        this.AbsoluteExpires = DateTime.Now.ToFileTimeUtc() / 1000 + this.expires;
      }
    }

    public long? AbsoluteExpires { get; private set; }

    public string Process { get; set; }

    public NameValueCollection Response { get; set; }

    public NameValueCollection Metadata { get; set; }


    /// <summary>
    /// 自定义的headers头
    /// </summary>
    public NameValueCollection Headers { get; set; }

    public SignatureOption(FssOperation method, string contentType = null)
    {
      this.Method = method;
      this._contentType = contentType;
      // this.ContentMd5 = "";
    }

    /// <summary>
    /// 获取或设置REST调用中的content-type头
    /// </summary>
    public string ContentType
    {
      get
      {
        if (string.IsNullOrWhiteSpace(_contentType))
        {
          return string.Empty;
        }
        return _contentType;
      }
    }

    public string getFormatedDate()
    {
      return this.Date.Value.ToUniversalTime().ToString("r");
    }
  }
}