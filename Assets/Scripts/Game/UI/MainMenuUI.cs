using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
	[SerializeField] private Button _play;

	private void Start()
	{
		AudioManager.Instance.PlayMusic(FMODEvents.Instance.MainMenu_Bgm);
	}

	public void Play()
	{
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SettingsButton_Sfx);
		_play.interactable = false;
		SceneManager.LoadSceneAsync("Main");
	}
}
