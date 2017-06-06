using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;
using Turing;
using WW.SimpleJSON;

namespace Turing {
  public class trVaultScrollQuestsController : MonoBehaviour {

    public Transform ScrollQuestsContainer;
    public TextMeshProUGUI ExampleAreaNameLabel;
    public trVaultExampleChallenge ExampleChallenge;
    
    public trVaultPopUpController PopUpPanel;
    public trUserOverallProgress SQProgress;
    
    /*
    New new: Light Pink: 168, 100, 168
    old new: Dark Pink: 168, 100, 168 56% opacity
    unlocked not played: Dark Purple: 96, 92, 168 56% opacity
    Unlocked playable: Light Purple: 116, 89, 213
    */  
    static Color clrNewNew = wwColorUtil.newColor(168, 100, 168, 1.00f);
    static Color clrOldNew = wwColorUtil.newColor(168, 100, 168, 0.56f);
    static Color clrLockLk = wwColorUtil.newColor( 96,  92, 168, 0.56f);
    static Color clrLockAv = wwColorUtil.newColor(116,  89, 213, 1.00f);
    
    void Start () {
      Setup();
    }

    void Setup() {    
      // new trTelemetryEvent(trTelemetryEventType.APPNAV_VAULT, true)
      //   .add(trTelemetryParamType.ROBOT_TYPE, trDataManager.Instance.CurrentRobotTypeSelected.ToString())
      //   .emit();
    }

    public void ShowPanel() {
      PopulateScrollQuests(); // always fetch the latest data
      gameObject.SetActive(true);
    }

    void PopulateScrollQuests() {
      // first, clear out all the existing scroll quests, if any
      for (int n = ScrollQuestsContainer.childCount - 1; n >= 0; --n) {
        Transform child = ScrollQuestsContainer.GetChild(n);
        if ((child != ExampleAreaNameLabel.transform) && (child != ExampleChallenge.transform)) {
          GameObject.Destroy(child.gameObject);
        }
      }

      // now populate the container with the quests in their respective regions
      Dictionary<trMapArea, List<trMissionFileInfo>> areaList = trDataManager.Instance.MissionMng.AuthoringMissionInfo.buildAreaList();

      foreach(trMapArea trMA in areaList.Keys) {
        TextMeshProUGUI label = GameObject.Instantiate<TextMeshProUGUI>(ExampleAreaNameLabel);
        label.gameObject.SetActive(true);
        label.text = trMA.UserFacingName;
        label.transform.SetParent(ScrollQuestsContainer);
        label.transform.localScale = Vector3.one;

        List<trMissionFileInfo> missionList = areaList[trMA];
        foreach(trMissionFileInfo trMFI in missionList) {
          trVaultExampleChallenge chlng = GameObject.Instantiate<trVaultExampleChallenge>(ExampleChallenge);
          chlng.gameObject.SetActive(true);
          chlng.TxtName.text = trMFI.MissionName;
          chlng.UUID         = trMFI.UUID;            // stashed in here to get the lambda below to work.

          trMissionFileInfo lambdaCapture = trMFI;
          chlng.BtnMain.onClick.AddListener(() => {
            onClickChallenge(lambdaCapture);
          });
          chlng.transform.SetParent(ScrollQuestsContainer);
          chlng.transform.localScale = Vector3.one;

          bool available = SQProgress.IsMissionCompletedOnce (trMFI.UUID);
          bool canStart  = SQProgress.IsMissionUnlocked      (trMFI.UUID);
          bool isRecent  = SQProgress.IsMissionRecentlySolved(trMFI.UUID);

          chlng.ImgLockedTotally.gameObject.SetActive(false);
          chlng.ImgLockedLoose  .gameObject.SetActive(false);
          chlng.ImgOldNew       .gameObject.SetActive(false);
          chlng.ImgNewNew       .gameObject.SetActive(false);
          chlng.BtnInfo         .gameObject.SetActive(false);


          if (available) {
            // the solution to this challenge is available as a program.
            //        chlng.BtnInfo         .gameObject.SetActive(true);
            //        chlng.BtnInfo.onClick.AddListener(() => {
            //          onClickChallengeInfo(chlng.UUID);
            //        });

            if (isRecent) {
              chlng.ImgNewNew   .gameObject.SetActive(true);
              chlng.BtnMain     .image.color = clrNewNew;
            }
            else {
              chlng.ImgOldNew   .gameObject.SetActive(true);
              chlng.BtnMain     .image.color = clrOldNew;
            }
          }
          else {
            // the solution to this challenge is not currently available as a program.
            if (canStart) {
              chlng.ImgLockedLoose  .gameObject.SetActive(true);
              chlng.BtnMain         .image.color = clrLockAv;
            }
            else {
              chlng.ImgLockedTotally.gameObject.SetActive(true);
              chlng.BtnMain         .image.color = clrLockLk;
              chlng.TxtName         .color       = new Color(1f, 1f, 1f, 0.5f);

            }
          }
        }
      }
    }
    
    private trMission selectedMission = null;
    
    void onClickChallenge(trMissionFileInfo trMFI) {
      bool      available = SQProgress.IsMissionCompletedOnce(trMFI.UUID);
      bool      canStart  = SQProgress.IsMissionUnlocked     (trMFI.UUID);
      trMission trM       = trDataManager.Instance.AuthoringMissionInfo.LoadMissionFromUUID(trMFI.UUID);
      selectedMission = trM;
      
      if (available) {    
        trProgram trP = trM.Puzzles[trM.Puzzles.Count - 1].LastHint.Program;
        PopUpPanel.Title.text = trMFI.MissionName;
        PopUpPanel.ShowProgram(trP);
        PopUpPanel.ShowCallToActionText("Work on a new copy of this ?");
        PopUpPanel.ShowCopyYes(true, onClickCopyChallenge);
      }
      else {
        if (canStart) {
          PopUpPanel.Title.text = "Solve Challenge";
          PopUpPanel.ShowDescription("To unlock\n<b>" + trM.UserFacingName + "</b>\nyou need to solve it.");
          PopUpPanel.ShowCallToActionText("Would you like to solve it now ?");
          PopUpPanel.ShowCopyYes(false, onClickStartChallenge);      
        }
        else {
          PopUpPanel.Title.text = "Solve More";
          PopUpPanel.ShowChallengeMap();
          PopUpPanel.ShowCallToActionText("Solve more challenges to unlock.");
          PopUpPanel.ShowCopyYes(false, onClickPlayMoreChallenges);
        }
      }
      PopUpPanel.ShowPanel();
    }
    
    void onClickCopyChallenge() {
      if (selectedMission == null) {
        WWLog.logError("unexpected: selectedMission is null");
        return;
      }
      
      // load challenge into freeplay, and add to My Programs.
      
      new trTelemetryEvent(trTelemetryEventType.CHAL_SAVE, true)
        .add(trTelemetryParamType.CHALLENGE, selectedMission.UserFacingName)
        .emit();
      trProgram trP = selectedMission.Puzzles[selectedMission.Puzzles.Count - 1].LastHint.Program;
      trDataManager.Instance.AddProgramToMyProgramsAndLoadFreeplay(trP, selectedMission.UserFacingName);
    }
    
    void onClickStartChallenge() {
      if (selectedMission == null) {
        WWLog.logError("unexpected: selectedMission is null");
        return;
      }
      
      trDataManager.Instance.MissionMng.LoadMission(selectedMission.UUID);
      trDataManager.Instance.MissionMng.AuthoringMissionInfo.EditState = MissionEditState.NORMAL;

      trMissionListController.loadMissionScene();
    }
    
    void onClickPlayMoreChallenges() {
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.MAP);
    }

    private delegate void jsonRecurseDelegate(string name, bool isDirectory, bool hasFileChildren, string path);
    private static HashSet<string> interestingExtensions = new HashSet<string> {".txt"};

    void _recurseJson(JSONArray jsa, jsonRecurseDelegate jrd, string path) {
      foreach (JSONClass jsc in jsa) {
        bool isDir = jsc.ContainsKey("children");
        bool hasFiles = isDir ? wwFileUtil.hasFileEntries(jsc["children"].AsArray, interestingExtensions) : false;
        jrd(jsc["name"], isDir, hasFiles, path);
        if (isDir) {
          _recurseJson(jsc["children"].AsArray, jrd, path + jsc["name"] + "/");
        }
      }
    }
    
  }
}