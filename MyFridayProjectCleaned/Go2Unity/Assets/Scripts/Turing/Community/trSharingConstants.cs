using UnityEngine;
using System.Collections;

public class trSharingConstants {

  private static string URL_TYPE = "https://";
  #if USE_ALPHA
  private static string SERVER_ENV = "alpha-"; // alpha
  #else
  private static string SERVER_ENV = ""; // production
  #endif
  private static string SERVICE_FILE = "file";
  private static string SERVICE_PUBLISHING = "publishing";
  private static string SERVER_BASE_URL = "makewonder.com/";

  private static string REST_FILE_WITH_ID = "file/id";
  private static string REST_FILE_WITH_TOKEN = "file/token";
  private static string REST_COMMUNITY_LIST_PICK = "wonder/pick";
  private static string REST_COMMUNITY_LIST_ALL = "wonder/publish";
  private static string REST_COMMUNITY_LIST_POPULAR = "wonder/popular";
  private static string REST_INCREMENT_DOWNLOAD_COUNT = "downloaded";


  public static int MIN_SHARING_TOKEN_LENGTH = 4;

  public static float DOWNLOAD_TIMEOUT = 21.0f;


  private static string getFullyQualifiedServerUrl(string service, string RESTAction) {
    #if USE_ALPHA
    if (wwDoOncePerTypeVal<string>.doIt("warn about using alpha servers")) {
      piConnectionManager.Instance.showSystemDialog("using alpha servers");
    }
    #endif
    return URL_TYPE + SERVER_ENV + service + "." + SERVER_BASE_URL + RESTAction; // default to PROD for now
  }


  public static string GetListUrl(CommunityCategory cat, piRobotType type, string lang){
    string typeAppend = type == piRobotType.DASH? "?robotType=1001" : "?robotType=1002";
    string langAppend = "&lang=" + lang;
    switch(cat){
    case CommunityCategory.All:
      return trSharingConstants.getFullyQualifiedServerUrl(trSharingConstants.SERVICE_PUBLISHING, trSharingConstants.REST_COMMUNITY_LIST_ALL) + typeAppend + langAppend;
    case CommunityCategory.Picks:
      return trSharingConstants.getFullyQualifiedServerUrl(trSharingConstants.SERVICE_PUBLISHING, trSharingConstants.REST_COMMUNITY_LIST_PICK) + typeAppend + langAppend;
    case CommunityCategory.Popular:
      return trSharingConstants.getFullyQualifiedServerUrl(trSharingConstants.SERVICE_PUBLISHING, trSharingConstants.REST_COMMUNITY_LIST_POPULAR) + typeAppend + langAppend;
    }
    return "";
  }

  public static string GetPublishedItemRESTUrl(string fileId = null){
    string baseUrl = trSharingConstants.getFullyQualifiedServerUrl(trSharingConstants.SERVICE_FILE, trSharingConstants.REST_FILE_WITH_ID);
    if (fileId != null) {
      baseUrl = baseUrl + "/" + fileId;
    }
    return baseUrl;
  }

  public static string GetSharedItemRESTUrl(string token = null){
    string baseUrl = trSharingConstants.getFullyQualifiedServerUrl(trSharingConstants.SERVICE_FILE, trSharingConstants.REST_FILE_WITH_TOKEN);
    if (token != null) {
      baseUrl = baseUrl + "/" + token;
    } 
    return baseUrl; 
  }

  public static string GetItemDownloadCountRESTUrl(trPublishedItem item = null){
    string baseUrl = trSharingConstants.getFullyQualifiedServerUrl(trSharingConstants.SERVICE_PUBLISHING, trSharingConstants.REST_INCREMENT_DOWNLOAD_COUNT);
    if (item != null) {
      baseUrl = baseUrl + "/" + item.ID;
    }
    return baseUrl;
  }
}
