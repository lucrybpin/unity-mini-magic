using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [field: SerializeField] public UIMessagesController UIMessagesController { get; private set; }
    [field: SerializeField] public Button ButtonPassUpkeep { get; private set; }

    public Action<int> OnButtonPassUpkeepClick;

    void Awake()
    {
        ButtonPassUpkeep.onClick.AddListener(() => { OnButtonPassUpkeepClick.Invoke(1); });
    }

    public Task ShowMessage(string message)
    {
        return UIMessagesController.ShowMessage(message);
    }

    public void SetButtonPassUpkeepVisibility(bool isVisible)
    {
        ButtonPassUpkeep.gameObject.SetActive(isVisible);
    }


}
