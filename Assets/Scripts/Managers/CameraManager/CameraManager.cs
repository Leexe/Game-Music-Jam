using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CameraManager : MonoSingleton<CameraManager>
{
	[Header("Flash")]
	[Tooltip("Full-screen WHITE Image with a CanvasGroup.")]
	[SerializeField]
	private CanvasGroup _flash;

	[SerializeField]
	private float _flashDuration = 0.6f;

	[Header("Polaroid")]
	[Tooltip("Full-screen panel holding the polaroid. Needs a CanvasGroup.")]
	[SerializeField]
	private CanvasGroup _polaroid;

	[Tooltip("The white polaroid frame (pops in and tilts).")]
	[SerializeField]
	private RectTransform _polaroidFrame;

	[Tooltip("The photo inside the frame.")]
	[SerializeField]
	private Image _polaroidPhoto;

	[Tooltip("Optional text under the photo.")]
	[SerializeField]
	private TMP_Text _polaroidCaption;

	[Tooltip("Full-screen button on the polaroid panel. Clicking closes the polaroid.")]
	[SerializeField]
	private Button _closeButton;

	[Tooltip("How long the photo takes to 'develop'.")]
	[SerializeField]
	private float _developDuration = 1.5f;

	// Events
	[HideInInspector]
	public Action<PhotoSpot> OnPhotoTaken;

	public bool IsBusy { get; private set; }

	private void Start()
	{
		_flash.alpha = 0f;
		_flash.blocksRaycasts = false;

		_polaroid.alpha = 0f;
		_polaroid.blocksRaycasts = false;

		_closeButton.onClick.AddListener(ClosePolaroid);
	}

	
	// Flash, then show the polaroid of the past for this spot
	
	public void TakePhoto(PhotoSpot spot)
	{
		if (IsBusy)
		{
			return;
		}

		IsBusy = true;
		spot.MarkPhotographed();

		// Fill in the polaroid
		_polaroidPhoto.sprite = spot.PastImage;
		_polaroidPhoto.color = new Color(1f, 1f, 1f, 0f); // starts blank, "develops" in
		if (_polaroidCaption != null)
		{
			_polaroidCaption.text = spot.Caption;
		}

		_polaroidFrame.localEulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(-6f, 6f));

		// Flash goes fully white instantly, polaroid is ready behind it
		_flash.alpha = 1f;
		_flash.blocksRaycasts = true;
		_polaroid.alpha = 1f;
		_polaroid.blocksRaycasts = true;

		Sequence
			.Create()
			.Group(Tween.Alpha(_flash, 0f, _flashDuration, Ease.OutQuad))
			.Group(Tween.Scale(_polaroidFrame, 0.6f, 1f, 0.4f, Ease.OutBack))
			.Chain(Tween.Alpha(_polaroidPhoto, 1f, _developDuration, Ease.InOutSine))
			.ChainCallback(() =>
			{
				_flash.blocksRaycasts = false;
				IsBusy = false; // now the player can close it
			});

		OnPhotoTaken?.Invoke(spot);
	}

	
	/// Fades the polaroid away
	
	public void ClosePolaroid()
	{
		if (IsBusy)
		{
			return;
		}

		Tween
			.Alpha(_polaroid, 0f, 0.25f)
			.OnComplete(() => _polaroid.blocksRaycasts = false);
	}
}
