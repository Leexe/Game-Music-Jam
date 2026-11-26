using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
	[SerializeField] private Button _play;

	public void Play()
	{
		_play.interactable = false;
		SceneManager.LoadSceneAsync("Main");
	}
}
