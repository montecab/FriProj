using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

namespace Turing{

  public class trDragDeleteController : MonoBehaviour{

    [SerializeField]
    private trProtoController _protoCtrl;
    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private RectTransform _animationRegion;
    [SerializeField]
    private Button _resetButton;
    [SerializeField]
    private trUndoRedoController _undoRedoCtrl;
    [SerializeField]
    private trTooltipPanelController _tooltip;

    private Vector3[] worldCorners;
    private float trashcanRadius;
    private bool isInTrashCan = false;
    private bool isClicked = false;
    private void Start (){
      if (_resetButton) {
        _resetButton.onClick.AddListener (onResetButtonClicked);
      }
    }

    public bool IsInTrashCanArea (Vector3 pos)
    {
      if (worldCorners == null) {
        worldCorners = new Vector3[4];
        _animationRegion.GetWorldCorners (worldCorners);
        trashcanRadius = (worldCorners [0] - worldCorners [3]).magnitude;
      }
      Vector3 a = new Vector3 (pos.x, pos.y, 0);
      Vector3 b = new Vector3 (this.gameObject.transform.position.x,
                    this.gameObject.transform.position.y,
                    0);
      float dis = (a - b).magnitude;
      if (_animator != null) {
        SetAnimation (dis < trashcanRadius * 1.5f);
      }
      return dis < trashcanRadius;
    }

    public void SetAnimation (bool active){
      if (isInTrashCan != active) {
        isInTrashCan = active;
        _animator.ChangeState((isInTrashCan)?1:2);
      }
    }

    private void onTooltipBGButtonClicked(){
      isClicked = false;
      _tooltip.Close();
    }

    private void onResetButtonClicked (){
      if (isClicked) {
        Reset();
        _tooltip.Close();
      }
      else {
        _tooltip.Display(wwLoca.Format("@!@Tap to undo all@!@"), onTooltipBGButtonClicked);
      }
      isClicked = !isClicked;
    }

    private void Reset ()
    {
      if (trDataManager.Instance.IsInNormalMissionMode) {
        new trTelemetryEvent(trTelemetryEventType.CHAL_RESET, true)
        .add(trTelemetryParamType.CHALLENGE, trDataManager.Instance.MissionMng.GetCurMission().UserFacingName)
        .add(trTelemetryParamType.STEP, trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.PuzzleIndex)
        .emit();
        trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.Program = trDataManager.Instance.MissionMng.UserOverallProgress.CurMissionInfo.ProgramForReset.DeepCopy();
        _protoCtrl.LoadProgram(trDataManager.Instance.GetCurProgram());
        trDataManager.Instance.SaveCurProgram();
      }
      else if (trDataManager.Instance.IsInFreePlayMode) {
        _undoRedoCtrl.UndoAll();
      }
      else {
        WWLog.logWarn("Try to reset program in neither mission nor free play");
      }
    }

  }
}
