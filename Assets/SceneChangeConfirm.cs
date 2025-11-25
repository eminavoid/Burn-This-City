using UnityEngine;
using UnityEngine.Events;

public class SceneChangeConfirm : MonoBehaviour
{
    public static SceneChangeConfirm Instance;

    [SerializeField] private GameObject root;

    private UnityAction onConfirm;
    private UnityAction onCancel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        root.SetActive(false);
    }

    public void Show(UnityAction confirm, UnityAction cancel = null)
    {
        onConfirm = confirm;
        onCancel = cancel;

        root.SetActive(true);
    }

    public void Confirm()
    {
        root.SetActive(false);
        onConfirm?.Invoke();
    }

    public void Cancel()
    {
        root.SetActive(false);
        onCancel?.Invoke();
    }
}
