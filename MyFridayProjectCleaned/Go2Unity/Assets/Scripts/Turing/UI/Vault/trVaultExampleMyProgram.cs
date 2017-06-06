using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Turing;
using TMPro;

// just a value-object to contain relevant stuff.

public class trVaultExampleMyProgram : MonoBehaviour {

  public Button          BtnMain;
  public TextMeshProUGUI LabelFilename;
  public Button          BtnRename;
  public Button          BtnDelete;
  public Image           Img;
  public Transform       BG;
  
  public trProgram       Program;
}
