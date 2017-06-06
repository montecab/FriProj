using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using WW.UGUI;

namespace Turing{
  public class trCoordinateAxis : MonoBehaviour {

  	public Dictionary<float, string> NormValueToLabelTable = new Dictionary<float, string>();

    public RectTransform CoordinatePanel;
    public GameObject TextPrefab;
    public Transform TextParent;

    public Direction Dir = Direction.Horizontal;

    public void UpdateView(){
      foreach(Transform child in TextParent.transform){
        Destroy(child.gameObject);
      }

      foreach(float key in NormValueToLabelTable.Keys){
        if(Dir == Direction.Horizontal){
          Vector3 pos = CoordinatePanel.XYRatioToWorldPos(new Vector2(key, 0));
          pos.y = TextParent.transform.position.y;
          pos.z = TextParent.transform.position.z;

          GameObject newText = Instantiate(TextPrefab, pos, TextPrefab.transform.rotation)as GameObject;
          newText.transform.SetParent(TextParent);
          newText.GetComponent<Text>().text = NormValueToLabelTable[key];
        }
        else{
          Vector3 pos = CoordinatePanel.XYRatioToWorldPos(new Vector2(0, key));
          pos.x = TextParent.transform.position.x;
          pos.z = TextParent.transform.position.z;
          
          GameObject newText = Instantiate(TextPrefab, pos, TextPrefab.transform.rotation)as GameObject;
          newText.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
          newText.transform.SetParent(TextParent);
          newText.GetComponent<Text>().text = NormValueToLabelTable[key];
          newText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        }

      }
    }
  }
}
