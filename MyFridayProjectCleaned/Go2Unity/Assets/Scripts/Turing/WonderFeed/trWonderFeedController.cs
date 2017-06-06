using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using WW.SimpleJSON;
using WW;


namespace Turing{
  public class trWonderFeedController : MonoBehaviour {
    public ScrollRect ScrollCtrl;
    public GameObject FeedItemPrefab;
    public Transform FeedItemListObj;

    public TextAsset FakeFeedJson;

    // private Dictionary<CommunityCategory, trCommunityCategoryButtonController> CategoryTable = new Dictionary<CommunityCategory, trCommunityCategoryButtonController>();
    // private Dictionary<trPublishedItem, trCommunityItemController> itemTable = new Dictionary<trPublishedItem, trCommunityItemController>();

    // private Dictionary<HTTPManager.RequestInfo, trCommunityItemController> requestTable = new Dictionary<HTTPManager.RequestInfo, trCommunityItemController>();

     void Start () {
       InitView();
     }

    void InitView(){
      onFinishDownloading();
    }

    void onFinishDownloading(){
      string js = FakeFeedJson.text;
      JSONNode jsn = JSON.Parse(js);
      List<FeedItem> itemList = new List<FeedItem>();
      foreach(JSONClass jsc in jsn["items"].AsArray){
        FeedItem item = new FeedItem();
        item.FromJson(jsc);
        itemList.Add(item);
      }

      for(int i = 0; i< itemList.Count; ++i){
        GameObject newItem = Instantiate(FeedItemPrefab) as GameObject;
        newItem.transform.SetParent(FeedItemListObj, false);
        trFeedItemController ctrl = newItem.GetComponent<trFeedItemController>();
        ctrl.CurItem = itemList[i];
        ctrl.BtnListener = onFeedItemClicked;
      }


    }

    void onFeedItemClicked(FeedItem item){
      Application.OpenURL(item.Link);
    }

  }
}

