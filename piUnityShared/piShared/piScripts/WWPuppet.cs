using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine; // debug log

namespace WW {

  public static class Puppet {
    #if UNITY_IPHONE
    const string LIBNAME_API = "__Internal";
    #else // Mac Editor and Mac Standalone
    const string LIBNAME_API = "APIObjectiveC";
    #endif
        
    #if UNITY_IPHONE
    // no platform-dependent members for iphone yet.
    #elif UNITY_ANDROID
    private static AndroidJavaClass jc = new AndroidJavaClass("com.makewonder.wwUnityWrapper"); 
    #endif

    #if UNITY_ANDROID

    static void WWPuppetCore_clearAll            (){}  
    static void WWPuppetCore_init                (StringBuilder key){}
    static float WWPuppetCore_recommendedInterval(){return 0;}
    static void WWPuppetCore_addFrame            (StringBuilder key, float time, float bodyX, float bodyY, float bodyRadians, float headPan, float headTilt){}
    static float  WWPuppetCore_duration          (StringBuilder key){return 0;}
    static int  WWPuppetCore_sequenceByteCount   (StringBuilder key){return 0;}
    static bool WWPuppetCore_sequence            (StringBuilder key, StringBuilder buffer, int bufferSize){return false;}
    static int  WWPuppetCore_subsequenceByteCount(StringBuilder key, float timeBegin, float timeFinish){return 0;}

    static bool WWPuppetCore_subsequence         (StringBuilder key, float timeBegin, float timeFinish, StringBuilder buffer, int bufferSize){return false;}

    static int  WWPuppetCore_convertANByteCount  (StringBuilder jsonString, int robotType){return 0;}

    static bool WWPuppetCore_convertAN           (StringBuilder jsonString, int robotType, byte[] buffer, int bufferSize){return false;}

    static bool WWPuppetCore_fullPathForSlot     (StringBuilder buffer, int bufferSize, int index, bool includeExtension){return false;}

    static bool WWPuppetCore_justDirForSlot      (StringBuilder buffer, int bufferSize, int index){return false;}

    static bool WWPuppetCore_justNameForSlot     (StringBuilder buffer, int bufferSize, int index){return false;}

    static bool WWPuppetCore_setLogFolder        (StringBuilder path){return false;}

    #else

    // these go straight to HAL !  skips WWCWrapper entirely.   can this be done in java also ?
    // here are the C functions in the interface:
    // void   WWPuppetCore_clearAll            ();
    // void   WWPuppetCore_init                (const char* key);
    // float  WWPuppetCore_recommendedInterval ()
    // void   WWPuppetCore_addFrame            (const char* key, float time, float bodyX, float bodyY, float bodyRadians, float headPan, float headTilt);
    // float  WWPuppetCore_duration            (const char* key);
    // size_t WWPuppetCore_sequenceByteCount   (const char* key);
    // bool   WWPuppetCore_sequence            (const char* key, char* buffer, size_t bufferSize);
    // size_t WWPuppetCore_subsequenceByteCount(const char* key, float timeBegin, float timeFinish);
    // bool   WWPuppetCore_subsequence         (const char* key, float timeBegin, float timeFinish, char* buffer, size_t bufferSize);
    // size_t WWPuppetCore_convertANByteCount  (const char* jsonString, WWRobotType robotType);
    // bool   WWPuppetCore_convertAN           (const char* jsonString, WWRobotType robotType, uint8_t* buffer, size_t bufferSize);
    // bool   WWPuppetCore_fullPathForSlot     (char* buffer, size_t bufferSize, size_t index, bool includeExtension);
    // bool   WWPuppetCore_justDirForSlot      (char* buffer, size_t bufferSize, size_t index);
    // bool   WWPuppetCore_justNameForSlot     (char* buffer, size_t bufferSize, size_t index);
    // void   WWPuppetCore_setLogFolder        (const char* path);

    [DllImport (LIBNAME_API)] private extern static
    void WWPuppetCore_clearAll            ();

    [DllImport (LIBNAME_API)] private extern static
    void WWPuppetCore_init                (StringBuilder key);

    [DllImport (LIBNAME_API)] private extern static
    float WWPuppetCore_recommendedInterval();

    [DllImport (LIBNAME_API)] private extern static
    void WWPuppetCore_addFrame            (StringBuilder key, float time, float bodyX, float bodyY, float bodyRadians, float headPan, float headTilt);

    [DllImport (LIBNAME_API)] private extern static
    float  WWPuppetCore_duration          (StringBuilder key);

    [DllImport (LIBNAME_API)] private extern static
    int  WWPuppetCore_sequenceByteCount   (StringBuilder key);

    [DllImport (LIBNAME_API)] private extern static
    bool WWPuppetCore_sequence            (StringBuilder key, StringBuilder buffer, int bufferSize);

    [DllImport (LIBNAME_API)] private extern static
    int  WWPuppetCore_subsequenceByteCount(StringBuilder key, float timeBegin, float timeFinish);

    [DllImport (LIBNAME_API)] private extern static
    bool WWPuppetCore_subsequence         (StringBuilder key, float timeBegin, float timeFinish, StringBuilder buffer, int bufferSize);

    [DllImport (LIBNAME_API)] private extern static
    int  WWPuppetCore_convertANByteCount  (StringBuilder jsonString, int robotType);

    [DllImport (LIBNAME_API)] private extern static
    bool WWPuppetCore_convertAN           (StringBuilder jsonString, int robotType, byte[] buffer, int bufferSize);

    [DllImport (LIBNAME_API)] private extern static
    bool WWPuppetCore_fullPathForSlot     (StringBuilder buffer, int bufferSize, int index, bool includeExtension);

    [DllImport (LIBNAME_API)] private extern static
    bool WWPuppetCore_justDirForSlot      (StringBuilder buffer, int bufferSize, int index);

    [DllImport (LIBNAME_API)] private extern static
    bool WWPuppetCore_justNameForSlot     (StringBuilder buffer, int bufferSize, int index);

    [DllImport (LIBNAME_API)] private extern static
    bool WWPuppetCore_setLogFolder        (StringBuilder path);
    #endif

    public static void clearAll() {
      WWPuppetCore_clearAll();
    }

    public static void init(string key) {
      WWPuppetCore_init(new StringBuilder(key));
    }

    public static float recommendedInterval {
      get {
        return WWPuppetCore_recommendedInterval();
      }
    }

    public static void addFrame(string key, float time, piBotBo robot) {
      piBotComponentBodyPose bp = robot.BodyPoseSensor;
      float hp = robot.HeadPanSensor.angle.valTarget;
      float ht = robot.HeadTiltSensor.angle.valTarget;
      WWPuppetCore_addFrame(new StringBuilder(key), time, bp.x, bp.y, bp.radians, hp, ht);
    }

    public static float duration(string key) {
      StringBuilder sbKey = new StringBuilder(key);
      return WWPuppetCore_duration(sbKey);
    }

    public static string sequence(string key) {
      // first get the size of the buffer we need
      StringBuilder sbKey = new StringBuilder(key);
      int bufSize = WWPuppetCore_sequenceByteCount(sbKey);
      StringBuilder sbBuffer = new StringBuilder(bufSize);
      if (!WWPuppetCore_sequence(sbKey, sbBuffer, sbBuffer.Capacity)) {
        Debug.LogError("failed to retrieve sequence " + key);
        return null;
      }
      else {
        return sbBuffer.ToString();
      }
    }

    public static string subsequence(string key, float timeBegin, float timeFinish) {
      // first get the size of the buffer we need
      StringBuilder sbKey = new StringBuilder(key);
      int bufSize = WWPuppetCore_subsequenceByteCount(sbKey, timeBegin, timeFinish);
      StringBuilder sbBuffer = new StringBuilder(bufSize);
      if (!WWPuppetCore_subsequence(sbKey, timeBegin, timeFinish, sbBuffer, sbBuffer.Capacity)) {
        Debug.LogError("failed to retrieve subsequence " + key + " [" + timeBegin + ", " + timeFinish + "]");
        return null;
      }
      else {
        return sbBuffer.ToString();
      }
    }

    public static byte[] convertAN(string jsonString, piRobotType robotType) {
      StringBuilder sbJson = new StringBuilder(jsonString);
      int bufferSize = WWPuppetCore_convertANByteCount(sbJson, (int)robotType);
      byte[] buffer = new byte[bufferSize];
      if (!WWPuppetCore_convertAN(sbJson, (int)robotType, buffer, buffer.Length)) {
        Debug.LogError("failed to convert sequence");
        return null;
      }
      else {
        return buffer;
      }
    }

    public static string fullPathForSlot(int index, bool includeSuffix) {
      StringBuilder str = new StringBuilder(128);
      WWPuppetCore_fullPathForSlot(str, str.Capacity, index, includeSuffix);
      return str.ToString();
    }

    public static string justDirForSlot(int index) {
      StringBuilder str = new StringBuilder(128);
      WWPuppetCore_justDirForSlot(str, str.Capacity, index);
      return str.ToString();
    }

    public static string justNameForSlot(int index) {
      StringBuilder str = new StringBuilder(128);
      WWPuppetCore_justNameForSlot(str, str.Capacity, index);
      return str.ToString();
    }
  }
}