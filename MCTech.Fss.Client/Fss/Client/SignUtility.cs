using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

using MCTech.Fss.Client.Exceptions;

namespace MCTech.Fss.Client
{
  internal class SignUtility
  {
    public static NameValueCollection BuildSubResource(SignatureOption option)
    {
      NameValueCollection subResource = new NameValueCollection();
      if (!string.IsNullOrWhiteSpace(option.Process))
      {
        subResource[HttpConsts.FSS_PROCESS] = option.Process;
      }

      if (option.Response != null)
      {
        foreach (string key in option.Response)
        {
          string lowerKey = "response-" + key.ToLower();
          subResource[lowerKey] = option.Response[key];
        }
      }
      return subResource;
    }
    public static string BuildCanonicalString(string resource, SignatureOption option, NameValueCollection subResource)
    {
      List<string> itemsToSign = new List<string>();
      itemsToSign.Add(option.Method.ToString());
      itemsToSign.Add(""); // MD5
      itemsToSign.Add(option.ContentType);

      if (option.AbsoluteExpires.HasValue)
      {
        itemsToSign.Add(option.AbsoluteExpires.ToString());
      }
      else if (option.Date.HasValue)
      {
        itemsToSign.Add(option.Date.Value.ToUniversalTime().ToString("r"));
      }
      else
      {
        throw new MCTechFssException("Expires和Date属性不能都为null");
      }

      if (option.Metadata != null)
      {
        List<string> keys = option.Metadata.AllKeys
          // 只加签特定meta信息
          .Where(key => key.StartsWith(HttpConsts.FSS_PREFIX))
          .OrderBy(key => key, StringComparer.InvariantCulture)
          .ToList();

        foreach (string key in keys)
        {
          itemsToSign.Add(key + ":" + option.Metadata[key]);
        }
      }

      // Add canonical resource
      string canonicalizedResource = BuildCanonicalizedResource(resource, subResource);
      itemsToSign.Add(canonicalizedResource);

      string content = string.Join("\n", itemsToSign);
      // Console.WriteLine(content);
      return content;
    }

    private static string BuildCanonicalizedResource(string canonicalizedResource, NameValueCollection subResource)
    {
      List<string> parameterNames = subResource.AllKeys
          .OrderBy(key => key, StringComparer.InvariantCulture)
          .ToList();

      char separator = '?';
      StringBuilder builder = new StringBuilder(canonicalizedResource);
      foreach (string paramName in parameterNames)
      {
        builder.Append(separator);
        builder.Append(paramName);
        string paramValue = subResource[paramName];
        if (!string.IsNullOrEmpty(paramValue))
        {
          builder.Append("=").Append(paramValue);
        }

        separator = '&';
      }
      return builder.ToString();
    }
  }
}