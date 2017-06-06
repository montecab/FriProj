using UnityEngine;
using System.Collections;
using TMPro;

namespace Turing{
  public class trAuthoringLastSavedInfoDisplay : MonoBehaviour {
  	public TextMeshProUGUI Label;

  	// Update is called once per frame
  	void Update () {
  	  Label.text = trDataManager.Instance.AuthoringMissionInfo.LastSavedMissionInfo;
  	}
  }
}
