using UnityEngine;
using System.Collections.Generic;
using WW.SaveLoad;

namespace Turing{
  public static class wwLoca {
    public static string Format(int pluralVal, string key, params object[] args){
      if (Turing.trMultivariate.isYES(Turing.trMultivariate.trAppOption.DEBUG_TRANSLATION_MODE)){
        bool isFound;
        string result = trDataManager.Instance.LocaManager.Format(pluralVal, key, out isFound, args);
        result = (isFound)?trMultivariate.DEBUG_TRANSLATION_TOKEN:result;
        return result;
      } else{
        return trDataManager.Instance.LocaManager.Format(pluralVal, key, args);
      }
    }
    public static string Format(string key, params object[] args) {
      if (Turing.trMultivariate.isYES(Turing.trMultivariate.trAppOption.DEBUG_TRANSLATION_MODE)){
        bool isFound;
        string result = trDataManager.Instance.LocaManager.Format(key, out isFound, args);
        result = (isFound)?trMultivariate.DEBUG_TRANSLATION_TOKEN:result;
        return result;
      } else{
        return trDataManager.Instance.LocaManager.Format(key, args);
      }
    }
  }

  public class trDataManager : Singleton<trDataManager> {

    private static readonly int TARGET_FRAME_RATE = 60;
    private static readonly string PATH_PO_FOLDER = "Strings/wonder";
    private const string PO_SAVED_FOLDER_NAME = "poFiles";
    private const string APP_NAME = "wonder";

    public trAppSaveInfo AppSaveInfo = new trAppSaveInfo();
    public trAppUserSettings AppUserSettings = new trAppUserSettings();
    public trUserFunctions UserFunctions = new trUserFunctions();
    public trVideoManager VideoManager = new trVideoManager();
    public wwLocaManager LocaManager = new wwLocaManager();

    private trMissionManager missionMng;
    public trMissionManager MissionMng {
      get {
        if(missionMng == null){
          missionMng = new trMissionManager();
        }
        return missionMng;
      }
    }

    public delegate void SaveProgramDelegate();
    public SaveProgramDelegate OnSaveCurProgram;

    public delegate void ChangeBehaviorMakerDelegate(bool value);
    public ChangeBehaviorMakerDelegate OnChangedBehaviorMakerFlag;

    public bool IsRCFreeplayInUse = false;

    private bool isBehaviorMakerEnabeld = false;
    public bool IsBehaviorMakerEnabeld {
      get {
        return isBehaviorMakerEnabeld;
      }
      set {
        isBehaviorMakerEnabeld = value;
        if (OnChangedBehaviorMakerFlag != null){
          OnChangedBehaviorMakerFlag(isBehaviorMakerEnabeld);
        }
      }
    }

    private trLoadingScreenPanelController loadingScreenCtrl;
    public trLoadingScreenPanelController LoadingScreenCtrl{
      get{
        if(loadingScreenCtrl == null){
          GameObject prefab =  Resources.Load("TuringProto/LoadingScreenCanvas",  typeof(GameObject)) as GameObject;
          GameObject newPanel = Instantiate(prefab) as GameObject;
          loadingScreenCtrl = newPanel.GetComponent<trLoadingScreenPanelController>();
          newPanel.transform.SetAsLastSibling();
        }
        return loadingScreenCtrl;
      }
    }


    private trInternetWarningManager internetWarningMng;
    public trInternetWarningManager InternetWarningManager{
      get{
        if(internetWarningMng == null){
          GameObject prefab =  Resources.Load("TuringProto/InternetWarningCanvas",  typeof(GameObject)) as GameObject;
          GameObject newPanel = Instantiate(prefab) as GameObject;
          internetWarningMng = newPanel.GetComponent<trInternetWarningManager>();
          newPanel.transform.SetAsLastSibling();
        }
        return internetWarningMng;
      }
    }

    public trAuthoringMissionInfo AuthoringMissionInfo{
      get{
        return MissionMng.AuthoringMissionInfo;
      }
    }  

    public int GetIQPoints(){
      if(MissionMng == null){
        return 0;
      }
      return MissionMng.UserOverallProgress.IQPoints;
    }  

    public bool IsInFreePlayMode{
      get{
        return !trDataManager.Instance.IsMissionMode 
          && trDataManager.Instance.MissionMng.AuthoringMissionInfo.EditState == MissionEditState.NORMAL;
      }
    }

    public bool IsInNormalMissionMode{
      get{
        return trDataManager.Instance.IsMissionMode 
          && trDataManager.Instance.MissionMng.AuthoringMissionInfo.EditState == MissionEditState.NORMAL;
      }
    }

    public trAuthoringMissionPanelController AuthoringMissionCtrl;
    public bool IsMissionMode = false;
    public bool SkipVault = false;   // whether we should go straight to freeplay from lobby
    public trNewMissionUnlockPanelController NewUnlockedMissionPanelCtrl;
    public bool IsAllowShowNewMissionPanel = true;
    public delegate void NewMissionUnlockDelegate();
    public NewMissionUnlockDelegate OnNewMissionUnlocked;
    public delegate void AuthoringModeChangeDelegate(bool isAuthoringMode);
    public AuthoringModeChangeDelegate onAuthorinModeChanged;
    public trAdminPanel AdminPanel = null;
    public trTouchCursorController TouchCursorCanvas = null;
    public piRobotType CurrentRobotTypeSelected = piRobotType.DASH;
    public bool optionsPanelShowInternal = false;

    private bool isInitialized = false;
    private GameObject _fpsMeter;

    public void Init(){
      if (isInitialized){
        return;
      }

      GraphicSetup();

      if (trMultivariate.Instance != null){
        trMultivariate.Instance.ValueChanged += OnValueChanged;
      }

      LocaManager.Init(APP_NAME, BuildInfo.AppVersion, getPOSavedPath());

      trMultivariate.trAppOption langOption = trMultivariate.trAppOption.LANGUAGE;
      OnValueChanged(langOption, trMultivariate.Instance.getOptionValue(langOption));

      MissionMng.Load();

      AppSaveInfo.Init();
      AppSaveInfo.Load();

      AppUserSettings.Load(); 

      UserFunctions.Load();
      trRewardsManager.Instance.Init();

      VideoManager.Load();
     
      initAdminPanel();
      trMultivariate.trAppOption touchCursorOption = trMultivariate.trAppOption.SHOW_TOUCH_CURSOR;
      OnValueChanged(touchCursorOption, trMultivariate.Instance.getOptionValue(touchCursorOption));
      trMultivariate.trAppOption fpsMeterOption = trMultivariate.trAppOption.SHOW_FPS_METER;
      OnValueChanged(fpsMeterOption, trMultivariate.Instance.getOptionValue(fpsMeterOption));

      isInitialized = true;
    }

    private void GraphicSetup(){
      Application.targetFrameRate = TARGET_FRAME_RATE;
    }

    private string getPOSavedPath(){
      return Application.persistentDataPath + "/" + PO_SAVED_FOLDER_NAME;
    }

    public void SaveBehavior(){
      if(AuthoringMissionInfo.EditState == MissionEditState.NORMAL){
        AppUserSettings.Save();
      }
      SaveCurProgram();
    }

    //not called anywhere, should we delete this?
    public void AddCurMissionProgramToMyProgramsAndLoadFreeplay(){
      new trTelemetryEvent(trTelemetryEventType.CHAL_SAVE, true)
        .add(trTelemetryParamType.CHALLENGE, MissionMng.GetCurMission().UserFacingName)
        .emit();

      trProgram program = MissionMng.UserOverallProgress.CurMissionInfo.Program;
      string    name    = MissionMng.GetCurMission().UserFacingName;
      AddProgramToMyProgramsAndLoadFreeplay(program, name);
    }
    
    public void AddProgramToMyProgramsAndLoadFreeplay(trProgram program, string name) {
      if( program.RobotType != CurrentRobotTypeSelected){
        if(MissionMng.GetCurMission() != null){
          WWLog.logWarn("Challenge " + MissionMng.GetCurMission().UserFacingName + " has wrong robot type in hints");
        }
        program.RobotType = CurrentRobotTypeSelected;
      }

      AppSaveInfo.AddMissionProgramAndSetCurProgram(program.DeepCopy(), name);
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.MAIN, trProtoController.RunMode.FreePlay.ToString());
    }

    public trProgram GetCurProgram(){
      if(IsMissionMode|| AuthoringMissionInfo.EditState != MissionEditState.NORMAL){
        return MissionMng.GetCurProgram();
      }
      else{
        return AppSaveInfo.CurProgram;
      }
    }

    public void SaveCurProgram(){
      if(IsMissionMode|| AuthoringMissionInfo.EditState != MissionEditState.NORMAL){
        MissionMng.SaveCurProgram();
      }
      else{
        AppSaveInfo.SaveCurProgram();
      }

      if(OnSaveCurProgram != null){
        OnSaveCurProgram();
      }
    }

    public bool IsInAuthoringMove{
      get{
        return (AuthoringMissionCtrl != null && AuthoringMissionCtrl.isActiveAndEnabled);
      }
    }

    void Update(){
      checkMissionUnlock();

      #if !UNITY_IPHONE && !UNITY_ANDROID
      if( IsOpenMissionAuthoringPanel()){
        OpenMissionAuthoringPanel();
      } else if (IsOpenAdminPanel()){
        AdminPanel.gameObject.SetActive(true);
      } else if (IsOpenInternalPanel()){
        Turing.trDataManager.Instance.optionsPanelShowInternal = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Options");
      } else if (IsOpenTeacherPanel()){
        Turing.trDataManager.Instance.optionsPanelShowInternal = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Options");
      }
      #endif


      string tmp = tappedCorner();
      if(tmp != null){
        trSecretAdminController.Instance.addToKeyPhrase(tmp);
      }
    }

    void checkMissionUnlock(){
      if(MissionMng.IsAnyMissionUnlockedNow()){
        if(NewUnlockedMissionPanelCtrl == null){
          GameObject prefab =  Resources.Load("TuringProto/MissionUnlockCanvas",  typeof(GameObject)) as GameObject;
          GameObject newMissionPanel = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
          NewUnlockedMissionPanelCtrl = newMissionPanel.GetComponent<trNewMissionUnlockPanelController>() ;
          newMissionPanel.transform.SetAsLastSibling();
          if (Application.isPlaying){
            DontDestroyOnLoad(newMissionPanel);
          }
        }
        NewUnlockedMissionPanelCtrl.Show();
        if(OnNewMissionUnlocked != null){
          OnNewMissionUnlocked();
        }
      }
    }

    void initAdminPanel(){
      if(AdminPanel == null){
        GameObject prefab =  Resources.Load("TuringProto/AdminCanvas",  typeof(GameObject)) as GameObject;
        GameObject adminPanel = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
        AdminPanel = adminPanel.GetComponent<trAdminPanel>() ;
        adminPanel.transform.SetAsLastSibling();
        if(Application.isPlaying){
          DontDestroyOnLoad(adminPanel);
        }
      }
    }

    void initTouchCursor() {
      if(TouchCursorCanvas == null){
        GameObject prefab =  Resources.Load("TuringProto/TouchCursorCanvas",  typeof(GameObject)) as GameObject;
        GameObject touchCursorCanvas = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
        TouchCursorCanvas = touchCursorCanvas.GetComponent<trTouchCursorController>() ;
        touchCursorCanvas.transform.SetAsLastSibling();
        if(Application.isPlaying){
          DontDestroyOnLoad(touchCursorCanvas);
        }
      }
    }

    public void OpenMissionAuthoringPanel(){
      if(AuthoringMissionCtrl == null ){
        GameObject prefab =  Resources.Load("TuringProto/AuthoringMissionCanvas",  typeof(GameObject)) as GameObject;
        GameObject adminPanel = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
        AuthoringMissionCtrl = adminPanel.GetComponent<trAuthoringMissionPanelController>() ;
        adminPanel.transform.SetAsLastSibling();
        if(Application.isPlaying){
          DontDestroyOnLoad(adminPanel);
        }
      }
      
      AuthoringMissionCtrl.SetActive(true);
    }

    public bool IsOpenMissionAuthoringPanel(){
      bool isAlt = Input.GetKey(KeyCode.LeftAlt) ||Input.GetKey(KeyCode.RightAlt);
      
      if( isAlt && Input.GetKeyDown(KeyCode.C)){
        return true;
      }
      return false;
    }

    public bool IsOpenAdminPanel(){
      bool isAlt = Input.GetKey(KeyCode.LeftAlt) ||Input.GetKey(KeyCode.RightAlt);
      
      if( isAlt && Input.GetKeyDown(KeyCode.A)){
        return true;
      }
      return false;
    }

    public bool IsOpenInternalPanel(){
      bool isAlt = Input.GetKey(KeyCode.LeftAlt) ||Input.GetKey(KeyCode.RightAlt);
      
      if( isAlt && Input.GetKeyDown(KeyCode.I)){
        return true;
      }
      return false;
    }

    public bool IsOpenTeacherPanel(){
      bool isAlt = Input.GetKey(KeyCode.LeftAlt) ||Input.GetKey(KeyCode.RightAlt);
      
      if( isAlt && Input.GetKeyDown(KeyCode.T)){
        return true;
      }
      return false;
    }

    string tappedCorner(){
      if(Input.GetMouseButtonDown(0)){
        Vector2 pos = Input.mousePosition;
        if(pos.x/Screen.width < 0.1f){
          return "L";
        }

        if(pos.x/Screen.width > 0.9f){
          return "R";
        }
        
      }
     
      return null;
    }

    private Dictionary<string, object> permanentItems = new Dictionary<string, object>();
    public T getPermanentItem<T>(string key) where T:class,new() {
      if (!permanentItems.ContainsKey(key)) {
        permanentItems.Add(key, new T());
      }

      object ret = permanentItems[key];

      if (ret is T) {
        return ret as T;
      }
      else {
        WWLog.logError("asked for type " + typeof(T).ToString() + " but found " + ret.GetType());
        return null;
      }
    }

    private void OnValueChanged(trMultivariate.trAppOption option, trMultivariate.trAppOptionValue newValue){
      if (option == trMultivariate.trAppOption.LANGUAGE) {
        if (newValue == trMultivariate.trAppOptionValue.SYSTEM) {
          wwLocaManager.overrideLanguage = null;
        }
        else {
          wwLocaManager.overrideLanguage = newValue.ToString();
        }
        string language = wwLocaManager.getSystemTextLanguage();
        LocaManager.SetLanguage(language, PATH_PO_FOLDER, getPOSavedPath());
      } else if (option == trMultivariate.trAppOption.SHOW_TOUCH_CURSOR) {
        if (newValue == trMultivariate.trAppOptionValue.YES) {
          initTouchCursor();
        }
      } else if(option==trMultivariate.trAppOption.SHOW_FPS_METER){
        if(newValue == trMultivariate.trAppOptionValue.YES && _fpsMeter==null){
          _fpsMeter = Instantiate(Resources.Load("TuringProto/FPSMeter")) as GameObject;
        }
      }
    }
    
#if UNITY_IOS || UNITY_ANDROID
    void OnApplicationPause(bool isPaused) {
      if (!isPaused) {
        SkipVault = false;
      }
    }
#endif
  }
}
