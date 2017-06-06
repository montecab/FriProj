using UnityEngine;
using WW.SimpleJSON;

namespace WW {
  public static class wwSimpleJsonUtils {

    /// <summary>
    /// if jsn is an array, return a random element from the array.
    /// otherwise, return jsn itself.
    /// if it is an empty array, returns null.
    /// </summary>
    /// <returns>an element</returns>
    /// <param name="jsn">either a JSONArray or a JSONNode of some other sort</param>
    public static JSONNode randomElement(JSONNode jsn, string butNotThis = null) {
      if (jsn is JSONArray) {
        JSONArray jsa = jsn.AsArray;
        if (jsa.Count == 0) {
          return null;
        }

        if (jsa.Count == 1) {
          return jsa[0];
        }

        if (butNotThis == null) {
          return jsa[Random.Range(0, jsa.Count)];
        }
        else {
          JSONNode candidate = null;
          for (int n = 0; n < 10; ++n) {
            candidate = jsa[Random.Range(0, jsa.Count)];
            if (!candidate.ToString().Equals(butNotThis)) {
              break;
            }
          }
          return candidate;
        }
      }
      else {
        return jsn;
      }
    }

    public static JSONClass loadFile(string path) {
      path = path.Replace(".json", "");
      TextAsset ta = Resources.Load<TextAsset>(path);
      if (ta == null) {
        WWLog.logError("unable to load resource file: " + path);
        return null;
      }
      JSONNode jsn = JSON.Parse(ta.text);
      if (jsn == null) {
        WWLog.logError("unable to parse json resource file: " + path);
        return null;
      }

      JSONClass jsc = jsn.AsObject;
      if (jsc == null) {
        WWLog.logError("json file top-level item is not a dictionary: " + path);
        return null;
      }

      return jsc;
    }
  }
}