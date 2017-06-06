using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using WW;

namespace AnimShop {
  public class asActionTrackController : asTrackController {

    private void enableDashWidget(Transform item, bool enable) {
      GameObject childrenContainer = (item.childCount > 0 ? item.GetChild(0).gameObject : null);
      if (childrenContainer != null) {
        childrenContainer.SetActive(enable);
      }
    }

    public void reset() {
      for (int n = 0; n < itemsContainer.childCount; ++n) {
        Transform item = itemsContainer.GetChild(n);
        enableDashWidget(item, false);
      }
    }

    public void handleRobotState(piBotBo robot, Transform playhead) {
      // find the widget the playhead is in, if any
      for (int n = 0; n < itemsContainer.childCount; ++n) {
        Transform item = itemsContainer.GetChild(n);

        RectTransform rt = item.GetComponent<RectTransform>();
        Vector3[] worldCorners = new Vector3[4];
        rt.GetWorldCorners(worldCorners);
        if (playhead.position.x > worldCorners[1].x) {
          if (playhead.position.x < (worldCorners[1].x + worldCorners[2].x) / 2f) {
            enableDashWidget(item, true);
            DashWidgetController dwc = item.GetComponentInChildren<DashWidgetController>();
            if (dwc != null) {
              dwc.update(robot);
            }
          }
        }
      }
    }

    protected override void onButtonClearTrack() {
      parentController.clearMotionRecording();
    }
  }
}
