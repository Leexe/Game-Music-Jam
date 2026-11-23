using PrimeTween;
using UnityEngine;

public class WinScreenUI : MonoBehaviour
{
	[Header("Title")]
	[SerializeField]
	private CanvasGroup _canvasGroup;

	[Header("Fade In Duration")]
	[SerializeField]
	private float _fadeInDuration = 1f;

	private Tween _fadeInTween;

	private void Awake()
	{
		_canvasGroup.interactable = false;
		_canvasGroup.blocksRaycasts = false;
	}

	private void OnEnable()
	{
		GameManager.Instance.OnWin += FadeInScreen;
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnWin -= FadeInScreen;
		}
	}

	private void FadeInScreen()
	{
		_fadeInTween.Stop();
		_fadeInTween = Tween.Alpha(_canvasGroup, 1f, _fadeInDuration);
		_fadeInTween.OnComplete(() =>
		{
			_canvasGroup.interactable = true;
			_canvasGroup.blocksRaycasts = true;
		});
	}
}
