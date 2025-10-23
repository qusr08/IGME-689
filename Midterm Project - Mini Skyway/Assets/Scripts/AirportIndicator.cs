using UnityEngine;

public class AirportIndicator : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private GameObject contentContainer;
    [Space]
    [SerializeField] private Airport _target;
    [SerializeField] private bool _isEnabled;

    public Airport Target
    {
        get => _target;
        set
        {
            _target = value;
            _target.Indicator = this;
        }
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            contentContainer.SetActive(value);
        }
    }

    private void Awake()
    {
        mainCamera = FindFirstObjectByType<Camera>();
        canvas = FindFirstObjectByType<Canvas>();
        rectTransform = GetComponent<RectTransform>();

        IsEnabled = false;
    }

    private void Update()
    {
        if (Target == null)
        {
            return;
        }

        if (Target.IsRendererVisible && !IsEnabled)
        {
            IsEnabled = true;
        }
        else if (!Target.IsRendererVisible && IsEnabled)
        {
            IsEnabled = false;
        }

        if (IsEnabled)
        {
            rectTransform.anchoredPosition = mainCamera.WorldToScreenPoint(Target.transform.position) / canvas.scaleFactor;
        }
    }
}
