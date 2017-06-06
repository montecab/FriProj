using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using WW.SimpleJSON;
using UnityEngine.Events;

public class wwTabControl : MonoBehaviour {

  [SerializeField]
  private List<wwTabItem> tabItems = new List<wwTabItem>();

  [SerializeField]
  private GameObject tabsPanel;

  [SerializeField]
  private GameObject containerPanel;

  [SerializeField]
  private int selectedTabIndex = 0;

  [SerializeField]
  public OnSelectEvent OnSelectedEvent;

  [SerializeField]
  public bool SwipeSnap = false;

  [SerializeField]
  public Color DefaultTabColor = new Color(76.0f / 255.0f, 85.0f / 255.0f, 93.0f / 255.0f);

  [SerializeField]
  public ColorBlock TabButtonTintColor = GetDefaultTabButtonTintColorBlock();

  [SerializeField]
  public Sprite TabButtonMaskSprite = _LoadAsset("Sprites/button_rounded");

  [SerializeField]
  public int TabButtonSpacing = 0;

  public TextAsset SourceJsonFile = null;
  private wwContentItemFilter activeFilter = null;

  public GameObject PanelWithTabs {
    get { return this.tabsPanel;}
  }

  public GameObject PanelWithContent {
    get { return this.containerPanel; }
  }

  public List<wwTabItem> TabsObjects {
    get { return this.tabItems; }
  }

  public static GameObject CreateDefaultTabControl() {
    GameObject result = new GameObject("TabControl");
    result.AddComponent<RectTransform>();
    result.AddComponent<ContentSizeFitter>();
    result.AddComponent<wwTabControl>();
    result.AddComponent<Image>();
    result.AddComponent<VerticalLayoutGroup>();
    result.GetComponent<Image>().color = Color.clear;
    result.GetComponent<RectTransform>().SetSize(new Vector2(600, 210));
    result.GetComponent<RectTransform>().SetPositionOfPivot(Vector3.zero);
    GameObject tabsPanel = new GameObject("Tabs");
    tabsPanel.AddComponent<Image>();
    tabsPanel.AddComponent<LayoutElement>();
    tabsPanel.GetComponent<LayoutElement>().preferredHeight = 100;
    tabsPanel.transform.SetParent(result.transform, false);
    tabsPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0);
    GameObject content = new GameObject("ContentPanels");
    content.AddComponent<LayoutElement>();
    content.GetComponent<LayoutElement>().preferredHeight = 200;
    content.transform.SetParent(result.transform, false);
    wwTabControl tabcontrol = result.GetComponent<wwTabControl>();
    tabcontrol.tabsPanel = tabsPanel;
    tabcontrol.containerPanel = content;
    return result;
  }

  public static GameObject CreateDefaultContentItem() {
    GameObject result = new GameObject("ScrollRect");
    result.AddComponent<RectTransform>();
    result.AddComponent<CanvasRenderer>();
    result.AddComponent<Image>();
    result.AddComponent<Mask>();
    result.AddComponent<ScrollRect>();
    GameObject content = new GameObject("Content");
    content.AddComponent<RectTransform>();
    content.transform.SetParent(result.transform);
    RectTransform transform = content.GetComponent<RectTransform>();
    transform.anchorMin = new Vector2(.0f, 1.0f);
    transform.anchorMax = new Vector2(.0f, 1.0f);
    ScrollRect scrollRect = result.GetComponent<ScrollRect>();
    scrollRect.content = content.GetComponent<RectTransform>();
    scrollRect.vertical = false;
    return result;
  }

  public static GameObject CreateDefaultTabButton() {
    GameObject result = new GameObject("TabButton");
    result.AddComponent<RectTransform>();
    result.AddComponent<CanvasRenderer>();
    result.AddComponent<Image>();
    result.AddComponent<Mask>();
    result.AddComponent<Button>();
    RectTransform transformRect = result.GetComponent<RectTransform>();
    transformRect.anchorMin = new Vector2(.0f, 1.0f);
    transformRect.anchorMax = new Vector2(.0f, 1.0f);
    transformRect.pivot = new Vector2(.0f, .5f);
    GameObject image = new GameObject("icon");
    image.AddComponent<RectTransform>();
    image.AddComponent<CanvasRenderer>();
    image.AddComponent<Image>();
    transformRect = image.GetComponent<RectTransform>();
    transformRect.anchorMin = new Vector2(.0f, .0f);
    transformRect.anchorMax = new Vector2(1.0f, 1.0f);
    transformRect.pivot = new Vector2(.0f, .5f);
    image.transform.SetParent(result.transform);
    return result;
  }

  public void ApplyFilter(wwContentItemFilter filter) {
    foreach (wwTabItem tabItem in tabItems) {
      tabItem.FilterContent(filter);
    }

    activeFilter = filter;
  }

  public void RegenerateControl(TextAsset sourceAsset = null, bool clear = true) {
    if (sourceAsset == null) {
      sourceAsset = SourceJsonFile;
    }

    if (clear) {
      foreach (wwTabItem tabItem in tabItems) {
        if (tabItem.TabButton != null) {
          tabItem.TabButton.transform.SetParent(null);
          Object.Destroy(tabItem.TabButton.gameObject);
        }

        if (tabItem.ContentPanel != null) {
          tabItem.ContentPanel.transform.SetParent(null);
          Object.Destroy(tabItem.ContentPanel);
        }

        tabItem.ClearContent();
      }
    }

    tabItems.Clear();
    var config = JSON.Parse(sourceAsset.text);
    int counter = 0;

    foreach (JSONClass categoria in config["categories"].AsArray) {
      var iconSprite = Resources.Load<Sprite>(categoria["icon"].Value);
      wwTabItem item = CreateDefaultTabItem(iconSprite, counter, DefaultTabColor);
      tabItems.Add(item);

      foreach (JSONClass action in categoria["actions"].AsArray) {
        List<string> tags = new List<string>();

        foreach (JSONNode node in action["tags"].AsArray) {
          tags.Add(node.Value);
        }

        GameObject contentItem = wwContentItem.CreateGameObjectRepresentation();
        contentItem.GetComponent<wwContentItem>().SetupGameObjectRepresentation(
          Resources.Load<Sprite>(action["icon"].Value),
          action["name"].Value,
          tags);
        item.AddContentItem(contentItem);
      }

      counter++;
    }

    if (selectedTabIndex < 0 || selectedTabIndex >= tabItems.Count) {
      selectedTabIndex = 0;
    }

    this.Start();
  }

  private static Sprite _LoadAsset(string name) {
#if UNITY_EDITOR
    return null;
#else
    return Resources.Load<Sprite>(name);
#endif
  }

  private static ColorBlock GetDefaultTabButtonTintColorBlock() {
    ColorBlock result = ColorBlock.defaultColorBlock;
    Color baseColor = result.normalColor;
    baseColor.a = .4f;
    result.normalColor = baseColor;
    baseColor.a = .6f;
    result.pressedColor = baseColor;
    result.highlightedColor = Color.white;
    result.disabledColor = Color.white;
    return result;
  }

  public wwTabItem CreateDefaultTabItem(Sprite tabIcon, int number) {
    return CreateDefaultTabItem(tabIcon, number, DefaultTabColor);
  }

  public wwTabItem CreateDefaultTabItem(Sprite tabIcon, int number, Color defaultColor) {
    GameObject tabObject = wwTabControl.CreateDefaultTabButton();
    wwTabItem result = new wwTabItem(wwTabControl.CreateDefaultContentItem(), tabObject.GetComponent<Button>());
    float tabButtonSize = 70;
    result.ContentPanel.transform.SetParent(containerPanel.transform, false);
    result.ContentPanel.GetComponent<RectTransform>().SetSize(new Vector2(600, 140));
    result.ContentPanel.GetComponent<Image>().color = defaultColor;
    result.TabButton.transform.SetParent(tabsPanel.transform, false);
    result.TabButton.GetComponent<RectTransform>().SetSize(new Vector2(tabButtonSize, tabButtonSize));
    result.TabButton.GetComponent<RectTransform>().SetPositionOfPivot(new Vector2((tabButtonSize + TabButtonSpacing) * number - 300 , 0));
    result.TabButton.colors = TabButtonTintColor;
    tabObject.GetComponentInParent<Image>().sprite = TabButtonMaskSprite;
    tabObject.GetComponentInParent<Image>().color = defaultColor;
    tabObject.transform.Find("icon").GetComponent<Image>().sprite = tabIcon;

    if (tabIcon == null) {
      tabObject.transform.Find("icon").GetComponent<Image>().color = Color.clear;
    }

    return result;
  }


  void Start () {
    foreach (wwTabItem item in tabItems) {
      item.RefreshChildrenList();
      _AddOnTabClickListener(item);
      item.SetOnItemSelectedListener(OnSelectedEvent);
      item.SwipeSnap = SwipeSnap;
    }

    SelectTabAtIndex(selectedTabIndex);
    ApplyFilter(activeFilter);
  }

  private void _AddOnTabClickListener(wwTabItem item) {
    item.TabButton.onClick.AddListener(() => _SelectTab(item));
  }

  private void _SelectTab(wwTabItem tabItem) {
    if (tabItem == null || !tabItems.Contains(tabItem)) { 
      return; 
    }

    foreach (wwTabItem item in tabItems) {
      item.SetActived(tabItem == item);
    }

    selectedTabIndex = tabItems.IndexOf(tabItem);
  }

  public void SelectTabAtIndex(int index) {
    if (index > -1 && index < tabItems.Count) {
      _SelectTab(tabItems[index]);
    }
  }

  public void AddTabItem(wwTabItem item) {
    tabItems.Add(item);
  }
}