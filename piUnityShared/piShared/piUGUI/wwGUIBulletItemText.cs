using UnityEngine;
using UnityEngine.UI;
using WW;

namespace WW.UGUI{
  public class wwGUIBulletItemText : MonoBehaviour {

    public Text ContentText;
    public Image Bullet1Image;
    public Image Bullet2Image;
    public int Index;

    void refreshUI(){
      if ((Index % 2) == 1) {
        Bullet1Image.gameObject.SetActive(true);
        Bullet2Image.gameObject.SetActive(false);
      }
      else {
        Bullet1Image.gameObject.SetActive(false);
        Bullet2Image.gameObject.SetActive(true);        
      }
    }

    void Start(){
      refreshUI();
    }
  }
}
