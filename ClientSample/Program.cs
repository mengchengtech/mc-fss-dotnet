using System;
using System.IO;
using System.Collections.Specialized;

using Newtonsoft.Json;
using MCTech.Fss.Client;

namespace ClientSample
{
  class Program
  {
    static void Main(string[] args)
    {
      MCFssClientConfig config = new MCFssClientConfig();
      config.BucketName = "iwop-dev";
      config.AccessKeyId = "AccessKeyId";
      config.AccessKeySecret = "AccessKeySecret";
      config.PrivateEndPoint = "http://dev.mctech.vip/fss/";
      config.PublicEndPoint = "http://dev.mctech.vip/fss/";


      Random rnd = new Random();
      String key = "java-client/de." + DateTime.Now.ToFileTimeUtc() + ".html";
      Console.WriteLine(key);
      MCFssClient client = new MCFssClient(config);
      {
        Console.WriteLine("====== put =========");
        NameValueCollection meta = new NameValueCollection();
        meta["x-fss-meta-module"] = "mod";
        FileStream stream = new FileStream("d:\\de.html", FileMode.Open);
        client.Put(key, null,

            stream,
            meta,
            "text/html"
        );
      }

      {
        Console.WriteLine("====== head =========");
        ObjectMeta meta = client.Head(key);
        Console.WriteLine(JsonConvert.SerializeObject(meta));
      }

      {
        Console.WriteLine("====== get object meta =========");
        NameValueCollection om = client.GetObjectMeta(key);
        Console.WriteLine(om);
      }

      {
        Console.WriteLine("====== get =========");
        RequestResult result = client.get(key);
        Console.WriteLine(result.GetContent());
      }

      {
        Console.WriteLine("====== get signature url =========");
        SignatureOption opts = new SignatureOption(FssOperation.GET);
        opts.Expires = 15 * 1000L;
        String accessUrl = client.GetSignatureUrl(key, opts);

        Console.WriteLine(accessUrl);
        Console.WriteLine("===================");
      }

      {
        Console.WriteLine("====== get signature url complex =========");
        SignatureOption opts = new SignatureOption(FssOperation.GET);
        NameValueCollection response = new NameValueCollection();
        response["content-disposition"] = "nnnnnnnnnnnnnn.jpg";
        opts.Response = response;
        opts.Process = "video/snapshot,t_7000,f_jpg,w_800,h_600,m_fast";
        opts.Expires = 15 * 1000L;
        String accessUrl = client.GetSignatureUrl("demo" + rnd.Next() + ".jpg", opts);
        Console.WriteLine((accessUrl));
      }

      {
        Console.WriteLine("====== generate object url =========");
        string objectUrl = client.GenerateObjectUrl(key);
        Console.WriteLine(objectUrl);
      }

      {
        Console.WriteLine("====== copy =========");
        String toKey = key + "." + rnd.Next() + ".copy.to";
        client.Copy(toKey, key);

        client.delete(key);
      }

      Console.WriteLine("====== finished =========");
    }
  }
}
