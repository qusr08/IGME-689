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

	private bool isEnabledFlag;

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
		isEnabledFlag = false;
	}

	private void Update()
	{
		if (Target == null)
		{
			return;
		}

		isEnabledFlag = false;
		if (Target.IsRendererVisible && (Target.ConnectedAirplanes.Count == 0 || Target.PassengerContainer.IsAtCapacity))
		{
			isEnabledFlag = true;
		}

		if (!IsEnabled && isEnabledFlag)
		{
			IsEnabled = true;
		}
		else if (IsEnabled && !isEnabledFlag)
		{
			IsEnabled = false;
		}

		if (IsEnabled)
		{
			rectTransform.anchoredPosition = mainCamera.WorldToScreenPoint(Target.transform.position) / canvas.scaleFactor;
		}
	}
}
