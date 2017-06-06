using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Turing;

public class trChallengeListController : MonoBehaviour {

  [SerializeField]
  private Button _backButton;
	[SerializeField]
	private TextMeshProUGUI _areaName;
  [SerializeField]
  private TextMeshProUGUI _completeText;
  [SerializeField]
  private TextMeshProUGUI _progressText;
  [SerializeField]
  private Image _areaBGImage;
  [SerializeField]
  private Image _robotImage;
  [SerializeField]
  private Sprite _dashImage;
  [SerializeField]
  private Sprite _dotImage;

  [Header("Do not change the order")]
  [SerializeField]
  private List<Sprite> _areaImages;

  public void SetupView(trMissionAreaButtonController ctrl){
    _areaName.text = ctrl.areaName;
    _completeText.text = wwLoca.Format("@!@Challenges completed@!@");
    _progressText.text = string.Format("{0}/{1}", ctrl.completedMissions, ctrl.Missions.Count);
    _areaBGImage.sprite = _areaImages[(int)ctrl.areaType];
    piRobotType robotType = ctrl.Missions[0].Behavior.MissionFileInfo.Type.GetRobotType();
    _robotImage.sprite = (robotType==piRobotType.DASH)?_dashImage:_dotImage;
    _backButton.onClick.AddListener(()=>{this.gameObject.SetActive(false);});
  }

}
