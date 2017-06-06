using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Turing{
  public class trAuthoringMissionConfigPanelController : MonoBehaviour {


  	public InputField NameInput;
    public InputField DescriptionInput;

    private trMission curMission;
    public trMission CurMission{
      set{
        curMission = value;
        setView();
      }
      get{
        return curMission;
      }
    }

    public GridLayoutGroup StepParent;
    public GameObject StepButtonPrefab;

    public trAuthoringMissionIconPanelController IconPanelCtrl;
    public trAuthoringMissionListPanelController ParentCtrl;
    public trAuthoringPuzzleListController PuzzleListCtrl;
    public trVideoSelectBtnController StartVideoButton;
    public trVideoSelectBtnController EndVideoButton;

    public Toggle IsTutorialToggle;

    public Button IconButton;

    public Toggle DashToggle;
    public Toggle DotToggle;

    //intro outro experiment
    public Toggle IsIntroOutroModeToggle;
    public trButtonBase IntroSpriteButton;
    public trButtonBase OutroSpriteButton;
    public InputField OutroDescriptionInput;
    public GameObject IntroOutroPanel;

    public void SetActive(){
      CurMission = trDataManager.Instance.MissionMng.AuthoringMissionInfo.CurMission ;
      this.gameObject.SetActive(true);
    }

    void OnEnable(){
      if(trDataManager.Instance.MissionMng.AuthoringMissionInfo.CurMission != CurMission){
        trDataManager.Instance.MissionMng.AuthoringMissionInfo.CurMission = CurMission;
      }
    }

    void setView(){
      PuzzleListCtrl.SetUp(curMission);        
      NameInput.text = curMission.UserFacingName;
      DescriptionInput.text = curMission.IntroDescription;
      IsTutorialToggle.isOn = curMission.IsTutorialMission;
      SetIconView();
      if(curMission.Type == trMissionType.DASH){
        DashToggle.isOn = true;
        DotToggle.isOn = false;
      }
      else if(curMission.Type == trMissionType.DOT){
        DashToggle.isOn = false;
        DotToggle.isOn = true;
      }

      IsIntroOutroModeToggle.isOn = curMission.IsIntroOutroMode;
      IntroOutroPanel.gameObject.SetActive(curMission.IsIntroOutroMode);
      if(curMission.IsIntroOutroMode){
        IntroSpriteButton.Img.sprite = trIconFactory.GetMissionIcon(curMission.IntroSprite);
        OutroSpriteButton.Img.sprite = trIconFactory.GetMissionIcon(curMission.OutroSprite);
        OutroDescriptionInput.text = curMission.OutroDescription;
      }

      StartVideoButton.VideoName.text = curMission.StartVideo;
      EndVideoButton.VideoName.text = curMission.EndVideo;
    }

    public void SetIconView(){
      IconButton.GetComponent<trButtonBase>().Img.sprite = trIconFactory.GetMissionIcon(curMission.SpriteName);
    }

    void onIconButtonClicked(){
      IconPanelCtrl.IconChangeListener = onIconChanged;
      IconPanelCtrl.Open(CurMission.SpriteName);
    }

    void onIconChanged(string sprite){
      trDataManager.Instance.AuthoringMissionInfo.CurMission.SpriteName = sprite;
      SetIconView();
      trDataManager.Instance.AuthoringMissionInfo.Save();
    }

    void Start(){
      NameInput.onEndEdit.AddListener(onNameChange);
      DescriptionInput.onEndEdit.AddListener(onDescriptionChange);
      IconButton.onClick.AddListener(onIconButtonClicked);
      IsTutorialToggle.onValueChanged.AddListener(onTutorialToggleChange);
      DashToggle.onValueChanged.AddListener(onTypeToggleChange);
      DotToggle.onValueChanged.AddListener(onTypeToggleChange);
      StartVideoButton.OnSave = onStartVideoChange;
      EndVideoButton.OnSave = onEndVideoChange;
      IsIntroOutroModeToggle.onValueChanged.AddListener(onIntroOutroModeChange);
      IntroSpriteButton.Btn.onClick.AddListener(onIntroSpriteClicked);
      OutroSpriteButton.Btn.onClick.AddListener(onOutroSpriteClicked);
      OutroDescriptionInput.onEndEdit.AddListener(onOutroDescriptionChange);
    }

    void OnDestroy(){
      NameInput.onEndEdit.RemoveListener(onNameChange);
      DescriptionInput.onEndEdit.RemoveListener(onDescriptionChange);
      IconButton.onClick.RemoveListener(onIconButtonClicked);
      IsTutorialToggle.onValueChanged.RemoveListener(onTutorialToggleChange);
      DashToggle.onValueChanged.RemoveListener(onTypeToggleChange);
      DotToggle.onValueChanged.RemoveListener(onTypeToggleChange);
      StartVideoButton.OnSave -= onStartVideoChange;
      EndVideoButton.OnSave -= onEndVideoChange;
      IsIntroOutroModeToggle.onValueChanged.RemoveListener(onIntroOutroModeChange);
      IntroSpriteButton.Btn.onClick.RemoveListener(onIntroSpriteClicked);
      OutroSpriteButton.Btn.onClick.RemoveListener(onOutroSpriteClicked);
      OutroDescriptionInput.onEndEdit.RemoveListener(onOutroDescriptionChange);
    }

    void onOutroDescriptionChange(string s){
      CurMission.OutroDescription = s;
      trDataManager.Instance.MissionMng.AuthoringMissionInfo.SaveCurMission();
    }

    void  onIntroSpriteClicked(){
      IconPanelCtrl.IconChangeListener = onIntroSpriteChange;
      IconPanelCtrl.Open(CurMission.IntroSprite);
    }

    void  onOutroSpriteClicked(){
      IconPanelCtrl.IconChangeListener = onOutroSpriteChange;
      IconPanelCtrl.Open(CurMission.OutroSprite);
    }

    void onIntroSpriteChange(string sprite){
      IntroSpriteButton.Img.sprite = trIconFactory.GetMissionIcon(sprite);
      CurMission.IntroSprite = sprite;
      trDataManager.Instance.MissionMng.AuthoringMissionInfo.SaveCurMission();
    }

    void onOutroSpriteChange(string sprite){
      OutroSpriteButton.Img.sprite = trIconFactory.GetMissionIcon(sprite);
      CurMission.OutroSprite = sprite;
      trDataManager.Instance.MissionMng.AuthoringMissionInfo.SaveCurMission();
    }

    void onIntroOutroModeChange(bool isOn){
      if(isOn != CurMission.IsIntroOutroMode){
        CurMission.IsIntroOutroMode = isOn;
        IntroOutroPanel.gameObject.SetActive(isOn);
        trDataManager.Instance.MissionMng.AuthoringMissionInfo.SaveCurMission();
      }
    }

    void onStartVideoChange(trVideoSelectBtnController ctrl){
      trDataManager.Instance.AuthoringMissionInfo.CurMission.StartVideo = ctrl.VideoName.text;
      trDataManager.Instance.MissionMng.AuthoringMissionInfo.SaveCurMission();
    }

    void onEndVideoChange(trVideoSelectBtnController ctrl){
      trDataManager.Instance.AuthoringMissionInfo.CurMission.EndVideo = ctrl.VideoName.text;
      trDataManager.Instance.MissionMng.AuthoringMissionInfo.SaveCurMission();
    }

    void onTypeToggleChange(bool ison){
      bool isChanged = false;
      if(DashToggle.isOn){
        if(curMission.Type != trMissionType.DASH){
          curMission.Type = trMissionType.DASH;
          isChanged = true;
        }
      }
      else if(DotToggle.isOn){
        if(curMission.Type != trMissionType.DOT){
          curMission.Type = trMissionType.DOT;
          isChanged = true;
        }
      }
      if(isChanged){
        trDataManager.Instance.AuthoringMissionInfo.Save();
      }

    }

    void onTutorialToggleChange(bool isOn){
      if(curMission.IsTutorialMission != isOn){
        curMission.IsTutorialMission = isOn;
        trDataManager.Instance.AuthoringMissionInfo.Save();
      }
     
    }

    void onNameChange(string s){
      if(curMission.UserFacingName != s){
        curMission.UserFacingName = s;
        trDataManager.Instance.AuthoringMissionInfo.Save();
        ParentCtrl.ResetStateMachineView();
      }
    }

    void onDescriptionChange(string s){
      if(curMission.IntroDescription != s){
        curMission.IntroDescription = s;
        trDataManager.Instance.AuthoringMissionInfo.Save();
      }

    }
  }


}
