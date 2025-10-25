using TMPro;
using UnityEngine;

public enum UIState
{
	MAIN_MENU, GAME, GAME_OVER
}

public class UIManager : MonoBehaviour
{
	[SerializeField] private AirportManager airportManager;
	[Space]
	[SerializeField] private GameObject mainMenuContainer;
	[Space]
	[SerializeField] private GameObject gameContainer;
	[SerializeField] private TextMeshProUGUI tripCounterText;
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
			_uiState = value;
			mainMenuContainer.SetActive(_uiState == UIState.MAIN_MENU);
			gameContainer.SetActive(_uiState == UIState.GAME);
			gameOverContainer.SetActive(_uiState == UIState.GAME_OVER);
		}
	}

	private UIState _uiState;

	private void Start()
	{
		State = UIState.MAIN_MENU;
	}
}
