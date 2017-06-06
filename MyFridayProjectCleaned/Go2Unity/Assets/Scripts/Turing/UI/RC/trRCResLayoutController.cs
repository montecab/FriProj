using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using WW.UGUI;

namespace Turing{
  public class trRCResLayoutController : MonoBehaviour {
    public RectTransform SoundPanel;
    public GridLayoutGroup UserSoundPanel;
    public GridLayoutGroup OnRobotSoundPanel;

    public RectTransform BGMid;
    public RectTransform BGLeft;
    public RectTransform BGFull;

    public uGUISegmentedController TabsCtrl;

      // Use this for initialization
    void Start () {
      fixBGLayout();
      TabsCtrl.Refresh();
    }

    void fixBGLayout(){
      float width = BGLeft.GetWidth();
      BGMid.offsetMin = new Vector2(width, 0);
      BGMid.offsetMax = new Vector2(-width, 0);
    }

    void fixSoundPanelLayout(){
      float height = SoundPanel.GetHeight();
      Vector2 cellsize = UserSoundPanel.cellSize;
      Vector2 spacing = UserSoundPanel.spacing;
      float cellheight = (height/(spacing.y + cellsize.y * 2)) * cellsize.y;
      float spacingY = (height/(spacing.y + cellsize.y * 2)) * spacing.y;
      UserSoundPanel.cellSize = new Vector2(cellheight, cellheight);
      UserSoundPanel.spacing = new Vector2(spacingY, spacingY);
      OnRobotSoundPanel.cellSize = new Vector2(cellheight, cellheight);
      OnRobotSoundPanel.spacing = new Vector2(0, spacingY);
    }
  }
}

