using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BookmarkTabButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[Title("References")]
	[SerializeField]
	private RectTransform _rectTransform;

	[Tooltip("Button used to detect active state (!interactable) when Linked Page is unassigned.")]
	[SerializeField]
	private Button _button;

	[Tooltip("Optional page GameObject controlled by this tab. When assigned, active state matches its active state.")]
	[SerializeField]
	private GameObject _linkedPage;

	[Title("Pop-Out Settings")]
	[Tooltip("Offset from resting anchored position when hovering an inactive bookmark.")]
	[SerializeField]
	private Vector2 _hoverOffset = new(20f, 0f);

	[Tooltip("Offset from resting anchored position when this bookmark is active.")]
	[SerializeField]
	private Vector2 _activeOffset = new(45f, 0f);

	[Tooltip("Duration of the pop-out animation in seconds.")]
	[SerializeField, Min(0f)]
	private float _tweenDuration = 0.2f;

	[Tooltip("Easing function for the pop-out animation.")]
	[SerializeField]
	private Ease _ease = Ease.OutBack;

	[SerializeField]
	private bool _useUnscaledTime = true;

	private Tween _moveTween;
	private Vector2 _restAnchoredPos;
	private CanvasGroup _linkedCanvasGroup;
	private bool _isHovered;
	private bool _isActive;

	private bool IsTabActive => !_button.interactable
		|| (_linkedPage && _linkedPage.activeInHierarchy && (!_linkedCanvasGroup || _linkedCanvasGroup.blocksRaycasts));

	private Vector2 TargetAnchoredPos =>
		_restAnchoredPos + (_isActive ? _activeOffset : (_isHovered ? _hoverOffset : Vector2.zero));

	private void Reset()
	{
		_rectTransform = GetComponent<RectTransform>();
		_button = GetComponent<Button>();
	}

	private void Awake()
	{
		_restAnchoredPos = _rectTransform.anchoredPosition;
		if (_linkedPage)
		{
			_linkedCanvasGroup = _linkedPage.GetComponent<CanvasGroup>();
		}
	}

	private void OnEnable()
	{
		_isHovered = false;
		_isActive = IsTabActive;
		_rectTransform.anchoredPosition = TargetAnchoredPos;
	}

	private void OnDisable()
	{
		_moveTween.Stop();
		_isHovered = false;
		_rectTransform.anchoredPosition = TargetAnchoredPos;
	}

	private void Update()
	{
		bool active = IsTabActive;
		if (active != _isActive)
		{
			SetTabActive(active);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_isHovered = true;
		if (!_isActive)
		{
			TweenToTarget();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_isHovered = false;
		if (!_isActive)
		{
			TweenToTarget();
		}
	}

	public void SetTabActive(bool isActive)
	{
		_isActive = isActive;
		TweenToTarget();
	}

	private void TweenToTarget()
	{
		_moveTween.Stop();
		_moveTween = Tween.UIAnchoredPosition(
			_rectTransform,
			TargetAnchoredPos,
			_tweenDuration,
			_ease,
			useUnscaledTime: _useUnscaledTime
		);
	}
}
