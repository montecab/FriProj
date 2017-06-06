using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Turing{
  public class trCommunityLeftPanel : MonoBehaviour{
    [SerializeField]
    private wwToggleableButton _robotToggleButton;

    [SerializeField]
    private Image _categoryImage;

    public wwToggleableButton robotToggleButton{
      get{
        return _robotToggleButton;
      }
    }

    public Sprite categorySprite{
      set{
        _categoryImage.sprite = value;
      }
    }
  }
}
