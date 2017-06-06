using UnityEngine;
using System.Collections.Generic;

#if UNITY_5_4_OR_NEWER
using UnityEngine.Networking;
#else
using UnityEngine.Experimental.Networking;
#endif


namespace WW {
  
  public static class wwWWWUtil {
    public static long getResponseCode(UnityWebRequest request, ref string responseString) {
      responseString = request.responseCode.ToString();
      return request.responseCode;
    }
  }
}
