using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class wwButtonSound : MonoBehaviour {
  public float     delay = 0;
  public AudioClip clip;
  public float     volume = 1f;

  private Button theButton = null;

  void Start() {
    theButton = GetComponent<Button>();
    if (theButton == null) {
      WWLog.logError("No Button Component! shutting down.");
      enabled = false;
    }
    else {
      theButton.onClick.AddListener(onClickTheButton);
    }

    if (volume > 1f) {
      WWLog.logWarn("audio clip has volume > 1: " + gameObject.name + "   " + clip.name);
    }
  }

  void OnDestroy() {
    if (theButton != null) {
      theButton.onClick.RemoveListener(onClickTheButton);
    }
  }

  void onClickTheButton() {
    if (delay == 0f) {
      playTheSoundNow();
    }
    else {
      StartCoroutine(playTheSoundDelayed());
    }
  }

  IEnumerator playTheSoundDelayed() {
    yield return new WaitForSeconds(delay);
    playTheSoundNow();
  }

  void playTheSoundNow() {
    GameObject go = new GameObject ("sound_" +  clip.name);
    go.transform.SetParent(this.transform);
    go.transform.position = Vector3.zero;
    AudioSource source = go.AddComponent<AudioSource>();
    source.clip = clip;
    source.volume = volume;
    source.Play();
    Destroy(go, clip.length);
  }
}

