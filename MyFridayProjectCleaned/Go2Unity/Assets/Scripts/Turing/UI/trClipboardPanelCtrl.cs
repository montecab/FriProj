using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Turing;
using WW.SimpleJSON;

public class trClipboardPanelCtrl : MonoBehaviour {

  public Button            btnToClipboard;
  public Button            btnFromClipboard;
  public trProtoController protoCtrl;
  
  private trAppSaveInfo trASI;
  
	// Use this for initialization
	void Start () {
    trASI = trDataManager.Instance.AppSaveInfo;
  
    btnToClipboard  .onClick.AddListener(onClickToClipboard);
    btnFromClipboard.onClick.AddListener(onClickFromClipboard);
	}

  void onClickToClipboard() {
    trClipboardManager.ClipboardValue = trDataManager.Instance.GetCurProgram().ToJson().ToString("");
  }
  
  void onClickFromClipboard() {
    try {
      JSONNode jsn = JSON.Parse(trClipboardManager.ClipboardValue);
      trProgram newProgram = trFactory.FromJson<trProgram>(jsn);
      
      trProgram curProgram = trDataManager.Instance.GetCurProgram();
      
      if (newProgram.RobotType != curProgram.RobotType) {
        protoCtrl.ShowConnectToRobotDialog();
        return;
      }
      
      newProgram.UserFacingName = curProgram.UserFacingName;
      newProgram.UUID           = curProgram.UUID;
      
      trASI.RemoveProgram(curProgram, null);
      trASI.AddProgram   (newProgram);
      trASI.CurProgram = newProgram;
      trASI.Save();
      protoCtrl.LoadProgram(newProgram);
    }
    catch (System.Exception e) {
      WWLog.logError("could not load from clipboard: " + e.ToString());
    }
    
    
  }
  
  
}
