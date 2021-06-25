using System;
using System.IO;
using System.Net;
using System.Web;
using System.Net.Cache;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Specialized;
using System.Security.Cryptography;

namespace MCTech.Fss.Client
{
  public class MCFssClient
  {
    private readonly MCFssClientConfig _config;
    private readonly Uri _defaultEndpoint;
    private readonly Uri _publicEndPoint;

    public MCFssClient(MCFssClientConfig config)
    {
      this._config = config;
      this._publicEndPoint = new Uri(config.PublicEndPoint);
      this._defaultEndpoint = config.Internal
        ? new Uri(config.PrivateEndPoint)
        : this._publicEndPoint;
    }

    /// <summary>
    /// </summary>
    /// <param name="key"></param>
    /// <returns>返回下载的文件内容，需要注意的时用完后必须显示调用Close方法，释放底层连接对象。否则会出现连接池用尽卡死的问题</returns>
    public RequestResult get(String key)
    {
      SignedData signedData = this.GenerateSignedData(new SignDataOption
      {
        Method = FssOperation.GET,
        Key = key
      });

      return this.SendRequest(signedData);
    }


    /**
     * @param key         文件存到服务器上的key
     * @param fileName    文件原始名称，下载时使用。可为null
     * @param is          要上传的文件内容流。
     * @param metadata    文件附加的meta信息。可为null
     * @param contentType 文件的content-type，下载的时候会用到。可为null
     * @param length      上传的内容长度。可为null
     * @return
     */
    /// <summary>
    /// </summary>
    /// <param name="key">文件存到服务器上的key</param>
    /// <param name="fileName">文件原始名称，下载时使用。可为null</param>
    /// <param name="stream">要上传的文件内容流。</param>
    /// <param name="metadata">文件附加的meta信息。可为null</param>
    /// <param name="contentType">文件的content-type，下载的时候会用到。可为null</param>
    public void Put(string key, string fileName, Stream stream,
      NameValueCollection metadata, string contentType)
    {
      NameValueCollection fssMetadata = new NameValueCollection();
      if (metadata != null)
      {
        foreach (string mKey in metadata)
        {
          string fssKey = HttpConsts.FSS_META_HEADER_PREFIX + mKey;
          fssMetadata[fssKey] = metadata[mKey];
        }
      }

      string rawName = fileName != null ? Path.GetFileName(fileName) : Path.GetFileName(key);
      NameValueCollection headers = new NameValueCollection();


      SignedData signedData = this.GenerateSignedData(new SignDataOption
      {
        Method = FssOperation.PUT,
        Key = key,
        ContentType = contentType,
        Metadata = fssMetadata
      });

      if (fileName != null)
      {
        signedData.Headers.Add(HttpConsts.CONTENT_DISPOSITION,
          "attachment;filename=" + HttpUtility.UrlEncode(rawName, Encoding.UTF8));
      }

      this.SendRequestNoResult(signedData, stream);
    }

    public void delete(string key)
    {
      SignedData signedData = this.GenerateSignedData(
        new SignDataOption
        {
          Key = key,
          Method = FssOperation.DELETE
        }
        );

      this.SendRequestNoResult(signedData);
    }

    public void Copy(string toKey, string fromKey)
    {
      SignedData signedData = this.GenerateSignedData(
        new SignDataOption
        {
          Key = toKey,
          Method = FssOperation.PUT,
          Metadata = new NameValueCollection
        {
          { HttpConsts.FSS_COPY_FILE_HEADER, fromKey }
        }
        });

      this.SendRequestNoResult(signedData);
    }

    public string GenerateObjectUrl(string key)
    {
      string path = this.GetResourcePath(key, false);
      Uri endPoint = this._publicEndPoint;
      Uri url = new Uri(endPoint, path);
      string objectUrl = url.ToString();
      return objectUrl;
    }

    /**
     *
     * @param {string} key
     * @param {any} option
     */
    public string GetSignatureUrl(string key, SignatureOption option)
    {
      string resource = this.GetResourcePath(key, true);
      SignedResource sign = this.SignatureResource(resource, option);

      // 默认为给外部使用，所以指定用外网地址
      Uri endPoint = this._publicEndPoint;
      string path = this.GetResourcePath(key, false);
      Uri url = new Uri(endPoint, resource);

      NameValueCollection query = new NameValueCollection();
      query.Add(HttpConsts.ACCESS_KEY_ID, this._config.AccessKeyId);
      query.Add(HttpConsts.EXPIRES, option.Expires.Value.ToString());
      query.Add(HttpConsts.SIGNATURE, sign.Signature);
      query.Add(sign.SubResource);

      // TODO: 生成URL
      string signedUrl = url.ToString();
      return signedUrl;
    }

    /**
     *
     * @param {string} key
     */
    public ObjectMeta Head(string key)
    {
      SignedData signedData = this.GenerateSignedData(new SignDataOption
      {
        Key = key,
        Method = FssOperation.HEAD
      });

      using (RequestResult result = this.SendRequest(signedData))
      {
        NameValueCollection meta = new NameValueCollection();
        foreach (string k in result.Headers)
        {
          if (k.StartsWith(HttpConsts.FSS_META_HEADER_PREFIX))
          {
            meta[k.Substring(11)] = result.Headers[k];
          }
        }

        return new ObjectMeta
        {
          Meta = meta,
          Headers = new NameValueCollection(result.Headers),
          StatusCode = result.StatusCode
        };
      }
    }

    /**
     *
     * @param {string} key
     */
    public NameValueCollection GetObjectMeta(string key)
    {
      SignedData signedData = this.GenerateSignedData(new SignDataOption
      {
        Key = key,
        Method = FssOperation.HEAD
      });

      using (RequestResult result = this.SendRequest(signedData))
      {
        NameValueCollection headers = result.Headers;
        return headers;
      }
    }

    private void SendRequestNoResult(SignedData signedData, Stream streamBody = null)
    {
      RequestResult result = null;
      try
      {
        result = this.SendRequest(signedData, streamBody);
      }
      finally
      {
        result.Close();
      }
    }

    private RequestResult SendRequest(SignedData signedData, Stream streamBody = null)
    {
      Uri resourceUri = signedData.TargetUrl;
      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resourceUri);
      request.Accept = "application/xml,*/*";
      request.Headers.Add("Accept-Language", "zh-CN");
      request.KeepAlive = true;
      request.Headers.Add(HttpRequestHeader.KeepAlive, "3000");
      request.Method = signedData.Method.ToString();
      request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

      foreach (DictionaryEntry entry in signedData.Headers)
      {
        string key = (string)entry.Key;
        object value = entry.Value;
        switch (key)
        {
          case HttpConsts.CONTENT_TYPE:
            request.ContentType = (string)value;
            break;
          case HttpConsts.DATE:
            request.Date = (DateTime)value;
            break;
          default:
            request.Headers[key] = (string)value;
            break;
        }
      }

      if (streamBody != null)
      {
        streamBody.CopyTo(request.GetRequestStream());
      }

      try
      {
        // Console.WriteLine("start get..........");
        // Console.WriteLine(resourceUri.ToString());
        HttpWebResponse response = request.GetResponse() as HttpWebResponse;
        // Console.WriteLine("finish get..........");
        return new RequestResult(response);
      }
      catch (WebException ex)
      {
        // Console.WriteLine("error..........");
        HttpWebResponse response = ex.Response as HttpWebResponse;
        return new RequestResult(response);
      }
    }

    /**
     * 使用header传递签名方式生成签名数据
     *
     * @param {SignDataOption} option
     */
    private SignedData GenerateSignedData(SignDataOption option)
    {
      FssOperation method = option.Method;
      string key = option.Key;
      string resource = this.GetResourcePath(key, true);

      Hashtable headers = new Hashtable();
      if (option.Metadata != null)
      {
        foreach (string name in option.Metadata)
        {
          // 全部转换为小写
          string lowerName = name.ToLower();
          headers[lowerName] = option.Metadata[name];
        }
      }

      SignatureOption opts = new SignatureOption(method, option.ContentType);
      opts.Date = DateTime.Now;
      opts.Metadata = option.Metadata;
      SignedResource sign = this.SignatureResource(resource, opts);
      headers.Add(HttpConsts.AUTHORIZATION,
        string.Format("FSS {0}:{1}", this._config.AccessKeyId, sign.Signature));
      UriBuilder builder = new UriBuilder(this._defaultEndpoint);
      builder.Path = builder.Path + GetResourcePath(key, false);
      builder.Query = ToQueryString(sign.SubResource);
      // 拼成服务端需要的地址
      Uri targetUrl = builder.Uri;
      headers.Add(HttpConsts.CONTENT_TYPE, opts.ContentType);
      headers.Add(HttpConsts.DATE, opts.Date);

      return new SignedData
      {
        TargetUrl = targetUrl,
        Method = option.Method,
        Headers = headers
      };
    }

    private string GetResourcePath(string key, bool addPrefix)
    {
      string prefix = addPrefix ? "/" : string.Empty;
      return prefix + this._config.BucketName + "/" + key;
    }

    private SignedResource SignatureResource(string resource, SignatureOption option)
    {
      NameValueCollection subResource = SignUtility.BuildSubResource(option);
      string canonicalString = SignUtility.BuildCanonicalString(resource, option, subResource);
      HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(this._config.AccessKeySecret));
      byte[] byteText = hmac.ComputeHash(Encoding.UTF8.GetBytes(canonicalString));
      return new SignedResource
      {
        Signature = Convert.ToBase64String(byteText),
        SubResource = subResource,
        Expires = option.AbsoluteExpires
      };
    }
    private string ToQueryString(NameValueCollection nvc)
    {
      var array = (
          from key in nvc.AllKeys
          from value in nvc.GetValues(key)
          select string.Format(
            "{0}={1}",
            HttpUtility.UrlEncode(key),
            HttpUtility.UrlEncode(value))
          ).ToArray();
      return string.Join("&", array);
    }
  }
}
