using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

[System.Serializable]
public class wwTabItem {

  [SerializeField]
  private GameObject itemContentPanel;

  [SerializeField]
  private Button activationButton;

  [SerializeField]
  private List<GameObject> contentItems;
  private List<GameObject> filteredList;
  private OnSelectEvent onClickListener;

  public bool SwipeSnap = false;

  public wwTabItem (UnityEngine.GameObject itemContentPanel, UnityEngine.UI.Button activationButton) {
    this.itemContentPanel = itemContentPanel;
    this.activationButton = activationButton;
    contentItems = new List<GameObject>();
  }

  public void SetOnItemSelectedListener(OnSelectEvent listener) {
    this.onClickListener = listener;

    foreach (GameObject item in contentItems) {
      item.GetComponent<wwContentItem>().OnSelectEvent = this.onClickListener;
    }
  }

  public void SetActived(bool isActive) {
    activationButton.interactable = !isActive;
    itemContentPanel.SetActive(isActive);
    var snapper = itemContentPanel.GetComponent<AnimatedSnapper>();

    if (!SwipeSnap) {
      if (Application.isEditor) {
        GameObject.DestroyImmediate(snapper);
      } else {
        GameObject.Destroy(snapper);
      }
    }

    if (isActive) {
      if (snapper == null && SwipeSnap) {
        itemContentPanel.AddComponent<AnimatedSnapper>();
      }

      activationButton.transform.SetSiblingIndex(activationButton.transform.parent.childCount + 1);
    }
  }

  public List<GameObject> ContentItemsList {
    get { return this.contentItems; }
  }

  public GameObject ContentPanel {
    get { return this.itemContentPanel; }
  }

  public Button TabButton {
    get { return this.activationButton; }
  }

  public void ClearContent() {
    foreach (GameObject item in contentItems) {
      item.transform.SetParent(null);
      GameObject.Destroy(item);
    }

    contentItems.Clear();
  }

  public void RefreshChildrenList() {
    contentItems.Clear();
    GameObject panel = _GetContentPanel();

    for (int i = 0; i < panel.transform.childCount; i++) {
      GameObject child = panel.transform.GetChild(i).gameObject;

      if (child.GetComponent<wwContentItem>() != null) {
        contentItems.Add(child);
      }
    }
  }

  public void FilterContent(wwContentItemFilter filter) {
    if (filteredList == null) {
      filteredList = new List<GameObject>(contentItems);
    }

    foreach (GameObject item in filteredList) {
      _RemoveFromParent(item, false);
    }

    filteredList.Clear();

    if (filter == null) {
      filteredList.AddRange(contentItems);
    } else {
      foreach (GameObject item in contentItems) {
        if (filter.IsItemMatchFilterCreateria(item.GetComponent<wwContentItem>())) {
          filteredList.Add(item);
        }
      }
    }

    for (int i = 0; i < filteredList.Count; i++) {
      GameObject item  = filteredList[i];
      _AddContentItemToPanel(item, filteredList);
      _RecalculateContentPanelSize(filteredList);
      float offsetX = _GetContentPanel().GetComponent<RectTransform>().GetSize().x / 2;
      RectTransform trans = item.GetComponent<RectTransform>();
      trans.localPosition = new Vector3(-offsetX + i * trans.GetSize().x, 0, 0);
    }
  }

  public void AddContentItem(GameObject item) {
    wwContentItem script = item.GetComponent<wwContentItem>();

    if (script != null) {
      script.OnSelectEvent = this.onClickListener;
    }

    contentItems.Add(item);
    _RecalculateContentPanelSize(contentItems);
    _AddContentItemToPanel(item, contentItems);
  }

  private void _AddContentItemToPanel(GameObject item, List<GameObject> objects) {
    item.transform.SetParent(_GetContentPanel().transform);
    Vector2 itemSize = this.itemContentPanel.GetComponent<RectTransform>().GetSize();
    itemSize.x = itemSize.y;
    RectTransform trans = item.GetComponent<RectTransform>();
    trans.localPosition = Vector3.zero;
    trans.SetSize(itemSize);
    trans.SetPositionOfPivot(new Vector2(.5f * itemSize.x * objects.Count - itemSize.x, 0));
    trans.SetDefaultScale();
  }

  public void RemoveContentItem(GameObject item) {
    RemoveContentItem(contentItems.IndexOf(item));
  }

  public void RemoveContentItem(int index) {
    if (index > -1 && index < contentItems.Count) {
      _RemoveFromParent(contentItems[index], true);
      contentItems.RemoveAt(index);
    }
  }

  private void _RemoveFromParent(GameObject item, bool destroy) {
    item.transform.SetParent(null);

    if (destroy) {
      GameObject.Destroy(item);
    }

    _RecalculateContentPanelSize(contentItems);
  }

  private GameObject _GetContentPanel() {
    return itemContentPanel.transform.GetChild(0).gameObject;
  }

  private void _RecalculateContentPanelSize(List<GameObject> objects) {
    Vector2 itemSize = this.itemContentPanel.GetComponent<RectTransform>().GetSize();
    RectTransform rectTrans = _GetContentPanel().GetComponent<RectTransform>();
    rectTrans.localPosition = Vector3.zero;
    rectTrans.pivot = new Vector2(.5f, .5f);
    rectTrans.SetSize(new Vector2(itemSize.y * objects.Count, itemSize.y));
    rectTrans.SetPositionOfPivot(new Vector2(itemSize.y * objects.Count / 2 - itemSize.x / 2, 0));
    rectTrans.SetDefaultScale();
    this.itemContentPanel.GetComponent<ScrollRect>().enabled = rectTrans.GetSize().x > itemSize.x;
  }
}

public class wwContentItemFilter {
  public delegate bool MatchCriteria(wwContentItem item);
  public MatchCriteria IsItemMatchFilterCreateria;

  public wwContentItemFilter (MatchCriteria isItemMatchFilterCreateria) {
    this.IsItemMatchFilterCreateria = isItemMatchFilterCreateria;
  }

}