using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using WW;
using WW.SimpleJSON;
using System.Text;
using UnityEngine.SceneManagement;

namespace Turing{
  public class trSharedItemDetailPanelController : trUIController {

    public trStateMachinePanelBase SMPanel;
    public trStateMachineRunningController SMRunningCtrl;
    public GameObject PublishedItemPanel;
    public TextMeshProUGUI Description;
    public TextMeshProUGUI ProgramNameLabel;
    public GameObject ProgramNameOnlyBackground;
    public GameObject ProgramNameAndDescriptionBackground;
    public TextMeshProUGUI AuthorLabel;
    public TextMeshProUGUI DownloadCountLabel;
    public GameObject DownloadCountStar;
    public Button DownloadButton;
    public Button BackButton;
    public Action BackButtonListener;
    public TextMeshProUGUI RemixDownloadPromptText;
    public GameObject RemixDownloadPrompt;
    public Button DismissRemixDownloadButton;

    public RectTransform RunningPanel;
    public Action<HTTPManager.RequestInfo> OnPayloadFinishListener;
    public TextMeshProUGUI RunLabel;

    private trSharedItemBase curItem;

    public bool downloadCountEnabled{
      set {
        DownloadCountLabel.enabled = value;
        DownloadCountStar.SetActive(value);
      }
    }

    public bool descriptionEnabled{
      set {
        Description.enabled = value;
        ProgramNameOnlyBackground.SetActive(!value);
        ProgramNameAndDescriptionBackground.SetActive(value);
      }
    }

    void Start(){
      SMRunningCtrl.RunButtonClickedListener += onRunBtnClick;
      DownloadButton.onClick.AddListener(onDownloadButtonClicked);
      BackButton.onClick.AddListener(onBackButtonClicked);
      DismissRemixDownloadButton.onClick.AddListener(onDismissRemixDownloadClick);
      RunLabel.text = wwLoca.Format("@!@Run@!@");
    }

    public void DownloadAndShowSharedItem(string token){
      curItem = new trPrivateSharedItem();
      ((trPrivateSharedItem)curItem).token = token;
      string url = trSharingConstants.GetSharedItemRESTUrl(token);
      downloadItem(url);
    }

    public void DownloadAndShowPublishedItem(trPublishedItem item){     
      curItem = item;
      string url = trSharingConstants.GetPublishedItemRESTUrl(((trPublishedItem)curItem).ProgramID);
      downloadItem(url);
    }

    void downloadItem(string url){
      HTTPManager.Instance.downloadUrl(url, onPayloadFinished,  trSharingConstants.DOWNLOAD_TIMEOUT);
      trDataManager.Instance.LoadingScreenCtrl.SetEnable(true);
    }

    public void LoadDescriptiveItem(trDescriptiveItem item){
      curItem = item;
      loadWithProgram(item.Program);
    }

    void loadWithProgram(trProgram program){
      //check to see if this is a newer version program to avoid breaking the app: TUR-2225
      if(program.IsFutureVersion){
        piConnectionManager.Instance.showSystemDialog("Incompatible Program", "This program requires a newer version of Wonder. Please update your Wonder app.");
        trDataManager.Instance.LoadingScreenCtrl.SetEnable(false);

        new trTelemetryEvent(trTelemetryEventType.PROGRAM_INCOMPATIBLE, true)
          .add(trTelemetryParamType.VER_NEW, program.Version)
          .add(trTelemetryParamType.VER_CURR, trProgram.CurrentVersion)
          .emit();

        return;
      }
      curItem.Program = program;
      ShowItemDetail(curItem);
      piUnityDelayedExecution.Instance.delayedExecution0(openPanel, 0.5f);
    }

    void openPanel(){
      trDataManager.Instance.LoadingScreenCtrl.SetEnable(false);
      if(this != null){
        this.gameObject.SetActive(true);
      }
    }

    void onPayloadFinished(HTTPManager.RequestInfo info){
      if(info.isDone){
        if(info.success){
          FilePayload payload = new FilePayload();
          JSONNode js = JSON.Parse(info.DownloadedText);
          payload.FromJson(js.AsObject);
          HTTPManager.Instance.downloadUrl(payload.Url, onFileDownloaded,  trSharingConstants.DOWNLOAD_TIMEOUT);
        }
        else{
          WWLog.logWarn("Payload downloading failed: " + info.ToString());
          trDataManager.Instance.InternetWarningManager.SetEnableWarningDialog(info);
          trDataManager.Instance.LoadingScreenCtrl.SetEnable(false);
        }
        if(OnPayloadFinishListener != null){
          OnPayloadFinishListener(info);
        }
      }

    }

    void onFileDownloaded(HTTPManager.RequestInfo info){     
      if(info.isDone){
        if(info.success){
          JSONNode js = JSON.Parse(info.DownloadedText);
          trProgram program = trFactory.FromJson<trProgram>(js.AsObject);
          loadWithProgram(program);
        }
        else{
          WWLog.logWarn("File downloading failed: " + info.ToString());
          trDataManager.Instance.InternetWarningManager.SetEnableWarningDialog(info);
          trDataManager.Instance.LoadingScreenCtrl.SetEnable(false);
        }

      }

    }

    protected override void onBackButtonClicked(){
      RemixDownloadPrompt.SetActive(false);
      this.gameObject.SetActive(false);
      if(BackButtonListener != null){
        BackButtonListener();
      }
    }

    void onDownloadButtonClicked(){
      if(curItem is trPublishedItem){
        new trTelemetryEvent(trTelemetryEventType.CMNTY_SAVE, true)
          .add(trTelemetryParamType.ROBOT_TYPE, curItem.Program.RobotType.ToString())
          .add(trTelemetryParamType.FILE_ID, ((trPublishedItem)curItem).ID)
          .emit();
      }
      else if (curItem is trPrivateSharedItem){
        new trTelemetryEvent(trTelemetryEventType.SHR_SAVE, true)
          .add(trTelemetryParamType.ROBOT_TYPE, curItem.Program.RobotType.ToString())
          .add(trTelemetryParamType.TOKEN, ((trPrivateSharedItem)curItem).token)
          .emit();
      }
      else if (curItem is trDescriptiveItem){
        new trTelemetryEvent(trTelemetryEventType.CHAL_SAVE, true)
          .add(trTelemetryParamType.ROBOT_TYPE, curItem.Program.RobotType.ToString())
          .add(trTelemetryParamType.CHALLENGE, ((trDescriptiveItem)curItem).Name)
          .emit();
      }

      trProgram program = curItem.Program.DeepCopy();
      program.RecentLoadedTime = System.DateTime.Now.ToFileTimeUtc();
      program.UUID = wwUID.getUID();
      if(curItem is trPublishedItem){
        // method handles shared and published files, so only do this for published files
        trPublishedItem item = ((trPublishedItem)curItem); 
        program.ParentToken = item.ProgramID;
        String url = trSharingConstants.GetItemDownloadCountRESTUrl(item);
        //WWLog.logInfo("incrementing download count: " + url);
        HTTPManager.Instance.Post(url, " ");
      }
      trDataManager.Instance.AppSaveInfo.AddProgram(program);
      trDataManager.Instance.CurrentRobotTypeSelected = program.RobotType;
      trDataManager.Instance.AppSaveInfo.CurProgram = program;
      trDataManager.Instance.AppSaveInfo.Save();
//      //TODO: add metrics
      trFileMenuPanelController.NewProgramRobotType = program.RobotType;
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.VAULT,  trFileMenuPanelController.DisplayMode.NewProgram.ToString());

    }

    public void ShowItemDetail(trSharedItemBase item){
      this.gameObject.SetActive(true);



      // show item descriptions
      if (item is trDescriptiveItem) {
        trDescriptiveItem descriptiveItem = (trDescriptiveItem)item;
        PublishedItemPanel.SetActive(true);
        if(Description != null){
          Description.text = descriptiveItem.Description;
        }       
      }

      // show additional for published item
      if(item is trPublishedItem){
        trPublishedItem publishedItem = (trPublishedItem)item;      
        DownloadCountLabel.text = publishedItem.Popularity.ToString();
        item.Program.UserFacingName = ((trPublishedItem)item).Name;
      }

      ProgramNameLabel.text = wwLoca.Format(item.Program.UserFacingName);

      curItem = item;
      SMPanel.SetUpView(item.Program);
      SMRunningCtrl.CurProgram = item.Program;
      SMPanel.StatePanel.transform.localPosition = Vector3.zero;
      trDataManager.Instance.CurrentRobotTypeSelected = item.Program.RobotType;
      trCurRobotController.Instance.CheckConnectRobot();
      wwUtility.FitContent2DShallow(SMPanel.StatePanel, 20, 20);
    }

    void onRunBtnClick(bool isrunning){
      if(isrunning){
        if(curItem is trPublishedItem){
          new trTelemetryEvent(trTelemetryEventType.CMNTY_RUN, true)
            .add(trTelemetryParamType.ROBOT_TYPE, curItem.Program.RobotType.ToString())
            .add(trTelemetryParamType.FILE_ID, ((trPublishedItem)curItem).ID)
            .emit();
        }
        else if (curItem is trPrivateSharedItem){
          new trTelemetryEvent(trTelemetryEventType.SHR_RUN, true)
            .add(trTelemetryParamType.ROBOT_TYPE, curItem.Program.RobotType.ToString())
            .add(trTelemetryParamType.TOKEN, ((trPrivateSharedItem)curItem).token)
            .emit();
        }
        else if (curItem is trDescriptiveItem){
          new trTelemetryEvent(trTelemetryEventType.CHAL_RUN, true)
            .add(trTelemetryParamType.ROBOT_TYPE, curItem.Program.RobotType.ToString())
            .add(trTelemetryParamType.CHALLENGE, ((trDescriptiveItem)curItem).Name)
            .emit();
        }
      }

      //animateRunningPanel();
      //TODO: add metrics
      if (!SMRunningCtrl.IsRunning && this.gameObject.activeSelf) {
        showRemixPrompt();
      }

      RunLabel.text = isrunning? wwLoca.Format("@!@Stop@!@") : wwLoca.Format("@!@Run@!@");
      RunLabel.color = isrunning ? new Color(210f/255f, 76f/255f, 83f/255f) : new Color(19f/255f, 220f/255f, 207f/255f);
    }

    void showRemixPrompt() {
      if (!RemixDownloadPrompt.activeSelf) {
        RemixDownloadPrompt.SetActive(true);
        DismissRemixDownloadButton.gameObject.SetActive(true);
      }
    }

    void onDismissRemixDownloadClick() {
      RemixDownloadPrompt.SetActive(false);
      DismissRemixDownloadButton.gameObject.SetActive(false);
    }
 
  }
}

public class FilePayload{
  public string FileID = "";
  public string MD5 = "";
  public string Url = "";
  public string Token = "";

  public const string JSON_FILEID = "id";
  public const string JSON_MD5 = "md5";
  public const string JSON_URL = "url";
  public const string JSON_FILETOKEN = "token";

  public void FromJson(JSONClass jsc){
    if(jsc[JSON_FILEID] != null){
      FileID = jsc[JSON_FILEID];
    }

    if(jsc[JSON_MD5] != null){
      MD5 = jsc[JSON_MD5];
    }

    if(jsc[JSON_URL] != null){
      Url = jsc[JSON_URL];
    }

    if(jsc[JSON_FILETOKEN] != null){
      Token = jsc[JSON_FILETOKEN];
    }
  }
}


