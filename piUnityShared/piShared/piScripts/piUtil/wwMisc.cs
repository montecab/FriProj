using System.Collections.Generic;
using System;

// miscellaneous utilities which are not dependent on Unity or Robots.

public static class wwDoOncePerTypeVal<T> {
  private static HashSet<T> doneThese = new HashSet<T>();
  
  public static bool doIt(T val) {
    bool ret = !doneThese.Contains(val);
    doneThese.Add(val);
    return ret;
  }
}

public static class wwClassUtils {
  public static List<Type> getAllSubclasses(Type baseClass) {
    List<Type> ret = new List<Type>();
    foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
        foreach (Type t in assembly.GetTypes()) {
          if (t.IsSubclassOf(baseClass)) {
            ret.Add(t);
          }
        }
      }
    return ret;
  }
}


public static class wwCollectionUtils {
  public static T2 dictSafeAccess<T1, T2>(Dictionary<T1, T2> dict, T1 key, T2 defaultVal, bool complain = true) {
    if (dict == null) {
      if (complain) {
        WWLog.logError("null Dictionary<" + typeof(T1).ToString() + ", " + typeof(T2).ToString() + ">");
      }
      return defaultVal;
    }
    
    if (dict.ContainsKey(key)) {
      return dict[key];
    }
    
    if (complain) {
      WWLog.logError("in Dictionary<" + typeof(T1).ToString() + ", " + typeof(T2).ToString() + ">, no such key: " + key.ToString() + ", using default: " + defaultVal.ToString());
    }
    
    return defaultVal;
  }
  
  public static Dictionary<T2, T1> invertDictionary<T1, T2>(Dictionary<T1, T2> dict, bool complain = true) {
    Dictionary<T2, T1> ret =new Dictionary<T2, T1>();
    
    foreach (T1 key in dict.Keys) {
      T2 val = dict[key];
      if (ret.ContainsKey(val) && complain) {
        WWLog.logError("invert dictionary: duplicate new key: " + val.ToString() + "; stomping old value.");
      }
      
      ret[val] = key;
    }
    
    return ret;
  }
  
  public static bool test() {
    Dictionary<string, int> src = new Dictionary<string, int> {
      {"one"         , 1},
      {"two"         , 2},
      {"three"       , 3},
      {"one plus one", 2},
    };
    
    bool passed = true;
    
    // test basic safe lookup
    passed &= (wwCollectionUtils.dictSafeAccess<string, int>(src, "two", 0, false) == 2);
    
    // test missing lookup
    passed &= (wwCollectionUtils.dictSafeAccess<string, int>(src, "four", 0, false) == 0);
    
    // invert
    Dictionary<int, string> inv = wwCollectionUtils.invertDictionary<string, int>(src, false);
    
    // inverted basic membership
    passed &= (inv[3] == "three");
    
    // inverted duplicate membership
    passed &= (inv[2] == "one plus one");
      
    

    if (!passed) {
      WWLog.logError("tests failed.");
    }    
    
    return passed;
  }
}

