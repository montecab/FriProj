namespace WW {
  /// <summary>
  /// various C# utilities.
  /// these should not use Unity things, except possibly WWLog.
  /// </summary>
  public static class wwCSUtil {
    /// <summary>
    /// Swap the specified a and b.
    /// </summary>
    /// <param name="a">one item.</param>
    /// <param name="b">another item.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public static void swap<T>(ref T a, ref T b) {
      T tmp = a;
      a = b;
      b = tmp;
    }

    /// <summary>
    /// concatenates a list of arrays
    /// </summary>
    /// <returns>a new array</returns>
    /// <param name="arrays">Arrays.</param>
    /// <typeparam name="T">the type contained by the arrays.</typeparam>
    public static T[] concatenateToNew<T>(params T[][] arrays) {
      int totalLength = 0;
      foreach (T[] array in arrays) {
        totalLength += array.Length;
      }

      T[] ret = new T[totalLength];

      int startAt = 0;
      foreach (T[] array in arrays) {
        array.CopyTo(ret, startAt);
        startAt += array.Length;
      }
      return ret;
    }

    // note: this does not throw the exception itself because at compile-time
    // there's no way for the compiler to know this routine always throws an exception,
    // and thus can give false "not every path returns a value" warnings in the calling code.
    public static T logAndCreateException<T>(string msg) where T: System.Exception, new() {
      T e = (T)System.Activator.CreateInstance(typeof(T), new string[] {msg});
//    T e = new T(msg); // grrr. can't do this.
      WWLog.logError(msg);
      return e;
    }

  }
}
