using UnityEngine;
using UnityEngine.UI;

namespace Turing{
  public class trVaultTopPanel : MonoBehaviour{

    [SerializeField]
    private Toggle _robotToggle;

    [SerializeField]
    private GameObject _dashActive;

    [SerializeField]
    private GameObject _dotActive;

    [SerializeField]
    private trBrowseTabBase _browseTab;


    public Toggle robotToggle{
      get {
        return _robotToggle;
      }
    }

    public GameObject dashActive{
      get {
        return _dashActive;
      }
    }

    public GameObject dotActive{
      get {
        return _dotActive;
      }
    }

    public trBrowseTabBase browseTab{
      get {
        return _browseTab;
      }
    }
  }
}
