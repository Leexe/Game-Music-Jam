using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The magic camera. Take a photo of a PhotoSpot to get a flash + a polaroid of the past.
/// </summary>
public class CameraManager : MonoSingleton<CameraManager>
{
	[Header("Flash")]
	[SerializeField]
	private CanvasGroup _flash;

	[SerializeField]
	private float _flashDuration = 0.6f;

	[Header("Polaroid")]
	[SerializeField]
	private CanvasGroup _polaroid;

	[SerializeField]
	private RectTransform _polaroidFrame;

	[SerializeField]
	private Image _polaroidPhoto;

	[SerializeField]
	private TMP_Text _polaroidCaption;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private float _developDuration = 1.5f;

	// Events
	[HideInInspector]
	public Action<PhotoSpot> OnPhotoTaken;
	[HideInInspector]
	public Action OnPolaroidClosed;
	public bool IsBusy { get; private set; }

	private void Start()
	{
		_flash.alpha = 0f;
		_flash.blocksRaycasts = false;

		_polaroid.alpha = 0f;
		_polaroid.blocksRaycasts = false;

		_closeButton.onClick.AddListener(ClosePolaroid);
	}

	/// <summary>
	/// Flash, then show the polaroid of the past for this spot
	/// </summary>
	public void TakePhoto(PhotoSpot spot)
	{
		if (IsBusy)
		{
			return;
		}

		spot.MarkPhotographed();
		OpenPolaroid(spot.PastImage, spot.Caption, true);
		OnPhotoTaken?.Invoke(spot);
	}

	/// <summary>
	/// Shows a polaroid again (no flash), e.g. when clicking it in the inventory
	/// </summary>
	public void ShowPolaroid(Sprite photo, string caption)
	{
		if (IsBusy)
		{
			return;
		}

		OpenPolaroid(photo, caption, false);
	}

	private void OpenPolaroid(Sprite photo, string caption, bool withFlash)
	{
		IsBusy = true;
		Tween.StopAll(_polaroid); // in case it was still closing

		_polaroidPhoto.sprite = photo;
		_polaroidPhoto.color = new Color(1f, 1f, 1f, withFlash ? 0f : 1f); // new photos "develop" in
		if (_polaroidCaption != null)
		{
			_polaroidCaption.text = caption;
		}

		_polaroidFrame.localEulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(-6f, 6f));
		_polaroid.alpha = 1f;
		_polaroid.blocksRaycasts = true;

		Sequence seq = Sequence.Create().Group(Tween.Scale(_polaroidFrame, 0.6f, 1f, 0.4f, Ease.OutBack));

		if (withFlash)
		{
			_flash.alpha = 1f;
			_flash.blocksRaycasts = true;
			seq = seq.Group(Tween.Alpha(_flash, 0f, _flashDuration, Ease.OutQuad))
				.Chain(Tween.Alpha(_polaroidPhoto, 1f, _developDuration, Ease.InOutSine));
		}

		seq.ChainCallback(() =>
		{
			_flash.blocksRaycasts = false;
			IsBusy = false; // now the player can close it
		});
	}

	/// <summary>
	/// Fades the polaroid away
	/// </summary>
	public void ClosePolaroid()
	{
		if (IsBusy)
		{
			return;
		}

		Tween
	.Alpha(_polaroid, 0f, 0.25f)
	.OnComplete(() =>
	{
		_polaroid.blocksRaycasts = false;
		OnPolaroidClosed?.Invoke();
	});
	}
}
