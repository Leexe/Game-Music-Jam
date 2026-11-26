using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
	[Serializable]
	private struct VolumeControl
	{
		public AudioManager.AudioBusType Channel;
		public Slider Slider;
	}

	[SerializeField]
	private VolumeControl[] _volumes;

	[SerializeField]
	private Button _resume;

	[SerializeField]
	private GameObject _settingsPanel;

	private float _previousTimeScale;
	private bool _cursorWasVisible;
	private GameObject _previousSelection;
	private bool _isOpen;
	private bool _returningToMenu;

	private void Awake()
	{
		foreach (VolumeControl control in _volumes)
		{
			control.Slider.onValueChanged.AddListener(value => AudioManager.Instance.SetVolume(control.Channel, value));
		}
	}

	private void OnEnable()
	{
		InputManager.Instance.OnEscapePerformed += ToggleUI;
		if (_settingsPanel.activeSelf)
		{
			OpenMenu();
		}
	}

	private void OnDisable()
	{
		if (InputManager.Instance)
		{
			InputManager.Instance.OnEscapePerformed -= ToggleUI;
		}
		CloseMenu();
	}

	public void Resume()
	{
		if (!_returningToMenu)
		{
			CloseMenu();
		}
	}

	public void ReturnToMainMenu()
	{
		if (_returningToMenu)
		{
			return;
		}

		_returningToMenu = true;
		AudioManager.Instance.StopAllEvents();
		SceneManager.LoadSceneAsync("MainMenu");
	}

	public void ToggleUI()
	{
		if (_returningToMenu)
		{
			return;
		}

		if (_settingsPanel.activeSelf)
		{
			CloseMenu();
		}
		else
		{
			OpenMenu();
		}
	}

	private void OpenMenu()
	{
		if (_isOpen)
		{
			return;
		}

		_isOpen = true;
		_previousTimeScale = Time.timeScale;
		_cursorWasVisible = Cursor.visible;
		_previousSelection = EventSystem.current.currentSelectedGameObject;
		Time.timeScale = 0f;
		Cursor.visible = true;

		_settingsPanel.SetActive(true);
		foreach (VolumeControl control in _volumes)
		{
			control.Slider.SetValueWithoutNotify(AudioManager.Instance.GetVolume(control.Channel));
		}
		_resume.Select();
	}

	private void CloseMenu()
	{
		if (!_isOpen)
		{
			return;
		}

		_isOpen = false;
		_settingsPanel.SetActive(false);
		Time.timeScale = _previousTimeScale;
		Cursor.visible = _returningToMenu || _cursorWasVisible;
		if (AudioManager.Instance)
		{
			AudioManager.Instance.SaveAudioPref();
			PlayerPrefs.Save();
		}
		if (!_returningToMenu && EventSystem.current && _previousSelection)
		{
			EventSystem.current.SetSelectedGameObject(_previousSelection);
		}
	}
}
