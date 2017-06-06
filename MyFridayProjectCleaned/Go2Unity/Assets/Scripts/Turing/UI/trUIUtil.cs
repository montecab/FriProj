using UnityEngine;
using System.Collections.Generic;
using Turing;

public static class trUIUtil {

  public static string normalizeUIText(string s) {
    string ret = s;
    ret = ret.Replace( "<B>",  "<b>");
    ret = ret.Replace("</B>", "</b>");
    ret = ret.Replace( "<I>",  "<i>");
    ret = ret.Replace("</I>", "</i>");
    
    return ret;
  }
  
  
  public static void normalizeUIText<T>(Dictionary<T, string> dict) {
    
    List<T> keys = new List<T>(dict.Keys);
    
    foreach (T key in keys) {
      dict[key] = trUIUtil.normalizeUIText(dict[key]);
    }
  }
  
  public static void normalizeUIText<T>(Dictionary<T, string[]> dict) {
    
    List<T> keys = new List<T>(dict.Keys);
    
    foreach (T key in keys) {
      string[] strings = dict[key];
      for (int n = 0; n < strings.Length; ++n) {
        strings[n] = trUIUtil.normalizeUIText(strings[n]);
      }
    }
  }
  
}
