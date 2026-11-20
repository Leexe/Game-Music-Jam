using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Win condition: photograph (collect) every PhotoSpot in the scene.
/// Shows the win screen after the last polaroid is closed.
/// </summary>
public class WinManager : MonoBehaviour
{
	[Tooltip("Full-screen win panel with a CanvasGroup.")]
	[SerializeField]
	private CanvasGroup _winScreen;

	[Tooltip("Optional. Restarts the scene.")]
	[SerializeField]
	private Button _playAgainButton;

	[SerializeField]
	private float _fadeDuration = 1f;

	private readonly HashSet<PhotoSpot> _collected = new HashSet<PhotoSpot>();
	private int _total;
	private bool _hasWon;

	public int Total => _total;

	public int Collected => _collected.Count;

	private void Start()
	{
		// Count every item, including ones in rooms that are currently hidden
		_total = FindObjectsByType<PhotoSpot>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;

		_winScreen.alpha = 0f;
		_winScreen.blocksRaycasts = false;

		if (_playAgainButton != null)
		{
			_playAgainButton.onClick.AddListener(PlayAgain);
		}

		CameraManager.Instance.OnPhotoTaken += HandlePhotoTaken;
		CameraManager.Instance.OnPolaroidClosed += CheckForWin;
	}

	private void OnDestroy()
	{
		if (CameraManager.Instance != null)
		{
			CameraManager.Instance.OnPhotoTaken -= HandlePhotoTaken;
			CameraManager.Instance.OnPolaroidClosed -= CheckForWin;
		}
	}

	private void HandlePhotoTaken(PhotoSpot spot)
	{
		_collected.Add(spot);
		Debug.Log($"Items collected: {Collected}/{_total}");
	}

	private void CheckForWin()
	{
		if (_hasWon || _total == 0 || Collected < _total)
		{
			return;
		}

		_hasWon = true;
		_winScreen.blocksRaycasts = true;
		Tween.Alpha(_winScreen, 1f, _fadeDuration);
	}

	private void PlayAgain()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
}
