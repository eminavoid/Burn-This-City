using UnityEngine;

public class UIToggle : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    
        public void Toggle()
        {
            panel.SetActive(!panel.activeSelf);
        }
}
