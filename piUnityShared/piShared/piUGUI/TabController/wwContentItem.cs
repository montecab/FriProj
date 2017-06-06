using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class wwContentItem : MonoBehaviour {

  private Sprite sprite;

  [SerializeField]
  private string contentItemName;

  [SerializeField]
  private List<string> tags;
  private OnSelectEvent localEvent;

  public OnSelectEvent OnSelectEvent {
    get { return this.localEvent; }
    set {
      this.localEvent = value;
      GetComponent<Button>().onClick.AddListener(() => {
        if (localEvent != null) { 
          localEvent.Invoke(this); 
        }
      });
    }
  }

  public void SetupGameObjectRepresentation (Sprite sprite, string name, List<string> tags) {
    this.sprite = sprite;
    this.contentItemName = name;
    this.tags = tags;
    GetComponent<Image>().sprite = this.sprite;
  }

  public static GameObject CreateGameObjectRepresentation() {
    GameObject result = new GameObject("ContentItem");
    result.AddComponent<RectTransform>();
    result.AddComponent<Button>();
    result.AddComponent<Image>();
    result.AddComponent<wwContentItem>();
    result.GetComponent<RectTransform>().pivot = new Vector2(.0f, .5f);
    result.GetComponent<RectTransform>().anchorMin = new Vector2(.0f, .5f);
    result.GetComponent<RectTransform>().anchorMax = new Vector2(.0f, .5f);
    return result;
  }

  public override string ToString () {
    return string.Format ("[wwContentItem: Sprite={0}, Name={1}, Tags={2}]", Sprite, Name, string.Join(",", tags.ToArray()));
  }

  public Sprite Sprite {
    get { return this.sprite; }
  }

  public string Name {
    get {return this.contentItemName; }
  }

  [SerializeField]
  public List<string> Tags {
    get { return tags; }
  }
}

[System.Serializable]
public class OnSelectEvent: UnityEvent<wwContentItem> {}
