using UnityEngine;
using UnityEngine.EventSystems;

public class SetButtonFocus : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
