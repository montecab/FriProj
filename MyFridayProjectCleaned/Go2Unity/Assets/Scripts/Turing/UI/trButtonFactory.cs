using UnityEngine;
using System.Collections;
using System;

namespace Turing{
  public class trButtonFactory :MonoBehaviour{
    static GameObject prefab;

    public static GameObject CreateRoundButton(){
      if(prefab == null){
        prefab = Resources.Load("TuringProto/ButtonPrefab") as GameObject;
      }
      GameObject newButton = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation) as GameObject;
      return newButton;
    }
  }
}
