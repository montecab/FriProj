using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Turing;

public class elementInfoPanelTestController : MonoBehaviour {
  public trElementInfoPanelController elementInfoPanel;
  public Button                       exampleButton;

  private const int                   maxNum = 6;

  void Start() {

    trDataManager.Instance.Init();

    for (int n = 1; n < maxNum; ++n) {
      Button btn = GameObject.Instantiate<Button>(exampleButton);
      btn.transform.SetParent(exampleButton.transform.parent);
      btn.transform.localScale = Vector3.one;
    }

    for (int n = 0; n < maxNum; ++n) {
      Button btn = exampleButton.transform.parent.GetChild(n).GetComponent<Button>();
      btn.GetComponentInChildren<Text>().text = (n + 1).ToString();

      int lambdaCapture = n + 1;
      btn.onClick.AddListener(() => {
        onClickNum(lambdaCapture);
      });
    }

    onClickNum(1);
  }

  private trBehaviorType[] itemsBehavior = new trBehaviorType[] {
    trBehaviorType.HEAD_PAN,
    trBehaviorType.HEAD_PAN_VOICE,
    trBehaviorType.COLOR_RED,
    trBehaviorType.MOODY_ANIMATION,
    trBehaviorType.MOVE_DISC_TURN,
    trBehaviorType.MOVE_CONT_SPIN,
    trBehaviorType.SOUND_ANIMAL,
    trBehaviorType.SOUND_USER,
  };

  private trTriggerType[] itemsTrigger = new trTriggerType[] {
    trTriggerType.DISTANCE_SET,
    trTriggerType.CLAP,
    trTriggerType.TIME,
    trTriggerType.RANDOM,
    trTriggerType.BEHAVIOR_FINISHED,
  };

  trState makeState() {
    trBehavior trB = new trBehavior(itemsBehavior[Random.Range(0, itemsBehavior.Length)]);
    trState trS = new trState();
    trS.Behavior = trB;

    if (trB.Type == trBehaviorType.MOODY_ANIMATION) {
      trS.SetBehaviorParameterValue(0, trMoodyAnimations.Instance.GetAllAnimations()[1].id);
      trS.Mood = Random.value > 0.5 ? trMoodType.FRUSTRATED : trMoodType.SILLY;
    }

    return trS;
  }

  void onClickNum(int num) {
    elementInfoPanel.gameObject.SetActive(true);

    elementInfoPanel.clear();

    trElementInfo trEI;

    if (num == 1) {
      trEI = new trElementInfo(makeState());
      elementInfoPanel.addNewItem(trEI);
    }
    else {
      for (int n = 0; n < Mathf.Min(num, itemsTrigger.Length); ++n) {
        trTransition trTrn = new trTransition();
        trTrn.Trigger = new trTrigger(itemsTrigger[n]);
        trTrn.StateSource = makeState();
        trEI = new trElementInfo(trTrn);
        elementInfoPanel.addNewItem(trEI);
      }
    }
  }
}
