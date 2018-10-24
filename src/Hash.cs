namespace HashGenerator {
  /// <summary> Hash class to store generated hashes </summary>
  class Hash {
    /// <summary> Hash constructor </summary>
    public Hash() { md5 = sha1 = sha256 = sha384 = sha512 = ""; }

    /// <summary> Resets all hash values to nil </summary>
    public void Reset() { md5 = sha1 = sha256 = sha384 = sha512 = ""; }

    /// <summary> Gets or sets MD5 hash </summary>
    public string MD5 {
      get { return md5; }
      set { md5 = value; }
    }

    /// <summary> Gets or sets SHA1 hash </summary>
    public string SHA1 {
      get { return sha1; }
      set { sha1 = value; }
    }

    /// <summary> Gets or sets SHA256 hash </summary>
    public string SHA256 {
      get { return sha256; }
      set { sha256 = value; }
    }

    /// <summary> Gets or sets SHA384 hash </summary>
    public string SHA384 {
      get { return sha384; }
      set { sha384 = value; }
    }

    /// <summary> Gets or sets SHA512 hash </summary>
    public string SHA512 {
      get { return sha512; }
      set { sha512 = value; }
    }

    private string md5;
    private string sha1;
    private string sha256;
    private string sha384;
    private string sha512;
  }
}
