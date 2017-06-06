using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Turing{
  public class trActuatorPanelControllerBase : MonoBehaviour {

    public List<trMapperPanelController> MapperCtrls = new List<trMapperPanelController>();

    public bool IsActivate{
      get{
        for(int i = 0; i< MapperCtrls.Count; ++i){
          if(MapperCtrls[i].MapData != null){
            if(MapperCtrls[i].MapData.Active){
              return true;
            }
          }
        }
        return false;
      }
    }

    public bool IsSimpleMode{
      get{
        for(int i = 0; i< MapperCtrls.Count; ++i){
          if(MapperCtrls[i].MapData != null){
            if(MapperCtrls[i].MapData.SimpleMode){
              return true;
            }
          }
        }
        return false;
      }
    }

    public void SetMode(bool isSimple){
      for(int i = 0; i< MapperCtrls.Count ; ++i){
        MapperCtrls[i].SetMode(isSimple);
      }
    }

    public void ResetActuators(){
      for(int i = 0; i< MapperCtrls.Count; ++i){
        MapperCtrls[i].ActuatorConfigCtrl.Reset();
      }
    }


    public void Reset(){
      for(int i = 0; i< MapperCtrls.Count; ++i){
        MapperCtrls[i].Reset();
      }
    }

  }
}
