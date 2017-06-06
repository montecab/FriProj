using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using WW.SimpleJSON;
using WW;


namespace Turing{
  public class trCommunityPanelController : trBrowsePanelBase{
    [SerializeField]
    private GameObject _tabPrefab;

    [SerializeField]
    private GameObject _listPrefab;


    [SerializeField]
    private trCommunityTopPanel _topPanelPrefab;
    [SerializeField]
    private Transform _topPanelHolder;
    private trCommunityTopPanel _topPanel;

    [SerializeField]
    private Transform _mainPanelPrefab;
    [SerializeField]
    private Transform _mainPanelHolder;
    private Transform _mainPanel;

    [SerializeField]
    private trCommunityLeftPanel _leftPanelPrefab;
    [SerializeField]
    private Transform _leftPanelHolder;
    private trCommunityLeftPanel _leftPanel;

    [SerializeField]
    private Button _backButton;

    [SerializeField]
    private GameObject _communityItemPrefab;

    [SerializeField]
    private trSharedItemDetailPanelController _detailPanelCtrlPrefab;
    [SerializeField]
    private Transform _detailPanelCtrlHolder;
    private trSharedItemDetailPanelController _detailPanelCtrl;

    [SerializeField]
    private Sprite _allSprite;

    [SerializeField]
    private Sprite _picksSprite;

    [SerializeField]
    private Sprite _popularSprite;

    private Dictionary<CommunityCategory, trCommunityCategoryButtonController> CategoryTable = new Dictionary<CommunityCategory, trCommunityCategoryButtonController>();
    private Dictionary<trPublishedItem, trCommunityItemController> itemTable = new Dictionary<trPublishedItem, trCommunityItemController>();

    private Dictionary<HTTPManager.RequestInfo, trCommunityItemController> requestTable = new Dictionary<HTTPManager.RequestInfo, trCommunityItemController>();
    private Dictionary<string, string> urlPayloadCache = new Dictionary<string, string>();
    private Dictionary<string, Sprite> urlSpriteCache = new Dictionary<string, Sprite>();

    private ToggleGroup _tabParent;

    private int downloadTicketsRemaining = 0;
    private bool downloadsEnabled = false;
    private const int MAX_SIMULTANEOUS_DOWNLOADS = 8;

    private bool isInBackground = false;

    private CommunityCategory curCategory = CommunityCategory.All;

    private T instantiateInHolder<T>(T prefab, Transform holder) where T : Component {
      T result = Instantiate(prefab);
      result.transform.SetParent(holder, false);
      return result;
    }

    void instantiatePanels(){
      if(_topPanel == null){
        _topPanel = instantiateInHolder(_topPanelPrefab, _topPanelHolder);
        _tabParent = _topPanel.tabToggleGroup;
        _robotToggle = _topPanel.robotButton;
        _dashImg = _topPanel.dashActive;
        _dotImg = _topPanel.dotActive;
      }
      if(_mainPanel == null){
        _mainPanel = instantiateInHolder(_mainPanelPrefab, _mainPanelHolder);
        _scrollCtrl = _mainPanel.GetComponent<ScrollRect>();
      }
      if(_leftPanel == null){
        _leftPanel = instantiateInHolder(_leftPanelPrefab, _leftPanelHolder);
        _robotToggleLeftPanelButton = _leftPanel.robotToggleButton;
      }
    }

    protected override void initView(){
      #if UNITY_EDITOR
      // category label lookups fail if Init() hasn't been called
      // when running Community scene in editor
      trDataManager.Instance.Init();
      #endif
      instantiatePanels();
      base.initView();

      // first, setup callbacks correctly
      _backButton.onClick.AddListener(onBackBtnClicked);

      // now enumerage each category as needed
      foreach(CommunityCategory cat in System.Enum.GetValues(typeof(CommunityCategory))){
        GameObject newCategory = Instantiate(_tabPrefab) as GameObject;
        newCategory.transform.SetParent(_tabParent.transform, false);
        newCategory.GetComponentInChildren<TextMeshProUGUI>().text = CommunityCategoryToLocaString(cat);
        trCommunityCategoryButtonController ctrl = newCategory.GetComponent<trCommunityCategoryButtonController>();
        ctrl.Category = cat;
        ctrl.TabToggle.group = _tabParent;
        CategoryTable.Add(cat, ctrl);
        ctrl.ToggleListener = onToggleCategory;
      }
      CategoryTable[CommunityCategory.Picks].TabToggle.isOn = true;
    }

    private string CommunityCategoryToLocaString(CommunityCategory cat){
      string result = "";
      if(cat == CommunityCategory.All){
        result = wwLoca.Format("@!@All@!@");
      } else if(cat == CommunityCategory.Picks){
        result = wwLoca.Format("@!@Picks@!@");
      } else if(cat == CommunityCategory.Popular){
        result = wwLoca.Format("@!@Popular@!@");
      }
      return result;
    }

    protected override void setRobotType(){
      base.setRobotType();
      CategoryTable[CommunityCategory.Picks].OnToggleChange(true); // it's not triggering if not calling
    }

    void onBackBtnClicked(){
      if(!isInBackground){
        trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.LOBBY);
      }

    }

    protected override void onRobotTypeChange(bool isOn){
      base.onRobotTypeChange(isOn);

      foreach(trCommunityCategoryButtonController ctrl in CategoryTable.Values){
        if(ctrl.TabToggle.isOn){
          //WWLog.logWarn(ctrl.Category.ToString());
          onToggleCategory(true, ctrl);
          break;
        }
      }
      if(_curTab != null){
        onToggleTab(true, _curTab);
      }
      downloadsEnabled = true;
    }

    void onToggleCategory(bool isOn, trCommunityCategoryButtonController btn){
      btn.HideAllLists();
      if(!btn.ListTable.ContainsKey(CurRobotType)){
        createList(btn, CurRobotType);
      } else{
        if(isOn){
          btn.ListTable[CurRobotType].gameObject.SetActive(true);
          _scrollCtrl.content = btn.ListTable[CurRobotType];
        }
      }
      if(isOn){
        curCategory = btn.Category;
        updateCatImage();
      }
      downloadsEnabled = true;
    }

    void updateCatImage(){
      switch(curCategory){
      case CommunityCategory.All:
        _leftPanel.categorySprite = _allSprite;
        break;
      case CommunityCategory.Picks:
        _leftPanel.categorySprite = _picksSprite;
        break;
      case CommunityCategory.Popular:
        _leftPanel.categorySprite = _popularSprite;
        break;
      }
    }

    void FixedUpdate(){
      if((!downloadsEnabled) || (downloadTicketsRemaining < 1)){
        return;
      }

      foreach(trCommunityItemController ctrl in itemTable.Values){
        if(!ctrl.ImageDownloaded && ctrl.RobotType == CurRobotType && ctrl.Category == curCategory){
          downloadTicketsRemaining -= 1;
          string url = trSharingConstants.GetPublishedItemRESTUrl(ctrl.CurItem.IconID);
          if(!applyPayloadCache(url, ctrl)){
            HTTPManager.RequestInfo request = HTTPManager.Instance.downloadUrl(trSharingConstants.GetPublishedItemRESTUrl(ctrl.CurItem.IconID), onDownloadImgPayloadFinished, trSharingConstants.DOWNLOAD_TIMEOUT);
            requestTable.Add(request, ctrl);
            if (downloadTicketsRemaining < 1) {
              return;
            }
          }
        }
      }

      downloadsEnabled = false;
    }

    private void OnEnable(){
      WW.BackButtonController.Instance.AddListener(onBackBtnClicked);
      piConnectionManager.Instance.OnChromeClose += setRobotType;
    }

    private void OnDisable(){
      if(WW.BackButtonController.Instance != null){
        WW.BackButtonController.Instance.RemoveListener(onBackBtnClicked);
      }    
      if(piConnectionManager.Instance != null){
        piConnectionManager.Instance.OnChromeClose -= setRobotType;
      }
    }

    protected override void createList(trBrowseTabBase btn, piRobotType type){
     
      trCommunityCategoryButtonController categoryButton = (trCommunityCategoryButtonController)btn; // always cast
      CommunityCategory cat = categoryButton.Category;
      GameObject newList = Instantiate(_listPrefab) as GameObject;
      newList.transform.SetParent(_scrollCtrl.transform, false);
      CategoryTable[cat].ListTable[CurRobotType] = newList.GetComponent<RectTransform>();
      if(CategoryTable[cat].TabToggle.isOn){
        _scrollCtrl.content = newList.GetComponent<RectTransform>();
        CategoryTable[cat].ListTable[CurRobotType].gameObject.SetActive(true);
      }

      new trTelemetryEvent(trTelemetryEventType.CMNTY_LOAD, true)
        .add(trTelemetryParamType.ROBOT_TYPE, type.ToString())
        .add(trTelemetryParamType.CATEGORY, cat.ToString())
        .emit();

      string lang = wwLocaManager.getSystemTextLanguage();
      switch(cat){
      case CommunityCategory.Picks:
        HTTPManager.Instance.downloadUrl(trSharingConstants.GetListUrl(cat, type, lang), onGetPickListFinished, trSharingConstants.DOWNLOAD_TIMEOUT);
        break;
      case CommunityCategory.All:
        HTTPManager.Instance.downloadUrl(trSharingConstants.GetListUrl(cat, type, lang), onGetAllListFinished, trSharingConstants.DOWNLOAD_TIMEOUT);
        break;
      case CommunityCategory.Popular:
        HTTPManager.Instance.downloadUrl(trSharingConstants.GetListUrl(cat, type, lang), onGetPopularListFinished, trSharingConstants.DOWNLOAD_TIMEOUT);
        break;
      }
      trDataManager.Instance.InternetWarningManager.SetEnableSpinner(true);
    }

    void onGetPopularListFinished(HTTPManager.RequestInfo info){
      onGetListFinished(CommunityCategory.Popular, info);
    }

    void onGetAllListFinished(HTTPManager.RequestInfo info){
      onGetListFinished(CommunityCategory.All, info);
    }

    void onGetPickListFinished(HTTPManager.RequestInfo info){
      onGetListFinished(CommunityCategory.Picks, info);
    }

    void onGetListFinished(CommunityCategory cat, HTTPManager.RequestInfo info){
      if(!info.isDone || this == null){
        return;
      }

      if(info.success){
        JSONNode jsn = JSON.Parse(info.DownloadedText);
        List<trPublishedItem> items = new List<trPublishedItem>();
        foreach(JSONClass jsc in jsn.AsArray){
          if(jsc["type"].Value != "WonderPublishedDetails"){
            continue;
          }
          trPublishedItem newItem = new trPublishedItem();
          newItem.FromJson(jsc["data"].AsObject);
          if(!TMP_TextUtilities.HasUnsupportedCharacters(newItem.Name) &&
           !TMP_TextUtilities.HasUnsupportedCharacters(newItem.Description)){
            items.Add(newItem);
          }
        }
       
        for(int i = 0; i < items.Count; ++i){
          GameObject newItem = Instantiate(_communityItemPrefab) as GameObject;
          trCommunityItemController ctrl = newItem.GetComponent<trCommunityItemController>();
          ctrl.CurItem = items[i];
          ctrl.BtnListener = onItemClick;
          ctrl.Category = curCategory;
          if(items[i].RobotType == piRobotType.UNKNOWN){
            ctrl.RobotType = CurRobotType;
          } else{
            ctrl.RobotType = items[i].RobotType;
          }
          newItem.transform.SetParent(CategoryTable[cat].ListTable[ctrl.RobotType].transform, false);
          itemTable.Add(items[i], ctrl);
          ctrl.ImageSpinner.gameObject.SetActive(true);
        }
        downloadTicketsRemaining += MAX_SIMULTANEOUS_DOWNLOADS;
        downloadsEnabled = true;
      } else{
        trDataManager.Instance.InternetWarningManager.SetEnableWarningDialog(info);
      }
      trDataManager.Instance.InternetWarningManager.SetEnableSpinner(false);
    }

    bool applyPayloadCache(string url, trCommunityItemController ctrl){
      string payloadUrl;
      if (!urlPayloadCache.TryGetValue(url, out payloadUrl)) {
        return false;
      }
      if (!applySpriteCache(payloadUrl, ctrl)) {
        HTTPManager.RequestInfo request = HTTPManager.Instance.downloadUrl(payloadUrl, onDownloadImgFinished, trSharingConstants.DOWNLOAD_TIMEOUT);
        requestTable.Add(request, ctrl);
      }
      return true;
    }

    bool applySpriteCache(string url, trCommunityItemController ctrl){
      Sprite sprite;
      if (!urlSpriteCache.TryGetValue(url, out sprite)) {
        return false;
      }
      ctrl.SetSprite(sprite);
      ctrl.ImageDownloaded = true;
      return true;
    }

    void onDownloadImgPayloadFinished(HTTPManager.RequestInfo info){
      if(this == null || !info.isDone){
        return;
      }
     
      if(info.success){
        FilePayload payload = new FilePayload();
        JSONNode js = JSON.Parse(info.DownloadedText);
        payload.FromJson(js.AsObject);
        urlPayloadCache[info.url] = payload.Url;
        if(requestTable.ContainsKey(info)){
          trCommunityItemController ctrl = requestTable[info];
          if (!applySpriteCache(payload.Url, ctrl)) {
            HTTPManager.RequestInfo request = HTTPManager.Instance.downloadUrl(payload.Url, onDownloadImgFinished, trSharingConstants.DOWNLOAD_TIMEOUT);
            requestTable.Add(request, ctrl);
          }
        }
      } else{
        WWLog.logWarn("Payload downloading failed: " + info.ToString());
        trDataManager.Instance.InternetWarningManager.SetEnableSpinner(false);
        requestTable[info].SetFailureSprite();
        requestTable[info].ImageDownloaded = true;
        downloadsEnabled = true;
        downloadTicketsRemaining += 1;
      }
      requestTable.Remove(info);
    }


    void onDownloadImgFinished(HTTPManager.RequestInfo info){
      if(this == null || !info.isDone){
        return;
      }

      // todo: it seems like every executable line in the next 25 lines or so is predicated on requestTable.ContainsKey(info),
      // so it seems like there's room for logic simplification.

      bool loadedTexture = false;

      if(info.success){       
        if(requestTable.ContainsKey(info)){
          Texture2D texture = new Texture2D(1, 1);
          if (!texture.LoadImage(info.request.downloadHandler.data)) {
            WWLog.logError("unable to parse image data. " + info.url);
          }
          else {
            loadedTexture = true;
            Sprite image = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            requestTable[info].SetSprite(image);
            urlSpriteCache[info.url] = image;
          }
        }
      }

      if (!loadedTexture){
        if(requestTable.ContainsKey(info)){
          requestTable[info].SetFailureSprite();
        }
      }

      if(requestTable.ContainsKey(info)){
        requestTable[info].ImageDownloaded = true;
        requestTable.Remove(info);
      }
      downloadsEnabled = true;
      downloadTicketsRemaining += 1;
    }

    //    //TODO: needs refactoring to HTTPManager
    //    IEnumerator downloadImg (string url, trCommunityItemController ctrl)
    //    {
    //      Texture2D texture = new Texture2D(1,1);
    //      WWW www = new WWW(url);
    //      yield return www;
    //      www.LoadImageIntoTexture(texture);
    //      ctrl.ImageSpinner.gameObject.SetActive(false);
    //      Sprite image = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    //      ctrl.Img.sprite = image;
    //      downloadNextImage = true;
    //    }

    void onItemClick(trCommunityItemController item){
      if(isInBackground){
        return;
      }

      new trTelemetryEvent(trTelemetryEventType.CMNTY_ITEM, true)
        .add(trTelemetryParamType.ROBOT_TYPE, item.RobotType.ToString())
        .add(trTelemetryParamType.CATEGORY, item.Category.ToString())
        .add(trTelemetryParamType.FILE_ID, item.CurItem.ID)
        .emit();

      isInBackground = true;
      if (_detailPanelCtrl == null) {
        _detailPanelCtrl = instantiateInHolder(_detailPanelCtrlPrefab, _detailPanelCtrlHolder);
      }
      _detailPanelCtrl.DownloadAndShowPublishedItem(item.CurItem);
      _detailPanelCtrl.BackButtonListener = () =>{
        isInBackground = false;
      };
    }

  }
}

