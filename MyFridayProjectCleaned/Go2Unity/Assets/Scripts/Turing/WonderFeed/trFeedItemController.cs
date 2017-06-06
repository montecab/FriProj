using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System;

public class trFeedItemController : MonoBehaviour {
  public TextMeshProUGUI TextDescriptionLabel;
  public TextMeshProUGUI ThumbnailDescriptionLabel;
  public Image Img;
  public Button Btn;

  public Sprite[] Sprites;

  public GameObject ThumbnailPanel;
  public GameObject TextPanel;

  public Action<FeedItem> BtnListener;
  private FeedItem curItem;
  public FeedItem CurItem{
    get{
      return curItem;
    }
    set{
      if(curItem == value){
        return;
      }
      curItem = value;
      setView();
    }
  }

  void Start(){
    Btn.onClick.AddListener(onBtnClick);
  }

  void onBtnClick(){
    if(BtnListener != null){
      BtnListener(curItem);
    }
  }

  void setView(){
    if(curItem == null){
      return;
    }
    ThumbnailPanel.SetActive(curItem.Type == FeedType.THUMBNAIL);
    TextPanel.SetActive(curItem.Type == FeedType.TEXT);
    //just for testing
    if(curItem.ImageUrl == "thumb1"){
      Img.sprite = Sprites[0];
    }
    else if(curItem.ImageUrl == "thumb2"){
      Img.sprite = Sprites[1];
    }
    else if(curItem.ImageUrl == "thumb3"){
      Img.sprite = Sprites[2];
    }

    if(FeedType.TEXT == curItem.Type){
      TextDescriptionLabel.text = curItem.Description;
    }
    else if(FeedType.THUMBNAIL == curItem.Type){
      ThumbnailDescriptionLabel.text = curItem.Description;
    }
  }
}
