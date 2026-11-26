using System;
using System.Text;
using PrimeTween;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NotebookUI : MonoBehaviour
{
	[Serializable]
	private struct TabPage
	{
		public Button Tab;
		public GameObject Page;
		public CanvasGroup CanvasGroup;

		[Tooltip("Activates the input field in this page when selected")]
		public TMP_InputField InputField;
	}

	[Title("Tabs")]
	[SerializeField]
	private TabPage[] _tabs;

	[SerializeField, Min(0)]
	private int _defaultTabIndex;

	[Title("Docking")]
	[SerializeField]
	private RectTransform _bottomBar;

	[SerializeField]
	private DraggablePanelUI _dragHandle;

	[Title("Interview")]
	[SerializeField]
	private RectTransform _interviewPosition;

	[SerializeField, Min(0f)]
	private float _moveDuration = 0.35f;

	[SerializeField]
	private Ease _moveEase = Ease.OutCubic;

	[Title("Page Fading")]
	[SerializeField, Min(0f)]
	private float _pageFadeDuration = 0.2f;

	[SerializeField]
	private Ease _pageFadeEase = Ease.OutQuad;

	[SerializeField]
	private bool _useUnscaledTime = true;

	[Title("Call Log")]
	[SerializeField]
	private ScrollRect _callLogScroll;

	[SerializeField]
	private TMP_Text _transcript;

	private const string CallTagIdentifier = "call";

	private readonly StringBuilder _callLog = new();
	private UnityAction[] _tabHandlers;
	private Sequence _fadeOutSequence;
	private Tween _fadeInTween;
	private int _activeTabIndex = -1;
	private string _lastContact;
	private bool _scrollToBottomPending;
	private DialogueManager _dialogue;

	private void Awake()
	{
		_tabHandlers = new UnityAction[_tabs.Length];
		for (int i = 0; i < _tabs.Length; i++)
		{
			int tabIndex = i;
			_tabHandlers[i] = () => ShowTab(tabIndex);
		}
	}

	private void Start()
	{
		if (_activeTabIndex < 0)
		{
			ShowTab(_defaultTabIndex);
		}
	}

	private void OnEnable()
	{
		if (!_dialogue)
		{
			_dialogue = DialogueManager.Instance;
			_dialogue.RegisterTagHandler(CallTagIdentifier, HandleCallTag);
			_dialogue.OnDialogueEnded += EndCall;
		}

		for (int i = 0; i < _tabs.Length; i++)
		{
			_tabs[i].Tab.onClick.AddListener(_tabHandlers[i]);
		}
	}

	private void OnDisable()
	{
		for (int i = 0; i < _tabs.Length; i++)
		{
			_tabs[i].Tab.onClick.RemoveListener(_tabHandlers[i]);
		}

		_fadeOutSequence.Stop();
		_fadeInTween.Stop();
		_dragHandle.StopDocking();
		if (_activeTabIndex >= 0)
		{
			SetAllPagesImmediate(_activeTabIndex);
		}
	}

	private void OnDestroy()
	{
		if (_dialogue)
		{
			_dialogue.UnregisterTagHandler(CallTagIdentifier);
			_dialogue.OnDialogueEnded -= EndCall;
		}
	}

	private void Update()
	{
		if (_scrollToBottomPending && _callLogScroll.gameObject.activeInHierarchy)
		{
			ScrollCallLogToBottom();
		}
	}

	public void ShowTab(int index)
	{
		if (index == _activeTabIndex)
		{
			return;
		}

		bool isInitial = _activeTabIndex < 0;
		_activeTabIndex = index;
		_fadeOutSequence.Stop();
		_fadeInTween.Stop();

		if (isInitial)
		{
			SetAllPagesImmediate(index);
			FocusAndUpdateScroll(index);
			return;
		}

		bool hasOutgoingFade = false;

		for (int i = 0; i < _tabs.Length; i++)
		{
			TabPage tab = _tabs[i];
			tab.Tab.interactable = i != index;

			if (i == index)
			{
				continue;
			}

			if (tab.Page.activeSelf && tab.CanvasGroup.alpha > 0f)
			{
				if (!hasOutgoingFade)
				{
					hasOutgoingFade = true;
					_fadeOutSequence = Sequence.Create(useUnscaledTime: _useUnscaledTime);
				}

				tab.CanvasGroup.interactable = false;
				tab.CanvasGroup.blocksRaycasts = false;
				_fadeOutSequence.Group(
					Tween
						.Alpha(tab.CanvasGroup, 0f, _pageFadeDuration, _pageFadeEase, useUnscaledTime: _useUnscaledTime)
						.OnComplete(tab.Page, static page => page.SetActive(false))
				);
			}
			else
			{
				SetPageState(tab, false);
			}
		}

		TabPage nextTab = _tabs[index];
		if (!nextTab.Page.activeSelf)
		{
			nextTab.CanvasGroup.alpha = 0f;
		}

		if (hasOutgoingFade)
		{
			_fadeOutSequence.ChainCallback(this, index, static (notebook, tabIndex) => notebook.StartFadeIn(tabIndex));
		}
		else
		{
			StartFadeIn(index);
		}
	}

	public void DockAboveBottomBar()
	{
		gameObject.SetActive(true);
		if (_dragHandle.IsDockedAbove(_bottomBar))
		{
			_dragHandle.MoveToPosition(_interviewPosition.position, _moveDuration, _moveEase, _useUnscaledTime);
			return;
		}

		_dragHandle.PlaceAboveBar(_bottomBar);
	}

	public void MoveToInterviewPosition()
	{
		_dragHandle.MoveToPosition(_interviewPosition.position, _moveDuration, _moveEase, _useUnscaledTime);

		for (int i = 0; i < _tabs.Length; i++)
		{
			if (_tabs[i].Page == _callLogScroll.gameObject)
			{
				ShowTab(i);
				return;
			}
		}
	}

	private void StartFadeIn(int index)
	{
		TabPage tab = _tabs[index];
		tab.Page.SetActive(true);
		tab.CanvasGroup.interactable = true;
		tab.CanvasGroup.blocksRaycasts = true;
		FocusAndUpdateScroll(index);
		_fadeInTween = Tween.Alpha(
			tab.CanvasGroup,
			1f,
			_pageFadeDuration,
			_pageFadeEase,
			useUnscaledTime: _useUnscaledTime
		);
	}

	private void SetAllPagesImmediate(int activeIndex)
	{
		for (int i = 0; i < _tabs.Length; i++)
		{
			bool isActive = i == activeIndex;
			_tabs[i].Tab.interactable = !isActive;
			SetPageState(_tabs[i], isActive);
		}
	}

	private static void SetPageState(TabPage tab, bool isActive)
	{
		tab.Page.SetActive(isActive);
		tab.CanvasGroup.alpha = isActive ? 1f : 0f;
		tab.CanvasGroup.interactable = isActive;
		tab.CanvasGroup.blocksRaycasts = isActive;
	}

	private void FocusAndUpdateScroll(int index)
	{
		TabPage activeTab = _tabs[index];
		if (activeTab.InputField)
		{
			activeTab.InputField.Select();
			activeTab.InputField.ActivateInputField();
		}
		else
		{
			EventSystem.current.SetSelectedGameObject(null);
		}

		if (_scrollToBottomPending && activeTab.Page == _callLogScroll.gameObject)
		{
			ScrollCallLogToBottom();
		}
	}

	private void HandleCallTag(string[] args)
	{
		AppendLine(args[0], _dialogue.CurrentStoryText.Trim());
	}

	private void EndCall() => _lastContact = null;

	private void AppendLine(string contact, string line)
	{
		if (_lastContact != contact)
		{
			if (_callLog.Length > 0)
			{
				_callLog.AppendLine();
			}
			_callLog.AppendLine(contact.ToUpperInvariant());
			_lastContact = contact;
		}
		_callLog.AppendLine(line);
		_transcript.text = _callLog.ToString();

		if (_callLogScroll.gameObject.activeInHierarchy)
		{
			ScrollCallLogToBottom();
		}
		else
		{
			_scrollToBottomPending = true;
		}
	}

	private void ScrollCallLogToBottom()
	{
		_scrollToBottomPending = false;
		Canvas.ForceUpdateCanvases();
		_callLogScroll.verticalNormalizedPosition = 0f;
	}
}
