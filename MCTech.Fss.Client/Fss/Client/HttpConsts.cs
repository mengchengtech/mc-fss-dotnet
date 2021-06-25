namespace MCTech.Fss.Client
{
  public static class HttpConsts
  {
    //  public const string ContentMd5 = "Content-MD5";

    public const string AUTHORIZATION = "authorization";
    public const string DATE = "date";
    public const string CONTENT_TYPE = "content-type";
    public const string EXPIRES = "Expires";

    public const string SIGNATURE = "Signature";

    public const string ACCESS_KEY_ID = "FSSAccessKeyId";

    public const string CONTENT_DISPOSITION = "content-disposition";

    public const string FSS_PREFIX = "x-fss-";

    public const string FSS_META_HEADER_PREFIX = FSS_PREFIX + "meta-";

    public const string FSS_COPY_FILE_HEADER = FSS_PREFIX + "copy-source";

    public const string FSS_PROCESS = FSS_PREFIX + "process";
  }
}