// from https://forum.unity3d.com/threads/tab-between-input-fields.263779/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Tab Navigator for UI
// Single instance of this script per GUI
// An alternative would be to use a next/previous setting on a single GUI item, which would mean one script per InputField - not ideal

public class wwTabNavigation : MonoBehaviour
{
  private EventSystem system;

  private void Start() {
    system = EventSystem.current;
  }

  private void Update() {
    if (system.currentSelectedGameObject == null || !Input.GetKeyDown (KeyCode.Tab)) {
      return;
    }

    Selectable current = system.currentSelectedGameObject.GetComponent<Selectable>();
    if (current == null) {
      return;
    }

    bool reverse = Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift);

    Selectable next = reverse ? findPrev<InputField>(current) : findNext<InputField>(current);

    if (next == null) {
      return;
    }

    // Simulate Inputfield MouseClick
    InputField inputfield = next.GetComponent<InputField>();
    if (inputfield != null) {
      inputfield.OnPointerClick(new PointerEventData(system));
    }

    // Select the next item in the taborder of our direction
    system.SetSelectedGameObject(next.gameObject);
  }

  Selectable findRight<T>(Selectable cur) {
    while (cur != null) {
      cur = cur.FindSelectableOnRight();
      if (cur is T) {
        break;
      }
    }
    return cur;
  }

  Selectable findLeft<T>(Selectable cur) {
    while (cur != null) {
      cur = cur.FindSelectableOnLeft();
      if (cur is T) {
        break;
      }
    }
    return cur;
  }

  Selectable findUp<T>(Selectable cur) {
    while (cur != null) {
      cur = cur.FindSelectableOnUp();
      if (cur is T) {
        break;
      }
    }
    return cur;
  }

  Selectable findDown<T>(Selectable cur) {
    while (cur != null) {
      cur = cur.FindSelectableOnDown();
      if (cur is T) {
        break;
      }
    }
    return cur;
  }

  Selectable findPrev<T>(Selectable current) {
    // horizontal first
    Selectable ret;

    ret = findLeft<T>(current);
    if (ret != null) {
      return ret;
    }

    ret = findUp<T>(current);
    if (ret == null) {
      return null;
    }

    Selectable tmp = findRight<T>(ret);
    while (tmp != null) {
      ret = tmp;
      tmp = findRight<T>(tmp);
    }

    return ret;
  }

  Selectable findNext<T>(Selectable current) {
    // horizontal first
    Selectable ret;

    ret = findRight<T>(current);
    if (ret != null) {
      return ret;
    }

    ret = findDown<T>(current);
    if (ret == null) {
      return null;
    }

    Selectable tmp = findLeft<T>(ret);
    while (tmp != null) {
      ret = tmp;
      tmp = findLeft<T>(tmp);
    }

    return ret;
  }
}