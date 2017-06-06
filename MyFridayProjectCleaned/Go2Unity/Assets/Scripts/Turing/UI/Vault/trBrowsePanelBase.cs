using UnityEngine;
using UnityEngine.UI;

namespace Turing{
  public class trBrowsePanelBase : MonoBehaviour{

    protected ScrollRect _scrollCtrl;
    protected Toggle _robotToggle;
    protected GameObject _dashImg;
    protected GameObject _dotImg;
    protected trBrowseTabBase _curTab;

    protected wwToggleableButton _robotToggleLeftPanelButton;

    protected piRobotType CurRobotType{
      get{
        return _robotToggle.isOn ? piRobotType.DASH : piRobotType.DOT;
      }
      set{
        _robotToggle.isOn = (value == piRobotType.DASH); 
      }
    }

    // Use this for initialization
    void Start(){
      initView();
      setRobotType();
    }

    void OnDestory(){
      _robotToggle.onValueChanged.RemoveListener(onRobotTypeChange); 
      _robotToggleLeftPanelButton.OnValueChanged -= onRobotToggleLeftChange; 
    }

    protected virtual void initView(){
      _robotToggle.onValueChanged.AddListener(onRobotTypeChange); 
      _robotToggleLeftPanelButton.OnValueChanged += onRobotToggleLeftChange; 
      _robotToggleLeftPanelButton.OverrideValue(_robotToggle.isOn, false);
    }

    protected virtual void setRobotType(){
      // default to dash as robot type for now
      if(piConnectionManager.Instance.LastConnectedRobot != null){
        CurRobotType = piConnectionManager.Instance.LastConnectedRobot.robotType;
      } else{
        CurRobotType = piRobotType.DASH;  
      }
    }

    void onRobotToggleLeftChange(bool isOn){
      _robotToggle.isOn = isOn;
    }

    protected virtual void onRobotTypeChange(bool isOn){
      _dashImg.SetActive(isOn);
      _dotImg.SetActive(!isOn);
      if(_curTab != null){
        onToggleTab(true, _curTab);
      }
      // also update left panel button
      _robotToggleLeftPanelButton.OverrideValue(isOn, false);
    }

    protected virtual void onToggleTab(bool isOn, trBrowseTabBase btn){
      if(!btn.ListTable.ContainsKey(CurRobotType)){
        createList(btn, CurRobotType);
      } else{
        btn.SetList(CurRobotType);
        _scrollCtrl.content = btn.ListTable[CurRobotType];
      }
      if(isOn){
        _curTab = btn;
      }
    }

    protected virtual void createList(trBrowseTabBase btn, piRobotType type){
      
    }
  }
}

