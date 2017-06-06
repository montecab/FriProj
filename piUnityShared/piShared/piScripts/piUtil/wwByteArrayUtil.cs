using System.Collections.Generic;


public static class wwBA {

  // word of warning: casting a float or double to an unsigned type (eg ushort, uint, byte) has undefined results.
  // instead, cast first to a signed integral type. at least, when compiling to C/C++.
  // I haven't been able to determine behavior for C# itself.

  public static byte mergeNibbles(byte highNibble, byte lowNibble) {
    byte h = (byte)((highNibble << 4) & 0xf0);
    byte l = (byte)((lowNibble      ) & 0x0f);
    return (byte)(h | l);
  }

  public static byte[] append(byte[] appendThis, byte[] toThis) {
    byte[] ret;

    if (toThis == null) {
      ret = new byte[appendThis.Length];
      appendThis.CopyTo(ret, 0);
      return ret;
    }

    ret = new byte[appendThis.Length + toThis.Length];

    toThis    .CopyTo(ret, 0);
    appendThis.CopyTo(ret, toThis.Length);

    return ret;
  }

  public static byte[] toByteArrayV(byte[] vals) {
    return vals;
  }

  public static byte[] toByteArrayV(List<byte[]> vals) {
    int byteCount = 0;
    foreach (byte[] ba in vals) {
      byteCount += ba.Length;
    }

    byte[] ret = new byte[byteCount];

    int offset = 0;
    foreach(byte[] ba in vals) {
      System.Buffer.BlockCopy(ba, 0, ret, offset, ba.Length);
      offset += ba.Length;
    }

    return ret;
  }

  public static byte[] toByteArray(string val) {
    byte[] ret = new byte[val.Length];
    for (int n = 0; n < val.Length; ++n) {
      ret[n] = (byte)(val[n]);
    }
    return ret;
  }

  public static byte[] toByteArray(bool val) {
    return toByteArray1(val ? (byte)1 : (byte)0);
  }

  public static byte[] toByteArray1(byte val) {
    byte[] ret = new byte[1];
    ret[0] = val;
    return ret;
  }

  public static byte[] toByteArray2(short val) {
    return toByteArray2((ushort)val);
  }

  public static byte[] toByteArray2(ushort val) {
    byte[] ret = new byte[2];
    ret[0] = (byte)((val & (ushort)0xff00) >> 8);
    ret[1] = (byte)((val & (ushort)0x00ff) >> 0);
    return ret;
  }

  public static byte[] toByteArray4(uint val) {
    byte[] ret = new byte[4];
    ret[0] = (byte)((val & (uint)0xff000000) >> 24);
    ret[1] = (byte)((val & (uint)0x00ff0000) >> 16);
    ret[2] = (byte)((val & (uint)0x0000ff00) >>  8);
    ret[3] = (byte)((val & (uint)0x000000ff) >>  0);
    return ret;
  }

  public static byte[] toByteArray5(ulong val) {
    byte[] ret = new byte[5];
    ret[0] = (byte)((val & (ulong)0xff00000000) >> 32);
    ret[1] = (byte)((val & (ulong)0x00ff000000) >> 24);
    ret[2] = (byte)((val & (ulong)0x0000ff0000) >> 16);
    ret[3] = (byte)((val & (ulong)0x000000ff00) >>  8);
    ret[4] = (byte)((val & (ulong)0x00000000ff) >>  0);
    return ret;
  }

  public static uint toUintFloatFloat(float f1, float f2) {
    int i1 = (int)(f1);
    int i2 = (int)(f2);
    uint ret = (uint)(((i1 & 0xffff) << 16) | (i2 & 0xffff));
    return ret;
  }

  public static byte[] toByteArrayRange(wwRange val) {
    return toByteArrayRange(val, false);
  }

  public static byte[] toByteArrayRange(wwRange val, bool invert) {
    int min  = (int)(val.Min * (float)((1 << 12) - 1));
    int max  = (int)(val.Max * (float)((1 << 12) - 1));
    if (invert) {
      int tmp = min;
      min = max;
      max = tmp;
    }
    byte b1  = (byte)(min >> 4);
    byte b2h = (byte)((min << 4) & 0xf0);
    byte b2l = (byte)((max >> 8) & 0x0f);
    byte b2  = (byte)(b2h | b2l);
    byte b3  = (byte)(max & 0xff);
    byte[] ret = new byte[] {b1, b2, b3};
    return ret;
  }

  public static byte[] toByteArray3(float f1, float f2, float f3) {
    byte b1 = (byte)(f1 * 255.0f);
    byte b2 = (byte)(f2 * 255.0f);
    byte b3 = (byte)(f3 * 255.0f);
    return new byte[] {b1, b2, b3};
  }
}
