using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WW.SimpleJSON;

/*
from warner:

Save Program:
POST https://alpha-share.makewonder.com/wonder/program
  headers: "Content-Type: application/json"
  body: Any Json Object
  response: { 
    "name": "<Generated Name>",
    "url": "<S3 Downlaod Path>"
  }

Fetch by Program Name:
GET https://alpha-share.makewonder.com/wonder/program/[Program Name]
  response: { 
    "name": "<Generated Name>",
    "url": "<S3 Downlaod Path>"
  }
  
Fetch Program Json Object:
GET <S3 Download Path>
  response: <Program Json Object>
*/

// TODO: all this should be replaced w/ HTTPManager.

namespace WW {
  public class SharingManager : Singleton<SharingManager> {

    public const string      TOK_NAME = "name";
    public const string      TOK_URL  = "url";


    public delegate void progressDelegate(float percentComplete, bool success, string result);

    public void uploadJSON(string URLBase, JSONNode jsn, progressDelegate hpd) {

      HTTPManager.httpProgressDelegate pd = delegate(HTTPManager.RequestInfo ri) {
        if (!ri.isDone) {
          return;
        }

        if (ri.success) {
          hpd(1.0f, ri.success, ri.DownloadedText);
        }
        else {
          hpd(1.0f, false, ri.ResponseCodeAndString); // todo: friendly error messages.
        }
      };

      string url = URLBase + "/program";
      HTTPManager.Instance.uploadJSON(url, jsn, pd, HTTPManager.cTimeoutDefault);
    }

    public void downloadCode(string URLBase, string code, progressDelegate pd) {

      HTTPManager.httpProgressDelegate myPD1 = delegate(HTTPManager.RequestInfo ri) {
        if (!ri.isDone) {
          return;
        }

        if (!ri.success) {
          pd(1.0f, false, ri.ResponseCodeAndString);
          return;
        }

        WW.SimpleJSON.JSONClass jsc = null;

        try {
          jsc = WW.SimpleJSON.JSON.Parse(ri.DownloadedText).AsObject;
        }
        catch (System.Exception e) {
          pd(1.0f, false, "could not parse response (1) - " + e.ToString());
          return;
        }
    
        if (jsc == null) {
          pd(1.0f, false, "could not parse response (2)");
          return;
        }
    
        if (!jsc.ContainsKey(TOK_NAME)) {
          pd(1.0f, false, "malformed json - no name");
          return;
        }
    
        if (!jsc.ContainsKey(TOK_URL)) {
          pd(1.0f, false, "malformed json - no url");
          return;
        }
    
        string code2 = normalizeCode(jsc[TOK_NAME]);

        pd(0.5f, false, "fetching " + code2);

        HTTPManager.httpProgressDelegate myPD2 = delegate(HTTPManager.RequestInfo ri2) {
          if (!ri2.isDone) {
            return;
          }

          if (!ri2.success) {
            pd(1.0f, false, ri2.ResponseCodeAndString);
            return;
          }

          pd(1.0f, true, ri2.DownloadedText);
        };
    
        HTTPManager.Instance.downloadUrl(jsc[TOK_URL], myPD2, HTTPManager.cTimeoutDefault);
      };

      string url = URLBase + "/program/" + code;
      HTTPManager.Instance.downloadUrl(url, myPD1, HTTPManager.cTimeoutDefault);
    }

    public static string normalizeCode(string src) {
      string ret = src;
      ret = ret.Replace('-', ' ');
      ret = ret.Trim();
      return ret;
    }
    

  }
}

