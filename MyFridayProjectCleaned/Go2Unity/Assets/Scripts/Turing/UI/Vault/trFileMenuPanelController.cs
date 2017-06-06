using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

namespace Turing{
  public class trFileMenuPanelController :  trBrowsePanelBase{

    [SerializeField]
    private GameObject _programPrefab;
    public trStateMachinePanelBase ScreenShotPanel;
    public trScreenShotController ScreenShotCtrl;

    [SerializeField]
    private trSharedItemDetailPanelController _sharedItemDetailPanelPrefab;
    [SerializeField]
    private Transform _sharedItemDetailPanelHolder;
    private trSharedItemDetailPanelController _sharedItemDetailPanel;

    [SerializeField]
    private trVaultMainPanel _mainPanelPrefab;
    [SerializeField]
    private Transform _mainPanelHolder;
    private trVaultMainPanel _mainPanel;

    [SerializeField]
    private trVaultEditPanel _editPanelPrefab;
    [SerializeField]
    private Transform _editPanelHolder;
    private trVaultEditPanel _editPanel;
    private trVaultExampleMyProgram _editPanelProgram;

    [SerializeField]
    private trVaultDeletePanel _deletePanelPrefab;
    [SerializeField]
    private Transform _deletePanelHolder;
    private trVaultDeletePanel _deletePanel;
    private trVaultExampleMyProgram _deletePanelProgram;

    [SerializeField]
    private trVaultLeftPanel _leftPanelPrefab;
    [SerializeField]
    private Transform _leftPanelHolder;
    private trVaultLeftPanel _leftPanel;

    [SerializeField]
    private trVaultTopPanel _topPanelPrefab;
    [SerializeField]
    private Transform _topPanelHolder;
    private trVaultTopPanel _topPanel;

    [SerializeField]
    private trCodeRedeemPanelController _codeRedeemPanelPrefab;
    [SerializeField]
    private Transform _codeRedeemPanelHolder;
    private trCodeRedeemPanelController _codeRedeemPanel;

    [Range(0.03f, 1.0f)]
    [SerializeField]
    private float maxThumbnailLoadTime = 0.03f;
    private Rect _viewportRect;
    private Vector3[] _worldCorners = new Vector3[4];

    public GameObject TutorialCanvasPrefab;

    public static piRobotType NewProgramRobotType = piRobotType.DASH;
    // only used for new program created

    public Button BackButton;

    public enum DisplayMode{
      MainMenu = 0,
      ScrollQuests = 1,
      MyPrograms = 2,
      NewProgram = 3
    }

    private DisplayMode currentDisplayMode;

    private bool isBackFilling = true;


    IEnumerator backfillScreenshots(){   
      bool regenAll = trMultivariate.isYES(trMultivariate.trAppOption.REGENERATE_ALL_THUMBNAILS);
      int dirtyTotalCount = 0;
      if(regenAll){
        dirtyTotalCount = trDataManager.Instance.AppSaveInfo.Programs.Count;
      } else{
        foreach(trProgram program in trDataManager.Instance.AppSaveInfo.Programs){
          if(program.ThumbnailDirty){
            dirtyTotalCount++;
          }
        }
      }


      int curCount = 1;
      trDataManager.Instance.LoadingScreenCtrl.SetEnable(true, getLoadingShowingText(curCount, dirtyTotalCount));
      // now load all the known programs
      trDataManager.Instance.AppSaveInfo.Programs.Sort((trProgram first, trProgram second) =>{
        return second.RecentLoadedTime.CompareTo(first.RecentLoadedTime);
      });
      bool saveProgramInfo = false;


      foreach(trProgram program in trDataManager.Instance.AppSaveInfo.Programs){
        if(regenAll || program.ThumbnailDirty){
          trDataManager.Instance.AppSaveInfo.initializeProgram(program);
          ScreenShotPanel.SetUpView(program);
          yield return new WaitForSeconds(0.2f);
          ScreenShotCtrl.takeScreenshot(program);
          yield return new WaitForSeconds(0.1f);
          program.ThumbnailDirty = false;
          saveProgramInfo = true;
          curCount++;
          trDataManager.Instance.LoadingScreenCtrl.UpdateText(getLoadingShowingText(curCount, dirtyTotalCount));
        }
      }
      if(saveProgramInfo){
        trDataManager.Instance.AppSaveInfo.Save();
      }
      isBackFilling = false;
      trDataManager.Instance.LoadingScreenCtrl.SetEnable(false);
      setupView();

    }


    string getLoadingShowingText(int cur, int total){
      if(total < 3){
        return "Loading...";
      }
      return string.Format("Updating your amazing programs: {0} of {1}", cur, total);
    }


    protected override void onRobotTypeChange(bool isOn){
      base.onRobotTypeChange(isOn);
      _mainPanel.setupButtonsForRobot(CurRobotType);
    }

    protected override void setRobotType(){
      piStringUtil.ParseStringToEnum<DisplayMode>(trNavigationRouter.Instance.GetTransitionParameterForScene(), out currentDisplayMode);
      if(currentDisplayMode == DisplayMode.NewProgram){
        CurRobotType = NewProgramRobotType;
      } else{
        if(piConnectionManager.Instance.LastConnectedRobot != null){
          CurRobotType = piConnectionManager.Instance.LastConnectedRobot.robotType;
        } else{
          CurRobotType = trDataManager.Instance.CurrentRobotTypeSelected;
        }
      }
    }

    private T instantiateInHolder<T>(T prefab, Transform holder) where T : Component {
      T result = Instantiate(prefab);
      result.transform.SetParent(holder, false);
      return result;
    }

    void instantiatePanels(){
      if(_mainPanel == null){
        _mainPanel = instantiateInHolder(_mainPanelPrefab, _mainPanelHolder);
        _mainPanel.createProgramAction = onCreateProgramButtonClicked;
        _mainPanel.enterKeyAction = onClickRedeemButton;
        _scrollCtrl = _mainPanel.GetComponent<ScrollRect>();
      }
      if(_leftPanel == null){
        _leftPanel = instantiateInHolder(_leftPanelPrefab, _leftPanelHolder);
        _robotToggleLeftPanelButton = _leftPanel.robotToggleButton;
        if(trMultivariate.isYESorSHOW(trMultivariate.trAppOption.VAULT_ALLOW_INTERNAL)){
          _leftPanel.secretButtonAction = onClickSecretButton;
        }
      }
      if(_topPanel == null){
        _topPanel = instantiateInHolder(_topPanelPrefab, _topPanelHolder);
        _robotToggle = _topPanel.robotToggle;
        _dashImg = _topPanel.dashActive;
        _dotImg = _topPanel.dotActive;
        _curTab = _topPanel.browseTab;
      }
    }

    protected override void initView(){
      instantiatePanels();
      base.initView();
      trCurRobotController.Instance.CheckRobotType = true;
      trDataManager.Instance.Init();
      StartCoroutine(backfillScreenshots());
      BackButton.onClick.AddListener(onClickBack);
      calculateViewportRect(0);
      if(FTUEManager.Instance.ShouldDisplayFTUE(FTUEType.FREEPLAY_PAGE)){
        piConnectionManager.Instance.hideChromeButton();
        GameObject tutorial = Instantiate(TutorialCanvasPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        tutorial.transform.SetParent(this.transform, false);
        trFTUEController ftueCtrl = tutorial.GetComponent<trFTUEController>();
        new trTelemetryEvent(trTelemetryEventType.WONDER_FTUE_FREEPLAY, true).emit();
        ftueCtrl.SetupView(FTUEType.FREEPLAY_PAGE, onCreateProgramButtonClicked);
      }
    }

    private void OnEnable(){
      WW.BackButtonController.Instance.AddListener(onClickBack);
      piConnectionManager.Instance.OnChromeClose += setRobotType;
    }

    private void OnDisable(){
      if(WW.BackButtonController.Instance != null){
        WW.BackButtonController.Instance.RemoveListener(onClickBack);
      }
      if(piConnectionManager.Instance != null){
        piConnectionManager.Instance.OnChromeClose -= setRobotType;
      }
    }

    void Update() {
      calculateViewportRect(_scrollCtrl.velocity.x);
    }

    void onClickSecretButton(){
      trNavigationRouter.Instance.ShowSceneWithName("InternalTesting");
    }

    void onClickRedeemButton(){
      if(_codeRedeemPanel == null){
        _codeRedeemPanel = instantiateInHolder(_codeRedeemPanelPrefab, _codeRedeemPanelHolder);
        if (_sharedItemDetailPanel == null) {
          _sharedItemDetailPanel = instantiateInHolder(_sharedItemDetailPanelPrefab, _sharedItemDetailPanelHolder);
          _sharedItemDetailPanel.descriptionEnabled = false;
          _sharedItemDetailPanel.downloadCountEnabled = false;
        }
        _codeRedeemPanel.DetailPanelCtrl = _sharedItemDetailPanel;
      }
      _codeRedeemPanel.gameObject.SetActive(true);
    }

    void onClickBack(){
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.LOBBY);
    }

    void setDisplayMode(DisplayMode mode){      
//      switch (mode) {
//      case DisplayMode.MainMenu:
//        MainUI.SetActive(true);
//        MyProgramsController.gameObject.SetActive(false);
//        ScrollQuestsController.gameObject.SetActive(false);
//        TitleLabel.text = "File menu";
//        break;
//      case DisplayMode.MyPrograms:
//        MainUI.SetActive(false);
//        TitleLabel.text = "My programs";
//        MyProgramsController.ShowPanel();
//        break;
//      case DisplayMode.ScrollQuests:
//        MainUI.SetActive(false);
//        TitleLabel.text = "Scroll quests";
//        ScrollQuestsController.ShowPanel();
//        break;
//      }
      currentDisplayMode = mode;
    }


    void onDeletePanelBack(){
      uGUIPanelTween.Instance.TweenClose(_deletePanel.transform);
    }

    void setupView(){
      trDataManager.Instance.Init();
      _topPanel.browseTab.TabToggle.isOn = true;
      onToggleTab(true, _topPanel.browseTab);

      if(currentDisplayMode == DisplayMode.NewProgram){
        animateFirstProgram(0.2f);
      }
    }

    protected override void onToggleTab(bool isOn, trBrowseTabBase btn){
      if(isBackFilling){
        return;
      }
      base.onToggleTab(isOn, btn);
    }

    public void AddNewProgram(trProgram program){
      GameObject list = _mainPanel.programListForRobotType(program.RobotType);
      trVaultExampleMyProgram item = createProgramObj(program, list);
      item.transform.SetAsFirstSibling();
      animateFirstProgram();
    }

    void animateFirstProgram(float delay = 0){
      GameObject list = _mainPanel.programListForRobotType(CurRobotType);
      Transform item = list.transform.GetChild(0);
      trVaultExampleMyProgram ctrl = item.GetComponent<trVaultExampleMyProgram>();
      ctrl.BG.localScale = Vector3.zero;     
      ctrl.BG.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(delay);

      //this is for some case the thumbnail is not yet generated so we need to reload the image
      ctrl.LabelFilename.text = ctrl.Program.UserFacingName;
      StartCoroutine(loadImage(ctrl));
    }

    void onCreateProgramButtonClicked(){
      trProgram newProgram = trDataManager.Instance.AppSaveInfo.CreateProgram(CurRobotType);

      new trTelemetryEvent(trTelemetryEventType.FP_PROGRAM_NEW, true)
        .add(trTelemetryParamType.ROBOT_TYPE, CurRobotType)
        .add(trTelemetryParamType.NUM_PROGS, trDataManager.Instance.AppSaveInfo.Programs.Count)
        .emit();
      
      loadProgram(newProgram);
    }

    private void loadFreePlay(){
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.MAIN, trProtoController.RunMode.FreePlay.ToString());
    }

    void onClickRenameMyProgram(trVaultExampleMyProgram trVEMP){
      if(_editPanel == null){
        _editPanel = instantiateInHolder(_editPanelPrefab, _editPanelHolder);
        _editPanel.nameChangedAction = onNameChanged;
        _editPanel.endEditAction = onEditNameFinish;
      }
      _editPanel.gameObject.SetActive(true);
      _editPanel.activateInputField();
      _editPanel.text = trVEMP.Program.UserFacingName;
      _editPanelProgram = trVEMP;
    }

    void onNameChanged(string s){
      _editPanel.text = trProgram.sanitizeFilename(s, true);
    }

    void onEditNameFinish(string s){
      if(_editPanelProgram == null){
        WWLog.logError("The program trying to edit is null.");
        return;
      }
      if(String.IsNullOrEmpty(s)){
        WWLog.logWarn("edited program name is empty, do nothing");
        return; 
      }
      _editPanelProgram.Program.UserFacingName = trProgram.sanitizeFilename(s);
      if(trDataManager.Instance != null){
        trDataManager.Instance.AppSaveInfo.SaveProgram(_editPanelProgram.Program);
      }
      _editPanelProgram.LabelFilename.text = _editPanelProgram.Program.UserFacingName;
      _editPanel.gameObject.SetActive(false);
    }

    void onClickMyProgram(trVaultExampleMyProgram trVEMP){   
      loadProgram(trVEMP.Program);
    }

    void loadProgram(trProgram program){
      trDataManager.Instance.AppSaveInfo.CurProgram = program;
      trDataManager.Instance.AppSaveInfo.Save();
      trDataManager.Instance.CurrentRobotTypeSelected = program.RobotType;

      loadFreePlay();
    }

    void onClickDeleteMyProgram(trVaultExampleMyProgram trVEMP){
      if(_deletePanel == null){
        _deletePanel = instantiateInHolder(_deletePanelPrefab, _deletePanelHolder);
        _deletePanel.deleteAction = onDLPnlDeleteButtonClicked;
        _deletePanel.cancelAction = onDeletePanelBack;
      }
      _deletePanel.imageSprite = trVEMP.Img.sprite;
      _deletePanel.title = wwLoca.Format("@!@Delete {0}?@!@", trVEMP.Program.UserFacingName);
      uGUIPanelTween.Instance.TweenOpen(_deletePanel.transform);
      _deletePanelProgram = trVEMP;
    }

    void onDLPnlDeleteButtonClicked(){
      if(_deletePanelProgram == null){
        WWLog.logError("The program trying to delete is null.");
        return;
      }

      trProgram nextInLine = null;
      foreach(trProgram trPrg in trDataManager.Instance.AppSaveInfo.Programs){
        if(trPrg != _deletePanelProgram.Program){
          if(trPrg.RobotType == _deletePanelProgram.Program.RobotType){
            nextInLine = trPrg;
            break;
          }
        }
      }

      trDataManager.Instance.AppSaveInfo.RemoveProgram(_deletePanelProgram.Program, nextInLine);
      Destroy(_deletePanelProgram.gameObject);
      _deletePanelProgram = null;
      onDeletePanelBack();
    }

    IEnumerator loadImage(trVaultExampleMyProgram item){
      string filePath = System.Uri.EscapeUriString("file://" + item.Program.ThumbnailPath);
      Texture2D texture = new Texture2D(1, 1);
      WWW www = new WWW(filePath);
      while(!www.isDone){
        yield return null;
      }
      www.LoadImageIntoTexture(texture);
      Sprite image = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
      item.Img.sprite = image;
    }

    void minmax4(float a, float b, float c, float d, out float min, out float max){
      if(a < b){
        min = a;
        max = b;
      } else{
        min = b;
        max = a;
      }
      if(c < min){
        min = c;
      } else if(c > max){
        max = c;
      }
      if(d < min){
        min = d;
      } else if(d > max){
        max = d;
      }
    }

    Rect rectFromWorldCorners(){
      float xMin, xMax, yMin, yMax;
      minmax4(_worldCorners[0].x, _worldCorners[1].x, _worldCorners[2].x, _worldCorners[3].x, out xMin, out xMax);
      minmax4(_worldCorners[0].y, _worldCorners[1].y, _worldCorners[2].y, _worldCorners[3].y, out yMin, out yMax);
      return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    void calculateViewportRect(float xSpeed) {
      _scrollCtrl.viewport.GetWorldCorners(_worldCorners);
      _viewportRect = rectFromWorldCorners();
      float timeMargin = Mathf.Max(Time.deltaTime, maxThumbnailLoadTime);
      if (xSpeed > 0) {
        _viewportRect.xMax += xSpeed * timeMargin;
      } else {
        _viewportRect.xMin += xSpeed * timeMargin;
      }
    }

    Rect itemIntersectionRect(RectTransform rectTransform) {
      rectTransform.GetWorldCorners(_worldCorners);
      Rect rect = rectFromWorldCorners();
      float itemWidth = rect.width;
      rect.xMin -= itemWidth;
      rect.xMax += itemWidth;
      return rect;
    }

    IEnumerator lazyLoad(trVaultExampleMyProgram item, IEnumerator coroutine){
      while(!_viewportRect.Overlaps(itemIntersectionRect(item.transform as RectTransform))) {
        yield return new WaitForEndOfFrame();
      }
      StartCoroutine(coroutine);
    }

    trVaultExampleMyProgram createProgramObj(trProgram program, GameObject list, bool loadLazily = false){
      trVaultExampleMyProgram item;

      GameObject newObj = Instantiate(_programPrefab);
      item = newObj.GetComponent<trVaultExampleMyProgram>();
      newObj.transform.SetParent(list.transform, false);

      item.Program = program;
      item.LabelFilename.text = program.UserFacingName;

      item.BtnMain.onClick.RemoveAllListeners();
      item.BtnMain.onClick.AddListener(() =>{
        onClickMyProgram(item);
      });

      item.BtnRename.onClick.RemoveAllListeners();
      item.BtnRename.onClick.AddListener(() =>{
        onClickRenameMyProgram(item);
      });

      item.BtnDelete.onClick.RemoveAllListeners();
      item.BtnDelete.onClick.AddListener(() =>{
        onClickDeleteMyProgram(item);
      });

      if(program.IsThumbnailExist){
        if(loadLazily){
          StartCoroutine(lazyLoad(item, loadImage(item)));
        } else{
          StartCoroutine(loadImage(item));
        }
      }

      return item;
    }

    protected override void createList(trBrowseTabBase btn, piRobotType type){
      GameObject list = _mainPanel.programListForRobotType(type);
      RectTransform scrolltrans = _mainPanel.scrollTransformForRobotType(type);
      btn.ListTable.Add(type, scrolltrans);    
      if(btn == _topPanel.browseTab){

        foreach(trProgram program in  trDataManager.Instance.AppSaveInfo.Programs){
          if(program.RobotType == type){
            createProgramObj(program, list, (list.transform.childCount >= 6));
          }
        }
      }
      btn.SetList(type);
      _scrollCtrl.content = scrolltrans;
    }
  }

}
