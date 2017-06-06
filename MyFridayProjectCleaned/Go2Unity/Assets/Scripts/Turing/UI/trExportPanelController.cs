using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Turing;
using System;
using WW.SaveLoad;
using TMPro;

public class trExportPanelController : MonoBehaviour {
  public Button          buttonFirmwareRequirements;
  public Text            labelFirmwareRequirements;
  public Button          buttonExport;
  public Text            labelExport;
  public Button          buttonAndRun;
  public Button          buttonTransferAnims;
  public Toggle          toggleAnimCacheBust;
  public TextMeshProUGUI labelAnimTransfer;
  public Image           imageSpinner;
  public Text            labelSrcPath;
  public InputField      inputSrcPath;
  public InputField      inputDestDir;
  public InputField      inputDestName;
  public InputField      inputDestExt;
  
  private piBotCommon theBot                  = null;
  private bool        disconnectAfterTransfer = false;
  private string      localPath               = "";
  private string      destDir                 = sharedConstants.TOK_SPKU;
  private string      destName                = "MAIN";
  private string      destExt                 = sharedConstants.TOK_ROBOT_SUFFIX_SPARK;
  private string      validatedLocalPath      = null;
  private bool        animTransferInProgress  = false;
  
  private bool allowAndRun = false;
  
  public void Start() {
    inputDestDir .interactable = false;
    inputSrcPath .text = localPath;
    inputDestDir .text = destDir;
    inputDestName.text = destName;
    inputDestExt .text = destExt;
  }
  
  public void Update() {
    // todo: disable transfer if there is a shell-command in flight.
    buttonAndRun.gameObject.SetActive(!TransferInProgress && allowAndRun);
    imageSpinner.gameObject.SetActive( TransferInProgress);
    labelExport.text = (TransferInProgress ? (piConnectionManager.Instance.BotInterface.fileTransferProgress(theBot.UUID) * 100.0f).ToString("000") : "Transfer");

    validateSettings();    
  }
  
  public void onClickExport() {
    if (!validateSettings()) {
      return;
    }
    
    disconnectAfterTransfer = false;
    StartCoroutine(exportAndTransfer());
  }

  public void onClickAndRun() {
    if (!validateSettings()) {
      return;
    }
    disconnectAfterTransfer = true;
    StartCoroutine(exportAndTransfer());
  }
  
  public void onClickTransferAnims() {
    if (animTransferInProgress) {
      animTransferInProgress = false;
    }
    else {
      piBotBase bot = ABot;
      if (bot == null) {
        labelAnimTransfer.text = "No Robot!";
      }
      else {
        StartCoroutine(transferAllAnimFiles((piBotCommon)bot));
      }
    }
  }
  
  
  // todo: abstract this up a bit so it's not specific to UI etc.
  private IEnumerator transferAllAnimFiles(piBotCommon robot) {
    // identify all animations
    
    buttonTransferAnims.GetComponentInChildren<Text>().text = "Stop";
    animTransferInProgress = true;
    List<trMoodyAnimation> anims = trMoodyAnimations.Instance.GetAllAnimations();
    
    int   files = 0;
    int   bytes = 0;
    float t0    = Time.time;
    
    foreach(trMoodyAnimation trMA in anims) {
      uint baseId = trMA.id;
      if (!animTransferInProgress) {
        break;
      }
      
      for (int n = 0; n < 7; ++n) {
        if (!animTransferInProgress) {
          break;
        }
        string prefix = "A";
        string anFNBase = prefix + baseId.ToString() + "_" + n.ToString();
        // note: secretly unity checks for files of type ".bytes"
        string anPath = sharedConstants.anFilePrefix + anFNBase;
        
        TextAsset ta = Resources.Load<TextAsset>(anPath);
        
        if (ta != null) {
          byte[] contents = ta.bytes;
          
          bool cacheBust = toggleAnimCacheBust.isOn;
          if (cacheBust) {
            if (wwDoOncePerTypeVal<string>.doIt("cache busting file xfer")) {
              WWLog.logError("cache-busting file transfer by destroying file contents!!");
            }
            // cache-bust. this makes the anim useless as an anim, but good as a file.
            contents[0] = (byte)UnityEngine.Random.Range(0, 255);
          }
          
          string robotPath = sharedConstants.TOK_SYST + anFNBase + sharedConstants.TOK_ROBOT_SUFFIX_ANIM;
          if (robotPath.Length > 16) {
            WWLog.logError("invalid robot animation filename: " + robotPath);
            break;
          }
          
          WWLog.logInfo("transferring " + contents.Length + " bytes : "  + anFNBase + " from " + anPath);
          piConnectionManager.Instance.BotInterface.fileTransfer(robot.UUID, contents, robotPath, WWRobotFileTransferFileType.FILE_TYPE_ANIM);
          
          files += 1;
          bytes += contents.Length;
          
          do
          {
            float progress = piConnectionManager.Instance.BotInterface.fileTransferProgress(robot.UUID);
            labelAnimTransfer.text = anFNBase + " : " + ((int)(progress * 100)).ToString("000");
            yield return null;
          } while (animTransferInProgress && (piConnectionManager.Instance.BotInterface.fileTransferProgress(robot.UUID) < 1));
          
          WWLog.logInfo("transferred " + anFNBase + "..");
          
          yield return new WaitForSeconds(0.2f);
        }
      }
    }
    
    float t1 = Time.time;
    
    buttonTransferAnims.GetComponentInChildren<Text>().text = "XFer all anims";
    labelAnimTransfer.text = "files:" + files + " bytes:" + bytes + " t:" + (int)(t1 - t0) + "s";
    animTransferInProgress = false;
    
    yield return null;
  }
  
  public void onClickPlayAnim() {
    byte[] cmd = wwBA.toByteArray1((byte)(bluetoothCommand_t.COMMAND_TYPE_PLAY_ANIMATION_BY_NAME));
    byte[] tmp = wwBA.toByteArray(destDir + destName);
    byte[] fn = new byte[sharedConstants.filePathMaxChars];
    for (int n = 0; n < fn.Length; ++n) {
      fn[n] = (n < tmp.Length ? tmp[n] : (byte)0);
    }
    byte[] btCmd = wwBA.append(fn, cmd);
    
    if (ABot != null) {
      ABot.cmd_sendRawData(btCmd);
    }
  }
  
  private bool TransferInProgress {
    get {
      return (theBot != null);
    }
  }
  
  private string RobotFilePath {
    get {
      latchDestValues();
      return destDir + destName + destExt;
    }
  }
  
  private WWRobotFileTransferFileType FileType {
    get {
      WWRobotFileTransferFileType ret = WWRobotFileTransferFileType.FILE_TYPE_NONE;
      switch (destExt) {
        default:
          WWLog.logError("unhandled file extension: " + destExt);
          ret = WWRobotFileTransferFileType.FILE_TYPE_NONE;
          break;
        case sharedConstants.TOK_ROBOT_SUFFIX_SPARK:
          ret = WWRobotFileTransferFileType.FILE_TYPE_SPARK;
          break;
        case sharedConstants.TOK_ROBOT_SUFFIX_ANIM:
          ret = WWRobotFileTransferFileType.FILE_TYPE_ANIM;
          break;
        case "WA":
          ret = WWRobotFileTransferFileType.FILE_TYPE_AUDIO;
          break;
      }
      
      return ret;
    }
  }
  
  
  private piBotCommon ABot {
    get {
      if (theBot == null || theBot.connectionState != PI.BotConnectionState.CONNECTED) {
        return (piBotCommon)piConnectionManager.Instance.AnyConnectedBo;
      }
      else {
        return theBot;
      }
    }
  }
   
  public bool AllowedToStartTransfer {
    get {
      if (ABot == null) {
        return false;
      }
      
      if (TransferInProgress) {
        return false;
      }

      return (ABot.pendingShellCommandCount == 0);
    }
  }
  
  private void latchDestValues() {
    localPath = inputSrcPath.text;
    destDir   = inputDestDir .text.ToUpper();
    destName  = inputDestName.text.ToUpper();
    destExt   = inputDestExt .text.ToUpper();
  }
  
  private bool validateSettings() {
    latchDestValues();
    
    Dictionary<string, string> extensionFolders = new Dictionary<string, string> {
      {sharedConstants.TOK_ROBOT_SUFFIX_SPARK, sharedConstants.TOK_SPKU},
      {sharedConstants.TOK_ROBOT_SUFFIX_ANIM , sharedConstants.TOK_SYST},
      {"WA", sharedConstants.TOK_SYST},
      };
    
    Color cYes = new Color(0.7f, 1.0f, 0.7f);
    Color cBad = new Color(1.0f, 0.6f, 0.6f);
    
    bool allGood = true;
    inputDestDir .image.color = cYes;
    inputDestName.image.color = cYes;
    inputDestExt .image.color = cYes;
    
    if (localPath == "") {
      inputDestExt.text = sharedConstants.TOK_ROBOT_SUFFIX_SPARK;
      inputDestExt.interactable = false;
      latchDestValues();
    }
    else {
      inputDestExt.interactable = true;
    }
    
    if (!extensionFolders.ContainsKey(destExt)) {
      allGood = false;
      inputDestExt.image.color = cBad;
    }
    else {
      inputDestDir.text = extensionFolders[destExt];
      latchDestValues();
    }
    
    if ((destName.Length > sharedConstants.fileBodyMaxChars) || (destName.Length == 0)) {
      allGood = false;
      inputDestName.image.color = cBad;
    }
    
    if (localPath != validatedLocalPath) {
      inputSrcPath .image.color = cYes;
      if (localPath == "") {
        byte[] export = GetContent(ABot);
        if (export == null) {
          labelSrcPath.text = "export error";
        }
        else {
          labelSrcPath.text = "" + GetContent(ABot).Length + " bytes";
        }
      }
      else {
        string fs = WW.SaveLoad.wwDataSaveLoadManagerStatic.Load(localPath, true);
        if (fs == null) {
          allGood = false;
          inputSrcPath.image.color = cBad;
          labelSrcPath.text = "bad file.";
        }
        else {
          byte[] fbytes = System.IO.File.ReadAllBytes(localPath);
          labelSrcPath.text = "" + fbytes.Length + " bytes";
        }
      }
      validatedLocalPath = localPath;
    }
    
    allGood = allGood && AllowedToStartTransfer;

    buttonExport       .enabled = allGood;
    buttonAndRun       .enabled = allGood;
    buttonTransferAnims.enabled = allGood;
    buttonExport       .GetComponent<Image>().color = Color.white * (allGood ? 1 : 0.7f);
    buttonAndRun       .GetComponent<Image>().color = Color.white * (allGood ? 1 : 0.7f);
    buttonTransferAnims.GetComponent<Image>().color = Color.white * (allGood ? 1 : 0.7f);
    
    return allGood;
  }
  
  private void showFirmwareRequirements() {
    string s = "";
    s += "please check your robot has firmware version:\n\n" + trToFirmware.cFirmwareRequired;
    s += "\n\n";
    s += "(this message is displayed no matter what firmware version the robot actually has. you may be all good.)";
    labelFirmwareRequirements.text = s;
    buttonFirmwareRequirements.gameObject.SetActive(true);
  }
  
  public void onClickFirmwareRequirements() {
    buttonFirmwareRequirements.gameObject.SetActive(false);
  }
  
  private trProgram getProgramToTransfer() {
    if (inputSrcPath.text == "") {
      return trDataManager.Instance.GetCurProgram();
    }
    else {
      return null;
    }
  }
  
  private byte[] GetContent(piBotBase robot) {
    byte[] ret = new byte[0];
    
    if (robot == null) {
      return null;
    }
    
    trProgram trPrg = getProgramToTransfer();
    
    if (trPrg != null) {
      ret = new trToFirmware().toFirmware(trPrg, robot.robotType);
    }
    else {
      string s = wwDataSaveLoadManagerStatic.Load(inputSrcPath.text);
      if (s == null) {
        return null;
      }
      ret = System.IO.File.ReadAllBytes(inputSrcPath.text);
    }
    
    return ret;
  }

  private IEnumerator exportAndTransfer() {
    
    if (wwDoOncePerTypeVal<string>.doIt("show firmware requirement message")) {
      showFirmwareRequirements();
      yield break;
    }
    
    
    theBot = ABot;
    if (theBot == null) {
      WWLog.logError("no robot.");
      yield break;
    }
    
    byte[] contents = GetContent(theBot);
    
    if (contents == null) {
      // error is already printed.
      yield break;
    }
    
    piConnectionManager.Instance.BotInterface.fileTransfer(theBot.UUID, contents, RobotFilePath, FileType);
    
    do
    {
      yield return null;
    } while ( theBot != null && piConnectionManager.Instance.BotInterface.fileTransferProgress(theBot.UUID) < 1);
    
    if (disconnectAfterTransfer) {
      piConnectionManager.Instance.BotInterface.disconnectRobot(theBot.UUID);
    }
    
    theBot = null;
  }
}
