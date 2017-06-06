using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Turing{
  public class trCommunityTopPanel : MonoBehaviour{

    [SerializeField]
    private ToggleGroup _tabToggleGroup;

    [SerializeField]
    private Toggle _robotButton;

    [SerializeField]
    private GameObject _dashActive;

    [SerializeField]
    private GameObject _dotActive;

    public ToggleGroup tabToggleGroup{
      get{
        return _tabToggleGroup;
      }
    }

    public Toggle robotButton{
      get{
        return _robotButton;
      }
    }

    public GameObject dashActive{
      get{
        return _dashActive;
      }
    }

    public GameObject dotActive{
      get{
        return _dotActive;
      }
    }
  }
}
