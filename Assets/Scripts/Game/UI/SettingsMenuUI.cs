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

	private const float TestSfxCooldown = 0.15f;

	private float _previousTimeScale;
	private bool _cursorWasVisible;
	private GameObject _previousSelection;
	private bool _isOpen;
	private bool _returningToMenu;
	private float _lastTestSfxTime;

	private void Awake()
	{
		foreach (VolumeControl control in _volumes)
		{
			AudioManager.AudioBusType channel = control.Channel;
			control.Slider.onValueChanged.AddListener(value => HandleVolumeChanged(channel, value));
		}
	}

	private void HandleVolumeChanged(AudioManager.AudioBusType channel, float value)
	{
		AudioManager.Instance.SetVolume(channel, value);
		if (Time.unscaledTime - _lastTestSfxTime < TestSfxCooldown)
		{
			return;
		}

		_lastTestSfxTime = Time.unscaledTime;
		FMODUnity.EventReference testSfx = channel switch
		{
			AudioManager.AudioBusType.Ambience => FMODEvents.Instance.SettingsTest2_Sfx,
			AudioManager.AudioBusType.Game => FMODEvents.Instance.SettingsTest3_Sfx,
			_ => FMODEvents.Instance.SettingsTest1_Sfx,
		};
		AudioManager.Instance.PlayOneShot(testSfx);
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
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SettingsButton_Sfx);
			CloseMenu();
		}
	}

	public void ReturnToMainMenu()
	{
		if (_returningToMenu)
		{
			return;
		}

		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SettingsButton_Sfx);
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

		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SettingsButton_Sfx);
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
