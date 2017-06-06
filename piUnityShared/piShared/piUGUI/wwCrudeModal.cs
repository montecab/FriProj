using UnityEngine;
using UnityEngine.UI;

public static class wwCrudeModal {
  static Canvas     theOneCanvas;
  static Text       theOneModalText;

  static Canvas TheOneCanvas {
    get {
      if (theOneCanvas == null) {
        GameObject goCV = new GameObject();
        goCV.name = "Modal Dialog Canvas";
        theOneCanvas = goCV.AddComponent<Canvas>();
        Object.DontDestroyOnLoad(goCV);
        theOneCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        theOneCanvas.sortingOrder = 100;

        goCV.AddComponent<GraphicRaycaster>();

        GameObject goPnl = new GameObject();
        RectTransform rtGo = goPnl.AddComponent<RectTransform>();
        Image img = goPnl.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.7f);
        rtGo.SetParent(goCV.transform);
        rtGo.anchorMin = Vector2.zero;
        rtGo.anchorMax = Vector2.one;
        rtGo.offsetMin = Vector2.zero;
        rtGo.offsetMax = Vector2.zero;


        Button btn = goPnl.AddComponent<Button>();
        btn.onClick.AddListener(onClickTheOneModal);

        GameObject goTxt = new GameObject();
        RectTransform rtTxt = goTxt.AddComponent<RectTransform>();
        theOneModalText = goTxt.AddComponent<Text>();
        rtTxt.SetParent(rtGo);
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.offsetMin = Vector2.zero;
        rtTxt.offsetMax = Vector2.zero;

        theOneModalText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        theOneModalText.fontSize = 30;
        theOneModalText.alignment = TextAnchor.MiddleCenter;
        theOneModalText.color = Color.black;
      }

      return theOneCanvas;
    }
  }
  static void onClickTheOneModal() {
    TheOneCanvas.gameObject.SetActive(false);
  }

  public static void showModal(string s) {
    TheOneCanvas.gameObject.SetActive(true);
    theOneModalText.text = s;
  }

}
