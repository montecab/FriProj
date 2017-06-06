using System;
using System.Collections.Generic;
using UnityEngine;
using PI;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class piUnityUtils {

	private static long _startTick = System.DateTime.Now.Ticks;
  private static float WIDE_SCREEN_RATIO = 1.5f;

	public static Color unityColor(piColorRGB value) {
		Color ret = new Color();
		ret.r = unityLed(value.r);
		ret.g = unityLed(value.g);
		ret.b = unityLed(value.b);
		ret.a = 1;
		return ret;
	}

  /// <summary>
  /// converts a 6-digit or 8-digit hex string into a unity color
  /// </summary>
  /// <returns>The color, if valid, or Magenta if not</returns>
  /// <param name="hexString">Hex string. formats: "RRGGBB", "RRGGBBAA", "#RRGGBB", "#RRGGBBAA", "RGB", "RGBA", "#RGB", "#RGBA"</param>
  public static Color unityColor(string hexString) {
    bool allGood = true;
    string s = "";

    if (hexString == null) {
      allGood = false;
    }
    else {
      s = hexString.Replace("#", "");
    }

    int len = s.Length;
    string sr = "";
    string sg = "";
    string sb = "";
    string sa = "ff";

    switch (len) {
      case 3:
        sr = s.Substring(0, 1);
        sg = s.Substring(1, 1);
        sb = s.Substring(2, 1);
        sr = sr + sr;
        sg = sg + sg;
        sb = sb + sb;
        break;
      case 4:
        sr = s.Substring(0, 1);
        sg = s.Substring(1, 1);
        sb = s.Substring(2, 1);
        sa = s.Substring(3, 1);
        sr = sr + sr;
        sg = sg + sg;
        sb = sb + sb;
        sa = sa + sa;
        break;
      case 6:
        sr = s.Substring(0, 2);
        sg = s.Substring(2, 2);
        sb = s.Substring(4, 2);
        break;
      case 8:
        sr = s.Substring(0, 2);
        sg = s.Substring(2, 2);
        sb = s.Substring(4, 2);
        sa = s.Substring(6, 2);
        break;
      default:
        allGood = false;
        break;
    }


    if (allGood) {
      int ir, ig, ib, ia;
      allGood = Int32.TryParse(sr, System.Globalization.NumberStyles.AllowHexSpecifier, null, out ir) && allGood;
      allGood = Int32.TryParse(sg, System.Globalization.NumberStyles.AllowHexSpecifier, null, out ig) && allGood;
      allGood = Int32.TryParse(sb, System.Globalization.NumberStyles.AllowHexSpecifier, null, out ib) && allGood;
      allGood = Int32.TryParse(sa, System.Globalization.NumberStyles.AllowHexSpecifier, null, out ia) && allGood;
      return new Color((float)ir/255.0f, (float)ig/255.0f, (float)ib/255.0f, (float)ia/255.0f);
    }
    else {
      WWLog.logError("invalid hex color: " + hexString);
      return new Color(1f, 0f, 1f, 1f);
    }
  }

  public static bool IsWideScreen(){
    float currentRation = Screen.width / (float)Screen.height;
    return currentRation > WIDE_SCREEN_RATIO;
  }
	public static float unityLed(byte value) {
		return (float)value / 255.0f;
	}
	
	// wall-seconds since this static class was initialized.
	// (wall-seconds are different than Time.time, because they keep moving forward during a single execution chain)
	public static double SessionWallTime {
		get {
			long dTicks = System.DateTime.Now.Ticks - _startTick;
			return (double)dTicks / 10000000.0;
		}
	}
  
  public static double SystemWallTime {
    get {
      return (double)System.DateTime.Now.Ticks / 10000000.0;
    }
  }
  
  public static T ensureComponent<T>(GameObject gameObject) where T:Component {
    T ret = gameObject.GetComponent<T>();
    if (ret == null) {
      ret = gameObject.AddComponent<T>();
    }
    
    return ret;
  }

  /// <summary>
  /// destroys all children of this transform.
  /// does NOT destroy the transform itself.
  /// </summary>
  /// <param name="t">the transform</param>
  public static void destroyAllChildren(Transform t) {
    destroyAllChildren(t, new HashSet<Transform>());
  }

  public static void destroyAllChildren(Transform t, HashSet<Transform> butSpareThese) {
    for (int n = t.childCount - 1; n >= 0; --n) {
      Transform tChild = t.GetChild(n);
      if (!butSpareThese.Contains(tChild)) {
        GameObject.Destroy(tChild.gameObject);
      }
    }
  }

  public static void SetGameObjectsActive(MonoBehaviour[] elements, bool value) {
    foreach (MonoBehaviour mb in elements) {
      mb.gameObject.SetActive(value);
    }
  }

  public static string pathToTransform(Transform trn) {
    Transform parent = trn.parent;
    if (parent == null) {
      return trn.name;
    }
    else {
      return pathToTransform(parent) + " | " + trn.name;
    }
  }
  
  public static RuntimePlatform CompilePlatform {
    get {
      #if UNITY_ANDROID
        return RuntimePlatform.Android;
      #elif UNITY_WEBGL
        return RuntimePlatform.WebGLPlayer;
      #elif UNITY_IPHONE
        return RuntimePlatform.IPhonePlayer;
      #elif UNITY_EDITOR_OSX
        return RuntimePlatform.OSXEditor;   // order of these last handful matters
      #elif UNITY_STANDALONE_OSX
        return RuntimePlatform.OSXPlayer;
      #endif
		return RuntimePlatform.WindowsPlayer;
    }
  }
  
  public static bool CheckCompilePlatformEqualsRuntimePlatform(bool quitIfNot) {
    bool ret;
    
    if (Application.platform == CompilePlatform) {
      ret = true;
    }
    else {
      ret = false;
      string s = "RuntimePlatform is " + Application.platform.ToString() + " but CompilePlatform is " + CompilePlatform.ToString() + ".";
      Debug.LogError(s);
      if (quitIfNot) {
        #if UNITY_EDITOR
        EditorUtility.DisplayDialog("Wrong Platform!", s, "ok");
        #else
        Application.Quit();
        #endif
        }
    }
    
    return ret;
  }

  // this could be the case in two situations:
  // either the instance has been destroyed,
  // or the Delegate is a static class method.
  public static bool isDelegateTargetNull(Delegate d) {
    if (d.Target == null) {
      return true;
    }

    // http://answers.unity3d.com/questions/1167880/delegatetarget-null-check-fails-when-its-actually.html
    // http://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it
    if (d.Target.Equals(null)) {
      return true;
    }

    return false;
  }

  public static bool Test() {
    bool passed = true;

    passed = passed & test_unityColor();

    return passed;
  }

  private static bool test_unityColor() {
    Dictionary<string, Color> testCases = new Dictionary<string, Color> {
      {"f00"    , new Color(1f, 0f, 0f, 1f)},
      {"F00"    , new Color(1f, 0f, 0f, 1f)},
      {"#F00"   , new Color(1f, 0f, 0f, 1f)},
      {"#FF0000", new Color(1f, 0f, 0f, 1f)},
      {"FF0000" , new Color(1f, 0f, 0f, 1f)},
      {"FFFF00" , new Color(1f, 1f, 0f, 1f)},
      {"00FFFF" , new Color(0f, 1f, 1f, 1f)},
      {"fff8"   , new Color(1f, 1f, 1f, 0.53333333333333333f)},
      {"fff8d"  , new Color(1f, 0f, 1f, 1f)}, // invalid
      {"#"      , new Color(1f, 0f, 1f, 1f)}, // invalid
      {"1234567", new Color(1f, 0f, 1f, 1f)}, // invalid
    };

    bool passed = true;

    foreach (string key in testCases.Keys) {
      Color result = unityColor(key);
      bool thisTestPass = result.Equals(testCases[key]);
      if (!thisTestPass) {
        WWLog.logError("test failed: " + key + " not parsed correctly. got: " + result.ToString() +  " but expected " + testCases[key].ToString());
      }
      passed = passed && thisTestPass;
    }

    return passed;
  }
}

