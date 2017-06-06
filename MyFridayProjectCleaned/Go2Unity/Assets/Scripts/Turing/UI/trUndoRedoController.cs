using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Turing{
  public class trUndoRedoController : MonoBehaviour {

    public trProtoController ProtoCtrl;
    public Button UndoButton;
    public Button RedoButton;

    private List<trProgram> history = new List<trProgram>();
    private int currentPositionInHistory = 0;
    private string curUUID = "";

    public bool IsUndoValid{
      get{
        return currentPositionInHistory >0;
      }
    }

    public bool IsRedoValid{
      get{
        return currentPositionInHistory < history.Count -1;
      }
    }

    private void Start(){
      UndoButton.onClick.AddListener(Undo);
      RedoButton.onClick.AddListener(Redo);
    }

    public void ClearHistory(){
      history.Clear();
      currentPositionInHistory = 0;
    }

    public void Save(trProgram program){
      if(program.UUID != curUUID){
        ClearHistory();
        curUUID = program.UUID;
      }
      for(int i = history.Count - 1; i>currentPositionInHistory; --i){
        history.RemoveAt(i);
      }
      history.Add(program.DeepCopy());
      currentPositionInHistory = history.Count -1;
      updateButtonView();
    }

    private void updateButtonView(){
      UndoButton.interactable = IsUndoValid;
      RedoButton.interactable = IsRedoValid;
      UndoButton.GetComponent<CanvasGroup>().alpha = (IsUndoValid)?1:0.25f;
      RedoButton.GetComponent<CanvasGroup>().alpha = (IsRedoValid)?1:0.25f;
    }

    public void UndoAll (){
      if (IsUndoValid) {
        currentPositionInHistory = 1;
        Undo();
      }
    }

    public void Undo (){
      if (IsUndoValid) {
        currentPositionInHistory -= 1;  
        ProtoCtrl.StateEditCtrl.UpdateUndoRedoView(history[currentPositionInHistory]);
        updateButtonView();

        if(trDataManager.Instance.IsInNormalMissionMode){
          new trTelemetryEvent(trTelemetryEventType.CHAL_UNDO, true)
            .add(trTelemetryParamType.CHALLENGE, trDataManager.Instance.MissionMng.GetCurMission().UserFacingName)
            .add(trTelemetryParamType.STEP, trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.PuzzleIndex)
            .emit();
        }
        else{
          new trTelemetryEvent(trTelemetryEventType.FP_UNDO, true).emit();
        }
      }
      else {
        WWLog.logError("undo called when invalid.");
      }
    }

    public void Redo(){
      if(IsRedoValid){
        currentPositionInHistory += 1;
        ProtoCtrl.StateEditCtrl.UpdateUndoRedoView(history[currentPositionInHistory]);
        updateButtonView();

        if(trDataManager.Instance.IsInNormalMissionMode){
          new trTelemetryEvent(trTelemetryEventType.CHAL_REDO, true)
            .add(trTelemetryParamType.CHALLENGE, trDataManager.Instance.MissionMng.GetCurMission().UserFacingName)
            .add(trTelemetryParamType.STEP, trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.PuzzleIndex)
            .emit();
        }
        else{
          new trTelemetryEvent(trTelemetryEventType.FP_REDO, true).emit();
        }
        //Debug.LogError("redo " + currentPositionInHistory);
      }
      else {
        WWLog.logError("redo called when invalid.");
      }
    }
  }
}

