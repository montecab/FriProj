using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Turing{
  public class trAuthoringHintListPanelController: MonoBehaviour {
    
    public GameObject ButtonPrefab;
    public GridLayoutGroup ButtonParent;
    public Button AddNewButton;
    public Transform Indicator;

    public GameObject CopyWhichText;
   
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

    private Dictionary<trAuthoringMissionButtonBase, trHint> buttonToHintTable = new Dictionary<trAuthoringMissionButtonBase,trHint>();

    public trAuthoringMissionPanelController AuthoringMissionMng;

    private bool isAddMode = false;
    
    void Start(){
      AddNewButton.onClick.AddListener(()=>onAddNewButtonClicked());
    }
    
    void onAddNewButtonClicked(){
      if(curPuzzle.Hints.Count == 0){
        AddHint();
      }else{
        isAddMode = !isAddMode;
        Color color = isAddMode? Color.yellow : Color.cyan;
        foreach(trAuthoringMissionButtonBase button in buttonToHintTable.Keys){       
          button.Img.color = color;
        }
        CopyWhichText.gameObject.SetActive(isAddMode);
      }     
    }
    
    public void Reorder(){
      setNewBtnLast();
    }
    
    void setNewBtnLast(){
      AddNewButton.transform.SetAsLastSibling();
    }
    
    void setView(){
      reset();
      for(int i = 0; i< curPuzzle.Hints.Count; ++i){
        AddButton(i + 1, curPuzzle.Hints[i]);
      }
    }
    
    void reset(){
      Indicator.gameObject.SetActive(false);
      foreach(trAuthoringMissionButtonBase button in buttonToHintTable.Keys){
        Destroy(button.gameObject);
      }
      buttonToHintTable.Clear();
    }
    
    void deleteButton(trAuthoringMissionButtonBase button){
      curPuzzle.Hints.Remove(buttonToHintTable[button]);
      trDataManager.Instance.AuthoringMissionInfo.Save();
      setView();
    }
    
    void reorder(trAuthoringMissionButtonBase button){
      Vector3 pos = ButtonParent.transform.InverseTransformPoint(button.transform.position) ;
      int index = getIndex(pos);
      trHint hint = buttonToHintTable[button];
      curPuzzle.Hints.Remove(hint);
      curPuzzle.Hints.Insert(index, hint);
      setView();
    }
    
    void showIndicator(trAuthoringMissionButtonBase button){
      Indicator.gameObject.SetActive(true);
      Vector3 pos = ButtonParent.transform.InverseTransformPoint(button.transform.position) ;
      getIndex(pos);
    }
    
    int getIndex(Vector3 pos){
      float height =  ButtonParent.cellSize.y + ButtonParent.spacing.y;
      int id = (int)((ButtonParent.padding.top - pos.y)/height);
      id = (int)Mathf.Clamp(id, 0, curPuzzle.Hints.Count - 1);
      Vector3 localPos = new Vector3(Indicator.localPosition.x, 
                                     -ButtonParent.padding.top - height *id,
                                     Indicator.localPosition.z);
      Indicator.position = ButtonParent.transform.TransformPoint(localPos);
      
      
      return id;
    }

    void AddHint(trProgram program = null){
      trHint hint = new trHint();
      if(program != null){
        hint.Program = program;
      }
      hint.Program.RobotType = trDataManager.Instance.MissionMng.GetCurMission().Type.GetRobotType();
      curPuzzle.Hints.Add(hint);
      trDataManager.Instance.AuthoringMissionInfo.Save();
      AddButton(curPuzzle.Hints.Count, hint);
    }
    
    void onButtonClicked(trAuthoringMissionButtonBase button){
      if(isAddMode){
        trProgram program =  buttonToHintTable[button].Program.DeepCopy();
        AddHint(program);
        onAddNewButtonClicked();
      }
      else{
        trDataManager.Instance.MissionMng.AuthoringMissionInfo.EditState = MissionEditState.EDIT_HINT_PROGRAM;
        trDataManager.Instance.IsMissionMode = true;

        trDataManager.Instance.AuthoringMissionInfo.CurHint = buttonToHintTable[button];
        AuthoringMissionMng.gameObject.SetActive(false);
        trDataManager.Instance.CurrentRobotTypeSelected = trDataManager.Instance.MissionMng.AuthoringMissionInfo.CurMission.Type.GetRobotType();
        trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.MAIN, trProtoController.RunMode.Challenges.ToString());
      }
    }

    void onNameInputChange(trAuthoringMissionButtonBase button){
      buttonToHintTable[button].UserFacingName = button.NameInput.text;
      trDataManager.Instance.AuthoringMissionInfo.Save();
    }

    public void onPointsInputChange(trAuthoringMissionButtonBase button){
      trHint hint = buttonToHintTable[button];
      int points = 0;
      int.TryParse(button.PointsInput.text, out points);
      hint.SubstractIQPoints = points;
      trDataManager.Instance.AuthoringMissionInfo.Save();
    }
    
    public void AddButton(int index, trHint hint){
      GameObject newButton = Instantiate(ButtonPrefab, Vector3.zero, Quaternion.identity)as GameObject;
      newButton.transform.SetParent(ButtonParent.transform, false);
      trAuthoringMissionButtonBase buttonCtrl = newButton.GetComponent<trAuthoringMissionButtonBase>();
      
      buttonCtrl.SetView(index, hint.UserFacingName);
      buttonCtrl.OnDeleteButtonClick += deleteButton;
      buttonCtrl.OnDragging += showIndicator;
      buttonCtrl.OnDragEnd += reorder;
      buttonCtrl.OnButtonClick += onButtonClicked;
      buttonCtrl.OnNameInputChange += onNameInputChange;
      buttonCtrl.OnPointsInputChange += onPointsInputChange;
      buttonCtrl.PointsInput.gameObject.SetActive(true);
      buttonCtrl.PointsInput.text = hint.SubstractIQPoints.ToString();
      buttonToHintTable.Add(buttonCtrl, hint );
      setNewBtnLast();
    }
  }
}

