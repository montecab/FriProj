using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Turing{
  public class trAuthoringPuzzleConfigPanelController : MonoBehaviour {
    
    public InputField NameInput;
    public InputField DescriptionInput;
    public Toggle LoadFromFirstHintToggle;

    public Toggle ShowIntroductionToggle;
    public InputField IntroductionInput;

    public Toggle CenterProgramOnStartToggle;
    
    private trPuzzle curPuzzle;
    public trPuzzle CurPuzzle{
      set{
        curPuzzle = value;
        setView();
      }
      get{
        return curPuzzle;
      }
    }
   
    public trAuthoringHintListPanelController HintListCtrl;
    public trAuthoringMissionConfigPanelController ParentCtrl;

    public Button IconButton;
    public trAuthoringMissionIconPanelController IconPanelCtrl;

    public void SetActive(trPuzzle puzzle){
      this.gameObject.SetActive(true);
      CurPuzzle = puzzle;
      HintListCtrl.CurPuzzle = puzzle;
    }

    public void SetIconView(){
      IconButton.GetComponent<trButtonBase>().Img.sprite = trIconFactory.GetMissionIcon(curPuzzle.IntroductionIconName);
    }

    void onIconButtonClicked(){
      IconPanelCtrl.Open(CurPuzzle.IntroductionIconName);
      IconPanelCtrl.IconChangeListener = onIconChanged;
    }

    void onIconChanged(string sprite){
      trDataManager.Instance.AuthoringMissionInfo.CurPuzzle.IntroductionIconName = sprite;
      SetIconView();
      trDataManager.Instance.AuthoringMissionInfo.Save();
    }

    
    void setView(){
      NameInput.text = curPuzzle.UserFacingName;
      DescriptionInput.text = curPuzzle.Description;
      LoadFromFirstHintToggle.isOn = curPuzzle.IsLoadStartProgram;
      ShowIntroductionToggle.isOn = curPuzzle.IsShowIntroduction;
      CenterProgramOnStartToggle.isOn = curPuzzle.IsCenterProgramOnStart;
      IntroductionInput.text = curPuzzle.IntroductionText;
      SetIconView();
    }
    
    void Start(){
      NameInput.onEndEdit.AddListener(onNameChange);
      DescriptionInput.onEndEdit.AddListener(onDescriptionChange);
      LoadFromFirstHintToggle.onValueChanged.AddListener(onToggleChange);
      ShowIntroductionToggle.onValueChanged.AddListener(onShowIntroductionToggleChange);
      CenterProgramOnStartToggle.onValueChanged.AddListener(onCenterProgramToggleChange);
      IntroductionInput.onEndEdit.AddListener(onIntroductionChange);
      IconButton.onClick.AddListener(onIconButtonClicked);
    }

    void onCenterProgramToggleChange(bool isOn){
      if(curPuzzle.IsCenterProgramOnStart != isOn){
        curPuzzle.IsCenterProgramOnStart = isOn;
        trDataManager.Instance.AuthoringMissionInfo.Save();
      }
    }

    void onShowIntroductionToggleChange(bool isOn){
      if(curPuzzle.IsShowIntroduction != isOn){
        curPuzzle.IsShowIntroduction = isOn;
        trDataManager.Instance.AuthoringMissionInfo.Save();
        IntroductionInput.gameObject.SetActive(isOn);
        IconButton.gameObject.SetActive(isOn);
      }
    }

    void onIntroductionChange(string s){
      if(curPuzzle.IntroductionText != s){
        curPuzzle.IntroductionText = s;
        trDataManager.Instance.AuthoringMissionInfo.Save();
      }     
    }

    void onToggleChange(bool isOn){
      if(curPuzzle.IsLoadStartProgram != isOn){
        curPuzzle.IsLoadStartProgram = isOn;
        trDataManager.Instance.AuthoringMissionInfo.Save();
      }
    }
    
    void onNameChange(string s){
      if(curPuzzle.UserFacingName != s){
        curPuzzle.UserFacingName = s;
        trDataManager.Instance.AuthoringMissionInfo.Save();
        ParentCtrl.PuzzleListCtrl.SetListView();
      }
    }
    
    void onDescriptionChange(string s){
      if(curPuzzle.Description != s){
        curPuzzle.Description = s;
        trDataManager.Instance.AuthoringMissionInfo.Save();
      }
    }
  }
  
  
}
