using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using WW;
using WW.UGUI;

namespace AnimShop {
  public class asAnimShopController : MonoBehaviour {
    public asTrackController       trackLights;
    public asTrackController       trackSounds;
    public asActionTrackController trackAction;
    public asMenuController        menuController;
    public asMenuController        topButtonPopUp;
    public Button                  buttonPlay;
    public Button                  buttonStop;
    public Transform               playHead;
    public RectTransform           timelineReference;


    public RectTransform           spareParts;
    public GameObject              reticule;
    public asTrackItem             trackItem;

    public DashWidgetController    dashWidget;
    public Button                  buttonTare;

    public asPlayMarker            playMarkerStart;
    public asPlayMarker            playMarkerFinish;
    public RectTransform           playRangeMarker;

    public Button                  buttonBack;

    private       float            playHeadStartTime = float.NaN;
    private const float            animDuration = 10f;

    private       bool             recording = false;
    private       piBotBo          theRobot = null;
    private       bool             prevPressedButtonMain = false;
    private       float            frameInterval;
    private       float            lastFrameTime;
    private       float            recordBeginTime;

    private const string           cPUPPET_KEY = "anim_shop";



    void Start() {
      spareParts    .gameObject.SetActive(false);
      menuController.gameObject.SetActive(false);
      topButtonPopUp.gameObject.SetActive(false);

      updatePlayButtons();
      buttonPlay.onClick.AddListener(onClickPlay);
      buttonStop.onClick.AddListener(onClickStop);
      buttonTare.onClick.AddListener(onClickTare);
      buttonBack.onClick.AddListener(onClickBack);

      playMarkerStart .onDragDelegate += onDragMarkerStart;
      playMarkerFinish.onDragDelegate += onDragMarkerFinish;

      trackLights.ParentController = this;
      trackSounds.ParentController = this;
      trackAction.ParentController = this;

      trackAction.reset();

      piConnectionManager.Instance.OnConnectRobot += onConnectRobot;

      frameInterval = Puppet.recommendedInterval;
      Puppet.init(cPUPPET_KEY);
    }

    void OnDestroy() {
      if (piConnectionManager.Instance != null) {
        piConnectionManager.Instance.OnConnectRobot -= onConnectRobot;
      }
    }

    void Update() {

      dashWidget.update(TheRobot);

      if (Playing) {
        updatePlayHead();
      }
    }

    public float PlayHeadTime {
      get {
        return Time.realtimeSinceStartup - playHeadStartTime;
      }
      private set {
        playHeadStartTime = Time.realtimeSinceStartup - value;
        playHead.position = worldPositionForTime(PlayHeadTime);
        if (Playing && !Recording) {
          float stopTime = timeForWorldPosition(playMarkerFinish.transform.position);
          convertAndBeginAnimation(value, stopTime);
        }
      }
    }

    private void updatePlayHead() {
      float playHeadTime = PlayHeadTime;

      float finish = recording ? animDuration : timeForWorldPosition(playMarkerFinish.transform.position);
      if (finish < playHeadTime) {
          Playing = false;
      }
      else {
        float oldPX = playHead.position.x;
        playHead.position = worldPositionForTime(playHeadTime);
        float newPX = playHead.position.x;

        if (Playing) {
          fireItemsInRange(oldPX, newPX);
        }
      }
    }

    public float timeForWorldPosition(Vector3 pos) {
      float localX = timelineReference.worldToLocalMatrix.MultiplyPoint(pos).x;
      float w   = timelineReference.GetWidth();
      float pxn = localX / w + 0.5f;
      return animDuration * pxn;
    }

    public Vector3 worldPositionForTime(float t) {
      float localX = 0;
      if (!float.IsNaN(t)) {
        float pxn = t / animDuration;
        float w   = timelineReference.GetWidth();
        localX = (pxn - 0.5f) * w;
      }
      Vector3 ret = timelineReference.localToWorldMatrix.MultiplyPoint(Vector3.right * localX);
      return ret;
    }

    public bool Playing {
      get {
        return !float.IsNaN(playHeadStartTime);
      }
      private set {
        if (value) {
          if (recording) {
            PlayHeadTime = 0;
          }
          else {
            PlayHeadTime = timeForWorldPosition(playMarkerStart.transform.position);
          }
        }
        else {
          playHeadStartTime = float.NaN;
          recording = false;
          if (TheRobot != null) {
            TheRobot.cmd_stopSingleAnim();
            TheRobot.cmd_bodyMotionCoast();
          }
        }

        updatePlayHead();
        updatePlayButtons();
        UpdateRangeSliders();
      }
    }

    public bool Recording {
      get {
        return Playing && recording;
      }
      private set {
        if (value == Recording) {
          WWLog.logError("setting Recording redundantly to " + value.ToString());
          return;
        }
        else {
          recording = value;
          Playing = recording;
          if (recording) {
            Puppet.init(cPUPPET_KEY);
            recordBeginTime = Time.realtimeSinceStartup;
            lastFrameTime = float.NaN;
            trackAction.reset();
            topButtonPopUp.gameObject.SetActive(false);
          }
        }
      }
    }

    void updatePlayButtons() {
      buttonPlay.gameObject.SetActive(!Playing);
      buttonStop.gameObject.SetActive( Playing);
      playHead  .gameObject.SetActive( Playing);
    }

    void UpdateRangeSliders() {
      playMarkerStart .GetComponent<Image>().raycastTarget = !Playing;
      playMarkerFinish.GetComponent<Image>().raycastTarget = !Playing;
      playMarkerStart .gameObject.SetActive(!Recording);
      playMarkerFinish.gameObject.SetActive(!Recording);
    }

    void onClickPlay() {
      Playing = true;
    }
    void onClickStop() {
      Playing = false;
    }

    void onClickTare() {
      if (TheRobot != null) {
        TheRobot.cmd_poseParam(0, 0, 0, 0, PI.WWPoseMode.WW_POSE_MODE_SET_GLOBAL, PI.WWPoseDirection.WW_POSE_DIRECTION_INFERRED, PI.WWPoseWrap.WW_POSE_WRAP_ON);
      }
    }

    void onClickBack() {
      Turing.trNavigationRouter.Instance.ShowSceneWithName(Turing.trNavigationRouter.SceneName.LOBBY);
    }

    void fireItemsInRange(float min, float max) {
      fireItemsInRange(min, max, trackLights.itemsContainer);
      fireItemsInRange(min, max, trackSounds.itemsContainer);
    }

    void fireItemsInRange(float min, float max, Transform parent) {
      for (int n = 0; n < parent.childCount; ++n) {
        asTrackItem asTI = parent.GetChild(n).GetComponent<asTrackItem>();
        if ((asTI.transform.position.x >= min) && (asTI.transform.position.x < max)) {
          asTI.fire();
        }
      }
    }

    private piBotBo TheRobot {
      get {
        return theRobot;
      }
      set {
        if (value != theRobot) {
          theRobot = value;
          if (theRobot != null) {
            theRobot.OnState += onRobotState;
          }
        }
        else {
          WWLog.logError("redundantly setting robot to " + theRobot.Name);
        }
      }
    }

    public void showRecordPopUp() {
      if (TheRobot == null) {
        topButtonPopUp.label.text = "Connect a Dash!";
        topButtonPopUp.contextImage.SetActive(false);
      }
      else {
        topButtonPopUp.label.text = "Press the top button\nto start/stop recording.";
        topButtonPopUp.contextImage.SetActive(true);
      }

      topButtonPopUp.gameObject.SetActive(true);
    }

    private void onConnectRobot(piBotBase robot) {
      if (robot.robotType == piRobotType.DASH) {
        TheRobot = (piBotBo)robot;
      }
    }

    private void onRobotState(piBotBase robot) {
      if (robot.robotType != piRobotType.DASH) {
        WWLog.logError("wrong robot type");
        return;
      }

      piBotBo bot = (piBotBo)robot;

      bool pressedButtonMain = (bot.ButtonMain.state == PI.ButtonState.BUTTON_PRESSED);
      bool changedButtonMain = (pressedButtonMain != prevPressedButtonMain);
      if (changedButtonMain) {
        if (!pressedButtonMain) {
          Recording = !Recording;
        }
        prevPressedButtonMain = pressedButtonMain;
      }

      if (Recording) {
        trackAction.handleRobotState(bot, playHead);
        tryRecordFrame(bot);
      }
    }

    private void tryRecordFrame(piBotBo robot) {
      bool recordIt = false;

      float tNow = Time.realtimeSinceStartup;
      float tDif = tNow - lastFrameTime;

      recordIt = recordIt || (float.IsNaN(lastFrameTime));
      recordIt = recordIt || (tDif > frameInterval);

      if (recordIt) {
        float t = tNow - recordBeginTime;

        Puppet.addFrame(cPUPPET_KEY, t, robot);

        lastFrameTime = tNow;
      }
    }

    public void clearMotionRecording() {
      Puppet.init(cPUPPET_KEY);
      trackAction.reset();
    }

    private void convertAndBeginAnimation(float startTime, float stopTime) {
      string jsonAnim = Puppet.subsequence(cPUPPET_KEY, startTime, stopTime);//stopTime);
      if (TheRobot != null) {
        TheRobot.cmd_stopSingleAnim();            // this may bhave no effect.
        TheRobot.cmd_startSingleAnim(jsonAnim);
      }
    }

    void markerFight(GameObject self, GameObject them, bool selfOnLeft) {
      Vector3[] corners = new Vector3[4];

      int selfCorner = selfOnLeft ? 3 : 0;
      int themCorner = selfOnLeft ? 0 : 3;

      self.GetComponent<RectTransform>().GetWorldCorners(corners);
      float selfEdgeX = corners[selfCorner].x;

      them.GetComponent<RectTransform>().GetWorldCorners(corners);
      float themEdgeX = corners[themCorner].x;

      float diff = themEdgeX - selfEdgeX;
      float sign = selfOnLeft ? 1f : -1f;

      if (diff * sign < 0) {
        them.transform.position -= Vector3.right * diff;
      }
    }

    void onDragMarkerStart() {
      markerFight(playMarkerStart.gameObject, playMarkerFinish.gameObject, true);
      updatePlayRangeMarker();
    }

    void onDragMarkerFinish() {
      markerFight(playMarkerFinish.gameObject, playMarkerStart.gameObject, false);
      updatePlayRangeMarker();
    }

    void updatePlayRangeMarker() {
      // this is astonishingly difficult. i must be missing something.
      Vector3 vs = playRangeMarker.parent.worldToLocalMatrix.MultiplyPoint(playMarkerStart .transform.position);
      Vector3 vf = playRangeMarker.parent.worldToLocalMatrix.MultiplyPoint(playMarkerFinish.transform.position);
      playRangeMarker.SetWidth(vf.x - vs.x);
      playRangeMarker.SetLeftTopPosition(new Vector2(vs.x, 0f));
      Vector3 tmp = playRangeMarker.transform.localPosition;
      tmp.y = playRangeMarker.parent.GetComponent<RectTransform>().GetHeight() * 0.5f;
      playRangeMarker.transform.localPosition = tmp;
    }
  }
}
