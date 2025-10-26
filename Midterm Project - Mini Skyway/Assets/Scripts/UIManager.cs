using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum UIState
{
	MAIN_MENU, GAME, EDIT, GAME_OVER
}

public class UIManager : MonoBehaviour
{
	[SerializeField] private AirportManager airportManager;
	[Space]
	[SerializeField] private GameObject mainMenuContainer;
	[Space]
	[SerializeField] private GameObject gameContainer;
	[SerializeField] private GameObject planeUIButtonPrefab;
	[SerializeField] private Transform planeUIButtonContainer;
	[SerializeField] private TextMeshProUGUI[] planeButtonUITextList;
	[SerializeField] private TextMeshProUGUI timeCounterText;
	[SerializeField] private TextMeshProUGUI tripCounterText;
	[SerializeField] private PlaneColorDictionary _planeColorDictionary;
	[Space]
	[SerializeField] private GameObject gameOverContainer;
	[SerializeField] private TextMeshProUGUI tripStatText;
	[SerializeField] private TextMeshProUGUI timeStatText;
	[SerializeField] private TextMeshProUGUI airportsStatText;

	public UIState State
	{
		get => _uiState;
		set
		{
			mainMenuContainer.SetActive(value == UIState.MAIN_MENU);
			gameContainer.SetActive(value == UIState.GAME || value == UIState.EDIT);
			gameOverContainer.SetActive(value == UIState.GAME_OVER);

			switch (value)
			{
				case UIState.MAIN_MENU:
					break;
				case UIState.GAME:
					if (_uiState == UIState.MAIN_MENU)
					{
						CurrentTime = 0f;
						CurrentTrips = 0;
						airportManager.StartGame();
					}
					else
					{
						planeButtonUITextList[(int)edittingPlaneColor].text = $"Edit {Enum.GetName(typeof(PlaneColor), edittingPlaneColor)} Path";
					}

					break;
				case UIState.EDIT:
					planeButtonUITextList[(int)edittingPlaneColor].text = $"Editting {Enum.GetName(typeof(PlaneColor), edittingPlaneColor)} Path...";

					break;
				case UIState.GAME_OVER:
					airportsStatText.text = $"Total Airports: {airportManager.AirportList.Count}";

					break;
			}

			_uiState = value;
		}
	}
	public PlaneColorDictionary PlaneColorDictionary => _planeColorDictionary;
	public int CurrentTrips
	{
		get => _currentTrips;
		set
		{
			_currentTrips = value;
			tripStatText.text = $"Total Trips: {_currentTrips}";
			tripCounterText.text = $"Trips: {_currentTrips}";
		}
	}
	public float CurrentTime
	{
		get => _currentTime;
		set
		{
			_currentTime = value;
			string timeString = TimeSpan.FromSeconds(_currentTime).ToString(@"mm\:ss\:fff");
			timeStatText.text = $"Total Time: {timeString}";
			timeCounterText.text = $"Time: {timeString}";
		}
	}

	private UIState _uiState;
	private int _currentTrips;
	private float _currentTime;
	private PlaneColor edittingPlaneColor;

	private void Awake()
	{
		planeButtonUITextList = new TextMeshProUGUI[Enum.GetValues(typeof(PlaneColor)).Length];
	}

	private void Start()
	{
		State = UIState.MAIN_MENU;

		for (int i = 0; i < Enum.GetValues(typeof(PlaneColor)).Length; i++)
		{
			AddPlaneUIButton((PlaneColor)i);
		}
	}

	private void Update()
	{
		if (State == UIState.GAME)
		{
			CurrentTime += Time.deltaTime;
		}
		else if (State == UIState.EDIT)
		{
			if (Input.GetMouseButtonDown(0))
			{
				if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
				{
					if (hit.collider.CompareTag("Airport"))
					{
						airportManager.AirplaneList[(int)edittingPlaneColor].AddAirportToPath(hit.transform.GetComponent<Airport>());
					}
				}
			}
		}
	}

	public void SetUIState(int state)
	{
		State = (UIState)state;
	}

	public void AddPlaneUIButton(PlaneColor planeColor)
	{
		Button planeUIButton = Instantiate(planeUIButtonPrefab, planeUIButtonContainer).GetComponent<Button>();
		planeUIButton.onClick.AddListener(() =>
		{
			if (State == UIState.GAME)
			{
				edittingPlaneColor = planeColor;
				airportManager.AirplaneList[(int)edittingPlaneColor].ClearAiportPath();
				State = UIState.EDIT;
			}
			else if (State == UIState.EDIT && edittingPlaneColor == planeColor)
			{
				State = UIState.GAME;
			}
		});
		TextMeshProUGUI planeButtonText = planeUIButton.GetComponentInChildren<TextMeshProUGUI>();
		planeUIButton.GetComponent<Image>().color = PlaneColorDictionary[planeColor];
		planeButtonText.text = $"Edit {Enum.GetName(typeof(PlaneColor), planeColor)} Path";
		planeButtonUITextList[(int)planeColor] = planeButtonText;
	}
}
