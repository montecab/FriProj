using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Turing{
  public class trAuthoringPuzzleListController: MonoBehaviour {

  	public GameObject ButtonPrefab;
    public GridLayoutGroup ButtonParent;
    public Button AddNewButton;
    public Transform Indicator;
    public trAuthoringPuzzleConfigPanelController PuzzleConfigPanelCtrl;

    public trAuthoringMissionPanelController AuthoringMissionMng;
    public Text ErrorInfo;

    private trMission mission;
    private Dictionary<trAuthoringMissionButtonBase, trPuzzle> buttonToPuzzleTable = new Dictionary<trAuthoringMissionButtonBase,trPuzzle>();


    void Start(){
      AddNewButton.onClick.AddListener(()=>onAddNewButtonClicked());
    }

    public void SetUp(trMission mission_set){
      mission = mission_set;
      initView();
    }

    void onAddNewButtonClicked(){
      trPuzzle puzzle= new trPuzzle();
      trHint hint = new trHint();
      if(mission.Puzzles.Count > 0){
        trPuzzle prevPuzzle =  mission.Puzzles[mission.Puzzles.Count - 1];
        trProgram prevPuzzleTargetProgram = prevPuzzle.Hints[prevPuzzle.Hints.Count - 1].Program;
        hint.Program = prevPuzzleTargetProgram.DeepCopy();
      }
      puzzle.Hints.Add(hint);
      mission.Puzzles.Add(puzzle);
      trDataManager.Instance.AuthoringMissionInfo.Save();
      AddButton(mission.Puzzles.Count, puzzle);
    }

    void setNewBtnLast(){
      AddNewButton.transform.SetAsLastSibling();
    }

    void initView(){
      reset();
      for(int i = 0; i< mission.Puzzles.Count; ++i){
        AddButton(i + 1, mission.Puzzles[i]);
      }
    }

    public void SetListView(){
      foreach(trAuthoringMissionButtonBase button in buttonToPuzzleTable.Keys){
        button.NameInput.text = buttonToPuzzleTable[button].UserFacingName;
      }
    }

    void reset(){
      Indicator.gameObject.SetActive(false);
      foreach(trAuthoringMissionButtonBase button in buttonToPuzzleTable.Keys){
        Destroy(button.gameObject);
      }
      buttonToPuzzleTable.Clear();
    }

    void deleteButton(trAuthoringMissionButtonBase button){
      mission.Puzzles.Remove(buttonToPuzzleTable[button]);
      trDataManager.Instance.AuthoringMissionInfo.Save();
      reset();
      initView();
    }

    void reorder(trAuthoringMissionButtonBase button){
      Vector3 pos = ButtonParent.transform.InverseTransformPoint(button.transform.position) ;
      int index = getIndex(pos);
      trPuzzle puzzle = buttonToPuzzleTable[button];
      mission.Puzzles.Remove(puzzle);
      mission.Puzzles.Insert(index, puzzle);
      reset ();
      initView();
    }

    void showIndicator(trAuthoringMissionButtonBase button){
      Indicator.gameObject.SetActive(true);
      Vector3 pos = ButtonParent.transform.InverseTransformPoint(button.transform.position) ;
      getIndex(pos);
    }

    int getIndex(Vector3 pos){
      float height =  ButtonParent.cellSize.y + ButtonParent.spacing.y;
      int id = (int)((ButtonParent.padding.top - pos.y)/height);
      id = (int)Mathf.Clamp(id, 0, mission.Puzzles.Count - 1);
      Vector3 localPos = new Vector3(Indicator.localPosition.x, 
                                     -ButtonParent.padding.top - height *id,
                                     Indicator.localPosition.z);
      Indicator.position = ButtonParent.transform.TransformPoint(localPos);


      return id;
    }

    void onPuzzleButtonClicked(trAuthoringMissionButtonBase button){
      trPuzzle puzzle = buttonToPuzzleTable[button];
      PuzzleConfigPanelCtrl.SetActive(puzzle);
      trDataManager.Instance.AuthoringMissionInfo.CurPuzzle = puzzle;
    }

    void onNameInputChange(trAuthoringMissionButtonBase button){
      buttonToPuzzleTable[button].UserFacingName = button.NameInput.text;
      trDataManager.Instance.AuthoringMissionInfo.Save();
    }

    void onRunButtonClicked(trAuthoringMissionButtonBase button){
      int index = trDataManager.Instance.AuthoringMissionInfo.CurMission.Puzzles.IndexOf (buttonToPuzzleTable[button]);
      trDataManager.Instance.MissionMng.UpdateUserOverallProgress(trDataManager.Instance.AuthoringMissionInfo.CurMission.UUID);
      trDataManager.Instance.MissionMng.LoadMission(trDataManager.Instance.AuthoringMissionInfo.CurMission.UUID);
      trDataManager.Instance.AuthoringMissionCtrl.SetActive(false);
      trDataManager.Instance.CurrentRobotTypeSelected = trDataManager.Instance.AuthoringMissionInfo.CurMission.Type.GetRobotType();

      trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.PuzzleIndex = index;
      trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.Program = trDataManager.Instance.AuthoringMissionInfo.CurMission.Puzzles[index].Hints[0].Program.DeepCopy();
      trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.HintIndex = 0;
      AuthoringMissionMng.SetActive(false);
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.MAIN, trProtoController.RunMode.Challenges.ToString());
    }

    public void onPointsInputChange(trAuthoringMissionButtonBase button){
      trPuzzle puzzle = buttonToPuzzleTable[button];
      int points = 0;
      int.TryParse(button.PointsInput.text, out points);
      puzzle.IQPoints = points;
      trDataManager.Instance.AuthoringMissionInfo.Save();
    }
     
    public void AddButton(int index, trPuzzle puzzle){
      GameObject newButton = Instantiate(ButtonPrefab, Vector3.zero, Quaternion.identity)as GameObject;
      newButton.transform.SetParent(ButtonParent.transform, false);
      trAuthoringMissionButtonBase buttonCtrl = newButton.GetComponent<trAuthoringMissionButtonBase>();

      buttonCtrl.SetView(index, puzzle.UserFacingName, true);
      buttonCtrl.OnDeleteButtonClick += deleteButton;
      buttonCtrl.OnDragging += showIndicator;
      buttonCtrl.OnDragEnd += reorder;
      buttonCtrl.OnButtonClick += onPuzzleButtonClicked;
      buttonCtrl.OnNameInputChange += onNameInputChange;
      buttonCtrl.OnRunButtonClick += onRunButtonClicked;
      buttonCtrl.OnPointsInputChange += onPointsInputChange;
      buttonCtrl.PointsInput.gameObject.SetActive(true);
      buttonCtrl.PointsInput.text = puzzle.IQPoints.ToString();
      buttonToPuzzleTable.Add(buttonCtrl, puzzle );
      setNewBtnLast();
    }
  }
}
