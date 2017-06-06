using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Turing{
  public class trMissionButtonController : trListItemControl {
    
  	[SerializeField]
  	private Image _missionButtonImage;
  	[SerializeField]
  	private ImageThreeSliced _backgroundImage;
  	[SerializeField]
    private ImageThreeSliced _foregroundImage;
  	[SerializeField]
  	private Color _defaultColor;
  	[SerializeField]
  	private Color _activeColor;

  	[SerializeField]
  	private Sprite _lockedSprite;
  	[SerializeField]
  	private Sprite _inProgressSprite;
  	[SerializeField]
  	private Sprite _completeSprite;
  	[SerializeField]
  	private Animator _animator;

  	private CanvasGroup _canvasGroup;
  	//TODO: We don't need _state here, it should be passed in by trMissionListController.
  	//		CanStartOrResume should also be in trMissionListController.
  	// 		This class should only care about UI visualization, reduce dependency from trMissionListController.
  	private trState _state; 

      public void SetupView(trState state){
        _state = state;
        _canvasGroup =  this.gameObject.GetComponent<CanvasGroup>();
        NameText.text = wwLoca.Format(state.Behavior.MissionFileInfo.MissionName);
        UpdateView();
      }

      public bool CanStartOrResume {
        get {
          if (_state == null) {
            return false;
          }
          if (!_state.Behavior.IsMissionBehavior) {
            return false;
          }
          string uuid  = _state.Behavior.MissionFileInfo.UUID;
          bool isCompleted  = trDataManager.Instance.MissionMng.UserOverallProgress.IsMissionCompletedOnce(uuid);
          bool isUnlocked   = trDataManager.Instance.MissionMng.UserOverallProgress.IsMissionUnlocked     (uuid);
          bool ret = false;
          ret = ret || (isUnlocked && !isCompleted);
          return ret;
        }
      }

      public void UpdateView(){
        if(_state == null){
          return;
        }
        bool isCompleted = !_state.Behavior.IsMissionBehavior;
        string uuid = "";
        if(_state.Behavior.IsMissionBehavior){       
          uuid  = _state.Behavior.MissionFileInfo.UUID;
          isCompleted |= trDataManager.Instance.MissionMng.UserOverallProgress.IsMissionCompletedOnce(uuid);
        }   
        
        if(isCompleted){
          bool isReplaying = !trDataManager.Instance.MissionMng.UserOverallProgress.IsMissionCompleted(uuid);
          if(isReplaying){ //Replaying button
         	  SetMissionButton(_inProgressSprite, _defaultColor, true);
          }
          else{ // Completed button
            SetMissionButton(_completeSprite, _defaultColor, true);
          }
        }
        else{
          if(trDataManager.Instance.MissionMng.UserOverallProgress.IsMissionUnlocked(uuid)){ //InProgress button
            SetMissionButton(_inProgressSprite, _activeColor, true);
            if(!trDataManager.Instance.MissionMng.UserOverallProgress.IsMissionStarted(uuid)){ //New Unlocked button
            	//Play animation
  			if(_animator){
  				_animator.enabled = true;
  			}
            }
          }
          else{ //Locked button
            SetMissionButton(_lockedSprite, _defaultColor, false);
          }
        }
      }

  	private void SetMissionButton(Sprite sprite, Color color, bool active){
  	  _missionButtonImage.sprite = sprite;
  	  _foregroundImage.color = color;
  	  _backgroundImage.color = new Color(color.r, color.g, color.b, color.a/2f);
  	  _canvasGroup.ignoreParentGroups = true;
  	  _canvasGroup.interactable = active;
  	  _canvasGroup.alpha = active? 1: 0.4f;
    }
  }
}
