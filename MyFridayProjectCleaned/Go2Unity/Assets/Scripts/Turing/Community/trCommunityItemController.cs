using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System;

namespace Turing{
  public class trCommunityItemController : MonoBehaviour {
    public TextMeshProUGUI NameLabel;
    public TextMeshProUGUI DescriptionLabel;
    public Image Img;
    public Sprite FailureSprite;
    public Button Btn;
    public TextMeshProUGUI PopularityLabel;
    public GameObject TriggerPrefab;
    public Transform TriggerParent;
    public GameObject ImageSpinner;
    public CommunityCategory Category;
    public piRobotType RobotType = piRobotType.DASH;
    public bool ImageDownloaded = false;


    public Action<trCommunityItemController> BtnListener;
    private trPublishedItem curItem;
    public trPublishedItem CurItem{
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
        BtnListener(this);
      }
    }

    public void SetFailureSprite(){
      ImageSpinner.SetActive(false);
      Img.color = Color.white;
      Img.sprite = FailureSprite;
    }

    public void SetSprite(Sprite sprite){
      ImageSpinner.SetActive(false);
      Img.color = Color.white;
      Img.sprite = sprite;
    }

    void setView(){
      if(curItem == null){
        return;
      }
      DescriptionLabel.text = curItem.Description;
      NameLabel.text = curItem.Name;
      PopularityLabel.text = curItem.Popularity.ToString();
      int count = 0;
      for(int i = 0; i< curItem.Elements.Count && count < 4; ++i){

        if(curItem.Elements[i].Type == ElementType.CUE){
          GameObject newT = Instantiate(TriggerPrefab) as GameObject;
          newT.transform.SetParent(TriggerParent, false);
          newT.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = trIconFactory.GetIcon(curItem.Elements[i].TriggerType);
          count ++;
          newT.transform.SetAsFirstSibling();
        }

//        else if (curItem.Elements[i].Type == ElementType.BEHAVIOR){
//          newT.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = trIconFactory.GetIcon(curItem.Elements[i].BehaviorType);
//        }
       
      }
    }
  }
}

