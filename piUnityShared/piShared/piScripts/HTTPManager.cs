using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using WW.SimpleJSON;
using System.Threading;

namespace WW {
  public class HTTPManager :  Singleton<HTTPManager> {

    public enum requestType {
      GET,
      POST,
      PUT
    }

    public static class cHeaders {
      // keys
      public const string CONTENT_TYPE = "Content-Type";

      // values
      public const string APPLICATION_JSON = "application/json";
    }

    public  const float cTimeoutDefault          = 5.0f;
    public  const int   REQUEST_TIMEOUT_RESPONSE = 408;
    public  const int   REQUEST_CANCEL_RESPONSE  = 403;

    private const float STATUS_CHECK_SECONDS     = 0.2f;

    public bool AllowAnonymousProgressDelegates  = false;

    public class RequestInfo {
      public string          url            = ""; // have to retain this because request is corrupt after Dispose() is called
      public long            responseCode   = 0;
      public string          responseString = "";
      public UnityWebRequest request        = null;
      public float           timeoutMoment  = float.NaN;
      public bool            isRequestDone  = false;

      public bool isDone{
        get{
          return isRequestDone || isTimeout || isCanceled;
        }
      }
      public bool success {
        get{
          return (responseCode >= 200) && (responseCode < 300);
        }
      }
      public bool isCanceled {
        get{
          return responseCode == REQUEST_CANCEL_RESPONSE;
        }
      }

      public bool isTimeout{
        get{
          return responseCode == REQUEST_TIMEOUT_RESPONSE;
        }
      }

      public string DownloadedText {
        get {
          if (request == null) {
            WWLog.logError("bad request");
            return null;
          }
          else {
            return request.downloadHandler.text;
          }
        }
      }

      public string ResponseCodeAndString {
        get {
          return responseCode.ToString() + ": " + responseString;
        }
      }

      public override string ToString() {
        string ret = "";
        ret +=       "url: "              + url;
        ret += " " + "isDone: "           + isDone.ToString();
        ret += " " + "success: "          + success.ToString();
        ret += " " + "responseCode: "     + responseCode;
        ret += " " + "responseString: \"" + responseString + "\"";
        ret += " " + "isTimeout: "        + isTimeout.ToString();
        ret += " " + "isCanceled: "       + isCanceled.ToString();
        ret += " " + "error: "            + request.error;
        ret += " " + "response: "         + request.downloadHandler.text;
//        ret += " " + "body: "             + request.uploadHandler.data.
        return ret;
      }
    }
    
    public delegate void httpProgressDelegate(RequestInfo RI);

    public void cancelRequest(RequestInfo ri){
      ri.responseCode   = REQUEST_CANCEL_RESPONSE;
      ri.responseString = "REQUEST CANCELED";
    }

    #region basic http

    public RequestInfo NewRequest(requestType type, string url, string body, httpProgressDelegate upd, float timeout, Dictionary<string, string>headers) {

      if (!AllowAnonymousProgressDelegates) {
        if ((upd != null) && (piUnityUtils.isDelegateTargetNull(upd))) {
          // possible todo: if it's null at this point, then we could skip the null-check before calling it later on.
          Debug.LogError("httpProgressDelegate is null - likely a static method. this is not currently supported. ignoring URL: " + url);
          return null;
        }
      }

      UnityWebRequest request;

      switch (type) {
        case requestType.GET:
          if (!string.IsNullOrEmpty(body)) {
            Debug.LogError("body was provided to GET request - " + url);
          }
          request = new UnityWebRequest(url);
          request.downloadHandler = new DownloadHandlerBuffer();
          request.method          = UnityWebRequest.kHttpVerbGET;
          break;
        case requestType.POST:
          // http://answers.unity3d.com/questions/1163204/prevent-unitywebrequestpost-from-url-encoding-the.html
          request                 = new UnityWebRequest(url);
          request.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
          request.downloadHandler = new DownloadHandlerBuffer();
          request.method          = UnityWebRequest.kHttpVerbPOST;
          break;
        case requestType.PUT:
        // http://answers.unity3d.com/questions/1163204/prevent-unitywebrequestpost-from-url-encoding-the.html
          request                 = new UnityWebRequest(url);
          request.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
          request.downloadHandler = new DownloadHandlerBuffer();
          request.method          = UnityWebRequest.kHttpVerbPUT;
          break;
        default:
          Debug.LogError("unhandled request type: " + type.ToString());
          return null;
      }

      if (headers != null) {
        foreach (string key in headers.Keys) {
          request.SetRequestHeader(key, headers[key]);
        }
      }

      RequestInfo info = new RequestInfo();
      info.url = url;
      info.request = request;
      StartCoroutine(startRequest(info, upd, timeout));
      return info;
    }

    public RequestInfo Get(string url, httpProgressDelegate upd = null, float timeout = HTTPManager.cTimeoutDefault) {
      return NewRequest(requestType.GET, url, null, upd, timeout, null);
    }

    public RequestInfo Post(string url, string body, httpProgressDelegate upd = null, float timeout = HTTPManager.cTimeoutDefault, Dictionary<string, string>headers = null) {
      return NewRequest(requestType.POST, url, body, upd, timeout, headers);
    }

    public RequestInfo Put(string url, string body, httpProgressDelegate upd = null, float timeout = HTTPManager.cTimeoutDefault, Dictionary<string, string>headers = null) {
      return NewRequest(requestType.PUT, url, body, upd, timeout, headers);
    }

    #endregion basic http

    #region convenience wrappers


    private RequestInfo SendJson(requestType rt, string url, SimpleJSON.JSONNode jsn, httpProgressDelegate upd = null, float timeout = HTTPManager.cTimeoutDefault, Dictionary<string, string>headers = null) {
      if (headers == null) {
        headers = new Dictionary<string, string>();
      }
      if (headers.ContainsKey(cHeaders.CONTENT_TYPE)) {
        WWLog.logError("headers already contain a content-type: " + headers[cHeaders.CONTENT_TYPE]);
      }
      headers[cHeaders.CONTENT_TYPE] = cHeaders.APPLICATION_JSON;
      return NewRequest(rt, url, jsn.ToString(" "), upd, timeout, headers);
    }

    public RequestInfo PutJson(string url, SimpleJSON.JSONNode jsn, httpProgressDelegate upd = null, float timeout = HTTPManager.cTimeoutDefault, Dictionary<string, string>headers = null) {
      return SendJson(requestType.PUT, url, jsn, upd, timeout, headers);
    }

    public RequestInfo PostJson(string url, SimpleJSON.JSONNode jsn, httpProgressDelegate upd = null, float timeout = HTTPManager.cTimeoutDefault, Dictionary<string, string>headers = null) {
      return SendJson(requestType.POST, url, jsn, upd, timeout, headers);
    }

    [System.Obsolete("uploadJSON() is deprecated, please use PutJson() or PostJson() instead.")]
    public RequestInfo uploadJSON(string url, JSONNode jsn, httpProgressDelegate upd, float timeout) {
      return SendJson(requestType.POST, url, jsn, upd, timeout);
    }

    public RequestInfo downloadUrl(string url, httpProgressDelegate upd, float timeout) {
      return Get(url, upd, timeout);
    }

    #endregion convenience wrappers

    private IEnumerator startRequest(RequestInfo info, httpProgressDelegate hpd, float timeout) {
      info.timeoutMoment = Time.realtimeSinceStartup + timeout;

      // we can't return send() as the IEnumerator here because UnityWebRequest seems to provide no Timeout functionality.
      info.request.Send();

      // do one check before waiting 200ms in case it failed immediately
      checkStatus(ref info, hpd);
      while (!info.isDone) {
        yield return new WaitForSeconds(STATUS_CHECK_SECONDS);
        checkStatus(ref info, hpd);
      }
    }

    // Legacy
    [System.Obsolete("WWW is on the way out, UnityWebRequest or WW.HTTPManager instead.")]
    public static void Dispose(WWW www) {
      new Thread( () => { 
        Debug.Log("WWW dispose started in background thread."); 
        www.Dispose(); 
        Debug.Log("WWW dispose finished."); 
      }).Start(); 
    }
  


    private void checkStatus(ref RequestInfo info, httpProgressDelegate hpd) {
      if(info.isCanceled){
//        info.request.Dispose();
      }
      else{
        info.isRequestDone = info.request.isDone;
        if (info.isRequestDone) {
          info.responseCode = wwWWWUtil.getResponseCode(info.request, ref info.responseString);
          if (!info.success) {
            Debug.LogWarning("request failed: " + info.ToString());
          }
        }
        else if(Time.realtimeSinceStartup > info.timeoutMoment){
          info.responseCode   = REQUEST_TIMEOUT_RESPONSE;
          info.responseString = "REQUEST TIMEOUT";
          Debug.LogWarning("request timed out: " + info.ToString());
        }
        
        if(info.isTimeout){
//          info.request.Dispose();
        }
      }   

      if (hpd != null) {
        if (AllowAnonymousProgressDelegates || !piUnityUtils.isDelegateTargetNull(hpd)) {
          hpd(info);
        }
        else {
          Debug.LogWarning("progressDelegate has been destroyed. Cancelling request " + info.url);
          HTTPManager.Instance.cancelRequest(info);
        }
      }
    }
  }

  public class HTTPManagerTest: Singleton<HTTPManagerTest>{
    public const string urlExampleProgram = "https://alpha-share.makewonder.com/wonder/program/lock-mangy-transport";
    public const string urlUploadProgram  = "https://alpha-share.makewonder.com/wonder/program";
    public const string urlTimeout        = "http://httpbin.org/delay/5";
    public const string url404            = "http://httpbin.org/no_such_page";
    public const string urlPost           = "http://httpbin.org/post";
    public const string urlPut            = "http://httpbin.org/put";
    public const string urlPostTest       = "http://posttestserver.com/post.php";
    IEnumerator testDL(){
      int num = 10;
      HTTPManager.RequestInfo[] ris = new HTTPManager.RequestInfo[num];
      
      for(int i = 0; i < num; ++i){
        ris[i] = HTTPManager.Instance.downloadUrl (urlExampleProgram, onUploadProgress, 5.0f); 
      }
      
      HTTPManager.Instance.cancelRequest(ris[4]);
      
      yield return new WaitForSeconds(6.0f);
      
      bool testPasses = true;
      
      for (int n = 0; n < num; ++n) {
        if (n == 4) {
          bool pass = ris[n].isCanceled && ris[n].isDone && !ris[n].isTimeout ;
          if(!pass){
            Debug.LogError("Something wrong for canceled request: " + ris[n].ToString());
          }
          testPasses = testPasses && pass;
        }
        else {
          bool pass = !ris[n].isCanceled && ris[n].isDone && !ris[n].isTimeout && ris[n].success;
          if(!pass){
            Debug.LogError("Something wrong for request: " + ris[n].ToString());
          }
          testPasses = testPasses && pass;
        }
      }

      if(testPasses){
        Debug.Log("DL test passed.");
      }
    }
    
    private bool testTimeout_DidTimeout = false;
    
    IEnumerator testTimeout() {
      HTTPManager.RequestInfo ri =  HTTPManager.Instance.downloadUrl(urlTimeout, onProgress_Timeout, 0.01f);
      yield return new WaitForSeconds(0.25f);
      bool testPassed = testTimeout_DidTimeout && ri.isTimeout && ri.isDone;
      if(!testPassed){
        Debug.LogError("Something wrong for timeout test: " + ri.ToString());
      }
      else{
        Debug.Log("Timeout test passed.");
      }
    }
    
    private void onProgress_Timeout(HTTPManager.RequestInfo ri) {
      if (ri.isTimeout) {
        if(testTimeout_DidTimeout){
          Debug.LogError("Received two timeout response: " + ri.ToString());
        }
        testTimeout_DidTimeout = true;
      }
    }

    IEnumerator test404() {
      HTTPManager.RequestInfo ri = HTTPManager.Instance.downloadUrl(url404, null, 3f);
      yield return new WaitForSeconds(4f);
      bool testPassed = (ri.responseCode == 404);
      if(!testPassed){
        Debug.LogError("Something wrong for 404 test: " + ri.ToString());
      }
      else{
        Debug.Log("404 test passed.");
      }
    }

    IEnumerator testPost() {
      string body = "helloThereHowDoYouDo";
      Dictionary<string, string> headers = new Dictionary<string, string> {
        {HTTPManager.cHeaders.CONTENT_TYPE, "text/json"},
      };
      HTTPManager.RequestInfo ri = HTTPManager.Instance.Post(urlPost, body, null, HTTPManager.cTimeoutDefault, headers);
      yield return new WaitForSeconds(HTTPManager.cTimeoutDefault + 1f);

      SimpleJSON.JSONClass jsc = SimpleJSON.JSON.Parse(ri.DownloadedText).AsObject;

      bool testPassed = true;
      testPassed = testPassed && (ri.responseCode   == 200);
      testPassed = testPassed && (string.Equals(jsc["data"], body));

      if(!testPassed){
        Debug.LogError("Something wrong for POST test: " + ri.ToString() + "  " + ri.DownloadedText);
      }
      else{
        Debug.Log("POST test passed.");
      }
    }

    void onProgress(HTTPManager.RequestInfo ri) {
      Debug.Log("Got progress. this: " + this.ToString() + "  url: " + ri.url);
    }

    IEnumerator testPut() {
      string body = "helloThereHowDoYouDo";
      HTTPManager.RequestInfo ri = HTTPManager.Instance.Put(urlPut, body, null, HTTPManager.cTimeoutDefault);
      yield return new WaitForSeconds(HTTPManager.cTimeoutDefault + 1f);

      SimpleJSON.JSONClass jsc = null;

      if (ri.responseCode == 200) {
        jsc = SimpleJSON.JSON.Parse(ri.DownloadedText).AsObject;
      }

      bool testPassed = true;
      testPassed = testPassed && (ri.responseCode   == 200);
      testPassed = testPassed && (string.Compare(jsc["data"], body) == 0);

      if(!testPassed){
        Debug.LogError("Something wrong for PUT test: " + ri.ToString() + ri.DownloadedText);
      }
      else{
        Debug.Log("PUT test passed.");
      }
    }

    IEnumerator testUploadProgram() {
      string body = "{\"myKey\":\"myValue\"}";
      Dictionary<string, string> headers = new Dictionary<string, string> {
        {HTTPManager.cHeaders.CONTENT_TYPE, "text/json"},
      };
      HTTPManager.RequestInfo ri = HTTPManager.Instance.Post(urlUploadProgram, body, null, HTTPManager.cTimeoutDefault, headers);
      yield return new WaitForSeconds(HTTPManager.cTimeoutDefault + 1f);

      bool testPassed = true;
      testPassed = testPassed && (ri.responseCode == 200);

      if(!testPassed){
        Debug.LogError("Something wrong for UploadProgram test: " + ri.ToString() + "  " + ri.DownloadedText);
      }
      else{
        Debug.Log("UploadProgram test passed. result = " + ri.DownloadedText);
      }
    }

    class DestroyTester : MonoBehaviour {
      public void onHTTPProgress(HTTPManager.RequestInfo ri) {
        if (this == null) {
          Debug.LogError("on progress. this = " + this.ToString() + " ri = " + ri.ToString());
        }
      }
    }

    IEnumerator testDestroyListener() {
      GameObject go = new GameObject();
      DestroyTester dt = go.AddComponent<DestroyTester>();

      HTTPManager.Instance.Get(HTTPManagerTest.urlTimeout, dt.onHTTPProgress, 5f);

      yield return new WaitForSeconds(1f);

      GameObject.Destroy(go);

      yield return new WaitForSeconds(2f + 1f);
    }

    static void staticOnProgress(HTTPManager.RequestInfo ri) {
    }

    IEnumerator testStaticListener() {
      HTTPManager.RequestInfo ri = HTTPManager.Instance.Get(HTTPManagerTest.urlTimeout, staticOnProgress, 3f);
      if (ri != null) {
        Debug.LogError("error: request info should be null when given a static onProgress delegate");
      }
      yield break;
    }

    public void test() {
      StartCoroutine(testDL             ());
      StartCoroutine(testTimeout        ());
      StartCoroutine(test404            ());
      StartCoroutine(testPost           ());
      StartCoroutine(testPut            ());
      StartCoroutine(testUploadProgram  ());
      StartCoroutine(testStaticListener());
      StartCoroutine(testDestroyListener());
    }

    void onUploadProgress(HTTPManager.RequestInfo ri) {
      //Debug.LogError(ri.ToString());
    }
  }

}
