using System;
using System.Collections.Generic;
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

	[SerializeField]
	private CharacterSO[] _characters = Array.Empty<CharacterSO>();

	[SerializeField]
	private CharacterSO _defaultSpeaker;

	[Title("Notes")]
	[SerializeField]
	private TMP_InputField _notesInput;

	[SerializeField, Min(0f)]
	private float _notesScrollbarSpacing = 8f;

	private const string CallTagIdentifier = "call";

	private readonly StringBuilder _callLog = new();
	private readonly Dictionary<string, CharacterSO> _characterLookup = new(StringComparer.OrdinalIgnoreCase);
	private UnityAction[] _tabHandlers;
	private Sequence _fadeOutSequence;
	private Tween _fadeInTween;
	private int _activeTabIndex = -1;
	private string _lastContact;
	private float _notesBaseRightOffset;
	private bool _notesScrollbarDirty;
	private bool _scrollToBottomPending;
	private DialogueManager _dialogue;

	private void Awake()
	{
		_transcript.richText = true;
		_notesBaseRightOffset = _notesInput.textViewport.offsetMax.x;
		BuildCharacterLookup();

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

		_notesInput.onValueChanged.AddListener(HandleNotesValueChanged);
		for (int i = 0; i < _tabs.Length; i++)
		{
			_tabs[i].Tab.onClick.AddListener(_tabHandlers[i]);
		}
	}

	private void OnDisable()
	{
		_notesInput.onValueChanged.RemoveListener(HandleNotesValueChanged);
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

	private void LateUpdate()
	{
		if (_notesScrollbarDirty && _notesInput.gameObject.activeInHierarchy)
		{
			UpdateNotesScrollbar();
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
			if (activeTab.InputField == _notesInput)
			{
				_notesScrollbarDirty = true;
			}
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
		_callLog.AppendLine(FormatCallLine(line));
		_callLog.AppendLine();
		_transcript.text = _callLog.ToString().TrimEnd();

		if (_callLogScroll.gameObject.activeInHierarchy)
		{
			ScrollCallLogToBottom();
		}
		else
		{
			_scrollToBottomPending = true;
		}
	}

	private void BuildCharacterLookup()
	{
		foreach (CharacterSO character in _characters)
		{
			if (!character)
			{
				continue;
			}

			if (!string.IsNullOrEmpty(character.Id))
			{
				_characterLookup[character.Id] = character;
			}

			if (!string.IsNullOrEmpty(character.Name))
			{
				_characterLookup[character.Name] = character;
			}
		}
	}

	private string FormatCallLine(string line)
	{
		CharacterSO speaker = ResolveSpeaker(line);
		if (!speaker)
		{
			return line;
		}

		string hex = ColorUtility.ToHtmlStringRGB(speaker.ColorTheme);
		return $"<color=#{hex}>{line}</color>";
	}

	private CharacterSO ResolveSpeaker(string line)
	{
		int colonIndex = line.IndexOf(':');
		if (colonIndex > 0)
		{
			string speakerId = line.Substring(0, colonIndex).Trim();
			if (_characterLookup.TryGetValue(speakerId, out CharacterSO character))
			{
				return character;
			}
		}

		return _defaultSpeaker;
	}

	private void ScrollCallLogToBottom()
	{
		_scrollToBottomPending = false;
		Canvas.ForceUpdateCanvases();
		_callLogScroll.verticalNormalizedPosition = 0f;
	}

	private void HandleNotesValueChanged(string _) => _notesScrollbarDirty = true;

	private void UpdateNotesScrollbar()
	{
		_notesScrollbarDirty = false;
		Scrollbar scrollbar = _notesInput.verticalScrollbar;
		if (!scrollbar)
		{
			return;
		}

		RectTransform viewport = _notesInput.textViewport;
		bool needsScrollbar = _notesInput.textComponent.preferredHeight > viewport.rect.height + 0.01f;
		if (scrollbar.gameObject.activeSelf != needsScrollbar)
		{
			scrollbar.gameObject.SetActive(needsScrollbar);
			float scrollbarWidth = ((RectTransform)scrollbar.transform).sizeDelta.x;
			Vector2 offsetMax = viewport.offsetMax;
			offsetMax.x = needsScrollbar ? _notesBaseRightOffset - scrollbarWidth - _notesScrollbarSpacing : _notesBaseRightOffset;
			viewport.offsetMax = offsetMax;
			_notesInput.textComponent.ForceMeshUpdate();
			scrollbar.size = needsScrollbar
				? Mathf.Clamp01(viewport.rect.height / _notesInput.textComponent.preferredHeight)
				: 1f;
		}
	}
}
