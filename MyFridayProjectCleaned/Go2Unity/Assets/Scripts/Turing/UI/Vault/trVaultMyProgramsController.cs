using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;
using Turing;
using WW.SimpleJSON;

namespace Turing {
  public class trVaultMyProgramsController : MonoBehaviour {
    
    public trVaultExampleMyProgram MostRecentMyProgram;  // also the example My Program.
    public Transform MyProgramsList;
    public GameObject MyProgramsContainer;

    public Button InternalProgramsButton;
    public Transform InternalProgramsList;
    public GameObject InternalProgramsContainer;
    public TextMeshProUGUI InternalProgramsGroupLabel;
    public trVaultExampleChallenge InternalProgramItem;
    
    public trVaultPopUpController PopUpPanel;
    public trAppSaveInfo AppSaveInfo;

    public GameObject FutureProgramModal;
    
    /*  
    My programs text capsule : 96, 92, 168 56% opacity
    Last programs text capsule: 105, 98, 218 50% opacity
    */
    static Color clrMyList = wwColorUtil.newColor( 96,  92, 168, 0.56f);
    static Color clrMyRcnt = wwColorUtil.newColor(105,  98, 218, 0.56f);
    
    void Start () {
      Setup();
    }

    void Setup() {    
      // pressing the "My programs" title will actually trigger a button!
      InternalProgramsButton.onClick.AddListener(showInternalPrograms);
      
      // new trTelemetryEvent(trTelemetryEventType.APPNAV_VAULT, true)
      //   .add(trTelemetryParamType.ROBOT_TYPE, trDataManager.Instance.CurrentRobotTypeSelected.ToString())
      //   .emit();
    }

    public void ShowPanel() {
      populateWithMyPrograms();
      gameObject.SetActive(true);
    }

    #region myPrograms  
    void populateWithMyPrograms() {
      // first, clear out all existing programs, if any    
      piUnityUtils.destroyAllChildren(MyProgramsList);
      InternalProgramsContainer.SetActive(false);
      MyProgramsContainer.SetActive(true);
    
      // now load all the known programs
      AppSaveInfo.Programs.Sort((trProgram first, trProgram second) => {
        return second.RecentLoadedTime.CompareTo(first.RecentLoadedTime);
      });

      bool firstProgram = true;
      foreach(trProgram program in AppSaveInfo.Programs){
        if (program.RobotType == trDataManager.Instance.CurrentRobotTypeSelected) {
          trVaultExampleMyProgram item;
          if (firstProgram) {
            firstProgram = false;
            item = MostRecentMyProgram;
            item.BtnMain.image.color = clrMyRcnt;
          }
          else {
            item = GameObject.Instantiate<trVaultExampleMyProgram>(MostRecentMyProgram);
            item.transform.SetParent(MyProgramsList);
            item.transform.localScale = Vector3.one;
            item.BtnMain.image.color = clrMyList;
          }
          
          item.Program            = program;
          item.LabelFilename.text = program.UserFacingName;
          
          item.BtnMain      .onClick.RemoveAllListeners();
          item.BtnMain      .onClick.AddListener(() => {
            onClickMyProgram(item);
          });
          
          item.BtnRename    .onClick.RemoveAllListeners();
          item.BtnRename    .onClick.AddListener(() => {
            onClickRenameMyProgram(item);
          });
          
          item.BtnDelete    .onClick.RemoveAllListeners();
          item.BtnDelete    .onClick.AddListener(() => {
            onClickDeleteMyProgram(item);
          });
        }
      }
    }
    
    private trVaultExampleMyProgram currentMyProgram;
    private trProgram previewedProgram;
    
    void previewMyProgram(trVaultExampleMyProgram trVEMP, string bottomText, Action<string> callback) {
      previewMyProgram(trVEMP.Program, bottomText);
      PopUpPanel.ShowEditInput(trVEMP.Program.UserFacingName, callback);
    }   

    void previewMyProgram(trVaultExampleMyProgram trVEMP, string bottomText, Action callback) {
      previewMyProgram(trVEMP.Program, bottomText, callback);
    }

    void previewMyProgram(trProgram trPrg, string bottomText, Action callback) {
      previewMyProgram(trPrg, bottomText);
      PopUpPanel.ShowCopyYes(false, callback);
    }

    void previewMyProgram(trProgram trPrg, string bottomText) {
      previewedProgram = trPrg;

      // load the full program
      AppSaveInfo.initializeProgram(previewedProgram);

      if(previewedProgram.IsFutureVersion){
        FutureProgramModal.SetActive(true);
        return;
      }
      
      PopUpPanel.Title.text = previewedProgram.UserFacingName;
      PopUpPanel.ShowProgram(previewedProgram);
      if (bottomText != null) {
        PopUpPanel.ShowCallToActionText(bottomText);
      }
      PopUpPanel.ShowPanel();
    }
    
    void onClickMyProgram(trVaultExampleMyProgram trVEMP) {   
      previewMyProgram(trVEMP, "Load this program ?", onClickConfirmLoadProgram);
    }
    
    void onClickConfirmLoadProgram() {
      trProgram trPRG = previewedProgram;
      AppSaveInfo.CurProgram = trPRG;
      AppSaveInfo.Save();
      
      loadFreePlay();
    }
    
    void onClickDeleteMyProgram(trVaultExampleMyProgram trVEMP) {
      previewMyProgram(trVEMP, "Delete this program ?", onClickConfirmDeleteProgram);
    }
    
    void onClickConfirmDeleteProgram() {
      trProgram nextInLine = null;
      foreach(trProgram trPrg in AppSaveInfo.Programs) {
        if (trPrg != previewedProgram) {
          if (trPrg.RobotType == previewedProgram.RobotType) {
            nextInLine = trPrg;
            break;
          }
        }
      }
      
      AppSaveInfo.RemoveProgram(previewedProgram, nextInLine);
      populateWithMyPrograms();
    }
    
    void onClickRenameMyProgram(trVaultExampleMyProgram trVEMP) {
      previewMyProgram(trVEMP, null, onClickConfirmRenameProgram); // null string => input
    }
    
    void onClickConfirmRenameProgram(string newFileName) {
      string s = trProgram.sanitizeFilename(newFileName);   
      if (string.IsNullOrEmpty(s)) {
        return;
      }
      
      previewedProgram.UserFacingName = s;
      AppSaveInfo.SaveProgram(previewedProgram);
      AppSaveInfo.Save();
      populateWithMyPrograms();
    }
    
    #endregion 

    #region showInternalPrograms
    void showInternalPrograms() {
      if (gameObject.activeSelf && trMultivariate.isYESorSHOW(trMultivariate.trAppOption.VAULT_ALLOW_INTERNAL)) {        
        if (InternalProgramsContainer.activeSelf) {     
          populateWithMyPrograms();
        }
        else {
          populateWithInternalPrograms();
        }
      }
    }

    void populateWithInternalPrograms() {
      JSONArray jsa = loadInternalProgramsIndexJson();
      if (jsa != null) {
        // loaded the internal programs index, now toggle UI appropriately
        MyProgramsContainer.SetActive(false);
        InternalProgramsContainer.SetActive(true);
        // clear out all existing internal programs, if any
        for (int n = InternalProgramsList.childCount - 1; n >= 0; --n) {
          Transform child = InternalProgramsList.GetChild(n);
          if ((child != InternalProgramsGroupLabel.transform) && (child != InternalProgramItem.transform)) {
            GameObject.Destroy(child.gameObject);
          }
        }
        // now recursively populate with the internal items
        _recurseJson(jsa, populateWithInternalItems, "");            
      }
    }

    JSONArray loadInternalProgramsIndexJson() {
      GenerateManifest(); // as needed
      JSONArray jsa = null;

      TextAsset ta = Resources.Load<TextAsset>(InternalManifestPath);
      if (ta == null) {
        WWLog.logError("could not load: " + InternalManifestPath);
      }
      else {
        jsa = JSON.Parse(ta.text).AsArray;
        if (jsa == null) {
          WWLog.logError("could not parse json: " + InternalManifestPath);
        }
      }
      return jsa;
    }

    void GenerateManifest() {
      #if UNITY_EDITOR

      string path = System.IO.Path.Combine(Application.dataPath, "Resources/" + InternalProgramsPath);
      JSONArray jsa = wwFileUtil.getFileList(path);
      string mp = System.IO.Path.Combine(Application.dataPath, "Resources/" + InternalManifestPath + ".txt");
      System.IO.File.WriteAllText(mp, jsa.ToString(""));

      // reimport
      string ap = "Assets/Resources/" + InternalManifestPath + ".txt";
      UnityEditor.AssetDatabase.ImportAsset(ap);

      WWLog.logInfo("regenerated \"" + ap + "\"");

      #endif
    }

    void populateWithInternalItems(string name, bool isDirectory, bool hasFileChildren, string path) {         
      if (isDirectory) {
        // if a directory has no actual files in it, no need to show it
        if (hasFileChildren) {
          string labelText = path + name;
          TextMeshProUGUI label = GameObject.Instantiate<TextMeshProUGUI>(InternalProgramsGroupLabel);
          label.gameObject.SetActive(true);
          label.text = labelText;
          label.transform.SetParent(InternalProgramsList);
          label.transform.localScale = Vector3.one;
        }
      }
      else {
        // it's a file
        // do we care ?
        if (System.IO.Path.GetExtension(name) != ".txt") {
          return;
        }
        if (name == "manifest.txt") {
          return;
        }
          
        string filePath = path + System.IO.Path.GetFileNameWithoutExtension(name);
        trProgram trPrg = getProgramForPath(filePath);
        if (trPrg != null) {
          if (trPrg.RobotType == trDataManager.Instance.CurrentRobotTypeSelected) {
            trVaultExampleChallenge chlng = GameObject.Instantiate<trVaultExampleChallenge>(InternalProgramItem);
            chlng.gameObject.SetActive(true);
            chlng.TxtName.text = trPrg.UserFacingName;
            
            string lambdaCapture = filePath;
            chlng.BtnMain.onClick.AddListener(() => {
              onClickInternalItem(lambdaCapture);
            });
            chlng.transform.SetParent(InternalProgramsList);
            chlng.transform.localScale = Vector3.one;

            chlng.ImgLockedTotally.gameObject.SetActive(false);
            chlng.ImgLockedLoose  .gameObject.SetActive(false);
            chlng.ImgOldNew       .gameObject.SetActive(false);
            chlng.ImgNewNew       .gameObject.SetActive(false);
            chlng.BtnInfo         .gameObject.SetActive(false);
          }
        }
      }
    }

    private const string InternalProgramsPath = "TuringProto/Internal";
    private const string InternalManifestPath = InternalProgramsPath + "/" + "manifest";
    private static Dictionary<string, trProgram> internalPrograms = null;
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
    private const string cTokPermanentDictList = "TRVAULTCONTROLLER_INTERNAL_PROGRAMS";
    private trProgram getProgramForPath(string path) {
      if (internalPrograms == null) {
        internalPrograms = trDataManager.Instance.getPermanentItem<Dictionary<string, trProgram>>(cTokPermanentDictList);
      }

      if (!internalPrograms.ContainsKey(path)) {
        string resourcePath = InternalProgramsPath + "/" + path;
        TextAsset ta = Resources.Load<TextAsset>(resourcePath);
        if (ta == null) {
          WWLog.logError("could not load resource: " + resourcePath);
          return null;
        }

        string s = ta.text;
        JSONNode jsn = JSON.Parse(s);
        if (jsn == null) {
          WWLog.logError("could not parse file as json: " + path);
          return null;
        }

        trProgram trPrg = trFactory.FromJson<trProgram>(jsn);
        if (trPrg == null) {
          WWLog.logError("could not parse json as program: " + path);
          return null;
        }

        internalPrograms[path] = trPrg;
      }

      return internalPrograms[path];
    }
    
    private void onClickInternalItem(string path) {
      trProgram trPrg = getProgramForPath(path);

      // not testing program for null or correct robot type because if it got into the list then we've already done that.
      previewMyProgram(trPrg, "Load It ?", onClickConfirmLoadProgram);
    }

    #endregion

    private void loadFreePlay() {
      trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.MAIN, trProtoController.RunMode.FreePlay.ToString());
    }
 }   
}
