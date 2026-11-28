using System;
using System.Collections.Generic;
using System.Linq;
using Ink.Runtime;
using Ink.UnityIntegration;
using JetBrains.Annotations;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
///     Acts as the bridge between Ink narrative scripts and Unity gameplay systems.
/// </summary>
public class DialogueManager : MonoSingleton<DialogueManager>
{
	[FoldoutGroup("References")]
	[SerializeField]
	private TextAsset _inkJson;

	[FoldoutGroup("References")]
	[SerializeField]
	private InkFile _inkStory;

	[FoldoutGroup("Transition Times")]
	[SerializeField]
	private float _characterFadeDuration = 0.5f;

	[FoldoutGroup("Transition Times")]
	[SerializeField, Min(0f)]
	private float _delayBeforeChoices = 1f;

	private readonly HashSet<string> _collectedItems = new(StringComparer.OrdinalIgnoreCase);
	private readonly HashSet<string> _interactedObjects = new(StringComparer.OrdinalIgnoreCase);
	private InventoryUI _inventory;
	private CameraManager _camera;
	private int _choiceIndex = -1;
	private string _currentSpeakerName;

	private bool _needToDisplayChoices;
	private Story _story;
	private bool _storyPlaying;
	private bool _typewriterPlaying;
	private Tween _choiceDelayTween;
	private Tween _choiceSelectDelayTween;

	/// <summary>
	///     The dialogue state instance owned by this controller.
	///     Other scripts should reference this to subscribe to dialogue events.
	/// </summary>
	public DialogueState DialogueState { get; } = new();

	public bool IsStoryPlaying => _storyPlaying;
	public bool HasKnot(string knotName) => !string.IsNullOrWhiteSpace(knotName) && _story != null && _story.KnotContainerWithName(knotName.Trim()) != null;
	public string CurrentStoryText => _story != null ? _story.currentText : string.Empty;

	public string CurrentChoiceKnot
	{
		get
		{
			if (_story == null || _story.currentChoices.Count == 0)
			{
				return string.Empty;
			}

			string sourcePath = _story.currentChoices[0].sourcePath;
			int dotIndex = sourcePath.IndexOf('.');
			return dotIndex > 0 ? sourcePath.Substring(0, dotIndex) : sourcePath;
		}
	}

	private bool ChoicesAvailable => _story.currentChoices.Count > 0;

	#region Events

	/// <summary>Supplies the Ink knot name, or null for a standalone line.</summary>
	public event Action<string> OnDialogueStarted;
	public event Action OnDialogueEnded;
	public event Func<int, float> OnGetChoiceDelay;
	internal event Action OnStandaloneContinue;

	public string GetChoiceText(int choiceIndex) => _story.currentChoices[choiceIndex].text;

	#endregion

	#region Unity Functions

	protected override void OnInitialized()
	{
		// Prevent continuing if paused
		DialogueState.AddBlockingCondition(() => Time.timeScale == 0f);

		string storyJson = _inkStory ? _inkStory.storyJson : (_inkJson ? _inkJson.text : null);
		if (string.IsNullOrEmpty(storyJson))
		{
			return;
		}

		_story = new Story(storyJson);
		InitializeTagHandlers();

		// Bind External Functions
		_story.BindExternalFunction("IsQuestCompleted", (string questId) => IsQuestCompleted(questId));
		_story.BindExternalFunction("IsQuestActive", (string questId) => IsQuestActive(questId));
		_story.BindExternalFunction("StartQuest", (string questId) => StartQuest(questId));
		_story.BindExternalFunction("CompleteQuest", (string questId) => CompleteQuest(questId));
		_story.BindExternalFunction("HasItem", (string itemId) => HasItem(itemId), lookaheadSafe: true);
		_story.BindExternalFunction("HasInteracted", (string objectId) => HasInteracted(objectId), lookaheadSafe: true);
		_story.BindExternalFunction("RecordInteraction", (string objectId) => RecordInteraction(objectId));
	}

	private void OnEnable()
	{
		DialogueState.OnStartStory += StartStory;
		DialogueState.OnChoiceSelect += ContinueStory;
		DialogueState.OnTypewriterFinish += SetTypewriterInactive;
		DialogueState.OnTypewriterFinish += DisplayChoices;
		InputManager.Instance.OnContinueStoryPerformed += ContinueStory;
	}

	private void OnDisable()
	{
		_choiceDelayTween.Stop();
		_choiceSelectDelayTween.Stop();
		DialogueState.OnStartStory -= StartStory;
		DialogueState.OnChoiceSelect -= ContinueStory;
		DialogueState.OnTypewriterFinish -= SetTypewriterInactive;
		DialogueState.OnTypewriterFinish -= DisplayChoices;

		if (InputManager.Instance)
		{
			InputManager.Instance.OnContinueStoryPerformed -= ContinueStory;
		}
	}

	private void Start()
	{
		DialogueState.RemoveAllCharacters();
		_inventory = FindFirstObjectByType<InventoryUI>();
		_camera = CameraManager.Instance;
		if (_camera)
		{
			_camera.OnPhotoTaken += HandlePhotoTaken;
		}
	}

	private void OnDestroy()
	{
		if (_camera)
		{
			_camera.OnPhotoTaken -= HandlePhotoTaken;
		}
	}

	#endregion

	#region Debug

	[FoldoutGroup("Debug")]
	[Button]
	[UsedImplicitly]
	private void TestStory(string knotName)
	{
		try
		{
			DialogueState.StartStory(knotName);
		}
		catch (Exception e)
		{
			Debug.LogWarning($"No story with knot \"{knotName}\": {e}");
		}
	}

	[FoldoutGroup("Debug")]
	[Button]
	[UsedImplicitly]
	private void StartBasicTest()
	{
		DialogueState.StartStory("Testing");
	}

	[FoldoutGroup("Debug")]
	[Button]
	[UsedImplicitly]
	private void StartAnimationTest()
	{
		DialogueState.StartStory("Testing_Animations");
	}

	[FoldoutGroup("Debug")]
	[Button]
	[UsedImplicitly]
	private void StartVoiceTest()
	{
		DialogueState.StartStory("Testing_Voices");
	}

	[FoldoutGroup("Debug")]
	[Button]
	[UsedImplicitly]
	private void StartTextAnimatorTest()
	{
		DialogueState.StartStory("Testing_TextAnimator");
	}

	#endregion

	#region External Functions

	private bool IsQuestCompleted(string questId)
	{
		return QuestManager.Instance.IsQuestCompleted(questId);
	}

	private bool IsQuestActive(string questId)
	{
		return QuestManager.Instance.IsQuestActive(questId);
	}

	private void StartQuest(string questId)
	{
		QuestManager.Instance.StartQuest(questId);
	}

	private void CompleteQuest(string questId)
	{
		QuestManager.Instance.CompleteQuest(questId);
	}

	public void RecordItem(string itemId) => _collectedItems.Add(itemId.Trim());

	public void RecordInteraction(string objectId) => _interactedObjects.Add(objectId.Trim());

	public bool HasItem(string itemId)
	{
		string trimmed = itemId.Trim();
		return _collectedItems.Contains(trimmed) || (_inventory && _inventory.HasItem(trimmed));
	}

	public bool HasInteracted(string objectId) => _interactedObjects.Contains(objectId.Trim());

	private void HandlePhotoTaken(PhotoSpot spot)
	{
		_collectedItems.Add(spot.name);
		if (spot.ItemData != null && !string.IsNullOrWhiteSpace(spot.ItemData.ItemName))
		{
			_collectedItems.Add(spot.ItemData.ItemName.Trim());
		}
	}

	#endregion

	#region Story Managers

	/// <summary>
	/// Broadcasts a standalone line. Each display manages its own lifetime.
	/// </summary>
	public bool DisplayLoneLine(string text, string speaker = "")
	{
		if (string.IsNullOrWhiteSpace(text) || !isActiveAndEnabled || _storyPlaying)
		{
			return false;
		}
		if (!DialogueState.CanAdvanceStory)
		{
			return false;
		}

		OnDialogueStarted?.Invoke(null);
		DialogueState.StartDialogue();
		DialogueState.DisplayDialogue(speaker, text);
		return true;
	}

	/// <summary>
	///     Starts the story from a specified Ink knot.
	/// </summary>
	private void StartStory(string knotName)
	{
		if (_story == null)
		{
			Debug.LogWarning("[DialogueManager] Assign an Ink JSON asset before starting an Ink story.");
			return;
		}

		_choiceDelayTween.Stop();
		_choiceSelectDelayTween.Stop();
		_typewriterPlaying = false;
		_needToDisplayChoices = false;
		_choiceIndex = -1;
		_currentSpeakerName = "";
		if (_storyPlaying)
		{
			// Restart the story
			_storyPlaying = false;
			DialogueState.RemoveAllCharacters();
		}

		_storyPlaying = true;
		OnDialogueStarted?.Invoke(knotName);

		if (knotName == "")
		{
			Debug.LogWarning("[DialogueController] KnotName is Empty");
		}
		else
		{
			_story.ChoosePathString(knotName);
		}

		ContinueStory();
	}

	/// <summary>
	///     Advances the story to the next line or handles player interaction.
	/// </summary>
	public void ContinueStory()
	{
		// Check global blocking conditions (Pause, Backlog, etc.)
		if (!DialogueState.CanAdvanceStory)
		{
			return;
		}

		if (!_storyPlaying)
		{
			OnStandaloneContinue?.Invoke();
			return;
		}

		// If typewriter is playing, skip it instead of continuing
		if (_typewriterPlaying)
		{
			DialogueState.SkipTypewriter();
			return;
		}

		if (_choiceDelayTween.isAlive)
		{
			_choiceDelayTween.Complete();
			return;
		}

		if (_choiceSelectDelayTween.isAlive)
		{
			return;
		}

		// If a choice was selected, proceed down that path
		if (_choiceIndex != -1)
		{
			if (ChoicesAvailable && _choiceIndex >= 0 && _choiceIndex < _story.currentChoices.Count)
			{
				_story.ChooseChoiceIndex(_choiceIndex);
			}
			_choiceIndex = -1;
		}

		if (_story.canContinue)
		{
			_story.Continue();
			_typewriterPlaying = true;

			// Signal start of new dialogue line
			DialogueState.StartDialogue();

			DisplayLine();
		}
		else if (!ChoicesAvailable)
		{
			ExitStory();
		}
	}

	/// <summary>
	///     Continues the story after a choice is selected.
	/// </summary>
	/// <param name="choiceIndex">Index of the selected choice.</param>
	private void ContinueStory(int choiceIndex)
	{
		if (!_storyPlaying || !DialogueState.CanAdvanceStory || !ChoicesAvailable)
		{
			return;
		}

		if (choiceIndex < 0 || choiceIndex >= _story.currentChoices.Count)
		{
			return;
		}

		_choiceDelayTween.Stop();
		_choiceSelectDelayTween.Stop();
		_typewriterPlaying = false;
		_choiceIndex = choiceIndex;

		float delay = OnGetChoiceDelay?.Invoke(choiceIndex) ?? 0f;
		if (delay > 0f)
		{
			_choiceSelectDelayTween = Tween.Delay(this, delay, static manager => manager.ContinueStory());
			return;
		}

		ContinueStory();
	}

	/// <summary>
	///     Ends the current story and cleans up state.
	/// </summary>
	private void ExitStory()
	{
		_choiceDelayTween.Stop();
		_choiceSelectDelayTween.Stop();
		_storyPlaying = false;
		_typewriterPlaying = false;
		_needToDisplayChoices = false;
		_choiceIndex = -1;
		_currentSpeakerName = "";

		DialogueState.EndStory();
		OnDialogueEnded?.Invoke();
	}

	#endregion

	#region Dialogue Display

	/// <summary>
	///     Displays the current line
	/// </summary>
	private void DisplayLine()
	{
		string storyLine = _story.currentText;
		ParseTags(_story.currentTags);
		while (string.IsNullOrWhiteSpace(storyLine) && _story.canContinue)
		{
			storyLine = _story.Continue();
			ParseTags(_story.currentTags);
		}

		_needToDisplayChoices = ChoicesAvailable;
		if (string.IsNullOrWhiteSpace(storyLine))
		{
			_typewriterPlaying = false;
			if (ChoicesAvailable)
			{
				ShowPendingChoices();
			}
			else
			{
				ExitStory();
			}
			return;
		}

		DialogueState.DisplayDialogue(_currentSpeakerName, storyLine);
	}

	/// <summary>
	///     Displays choices if they are pending after typewriter completes.
	/// </summary>
	private void DisplayChoices()
	{
		if (!_needToDisplayChoices)
		{
			return;
		}

		if (_delayBeforeChoices > 0f)
		{
			_choiceDelayTween.Stop();
			_choiceDelayTween = Tween.Delay(this, _delayBeforeChoices, static manager => manager.ShowPendingChoices());
			return;
		}

		ShowPendingChoices();
	}

	private void ShowPendingChoices()
	{
		var choices = _story.currentChoices.Select(choice => choice.text).ToList();
		DialogueState.DisplayChoices(choices);
		_needToDisplayChoices = false;
	}

	/// <summary>
	///     Marks the typewriter as inactive when animation completes.
	/// </summary>
	private void SetTypewriterInactive()
	{
		_typewriterPlaying = false;
	}

	#endregion

	#region Tag Parsing

	private readonly Dictionary<string, Action<string[]>> _tagHandlers = new();

	/// <summary>
	///     Registers a custom Ink tag handler for the given identifier (e.g. "call" for #call_Laila).
	/// </summary>
	public void RegisterTagHandler(string identifier, Action<string[]> handler) =>
		_tagHandlers[identifier.ToLower()] = handler;

	/// <summary>
	///     Unregisters a custom Ink tag handler.
	/// </summary>
	public void UnregisterTagHandler(string identifier) => _tagHandlers.Remove(identifier.ToLower());

	/// <summary>
	///     Initializes the tag handler dictionary with all built-in tag types.
	///     Called in Awake after story initialization.
	/// </summary>
	private void InitializeTagHandlers()
	{
		_tagHandlers["ch"] = HandleCharacterTag;
		_tagHandlers["chl"] = args => HandlePositionedCharacterTag(args, CharacterPosition.Left);
		_tagHandlers["chc"] = args => HandlePositionedCharacterTag(args, CharacterPosition.Center);
		_tagHandlers["chr"] = args => HandlePositionedCharacterTag(args, CharacterPosition.Right);
		_tagHandlers["an"] = HandleAnimationTag;
		_tagHandlers["nm"] = HandleNameTag;
		_tagHandlers["d"] = HandleDialogueVoiceTag;
		_tagHandlers["bg"] = HandleBackgroundTag;
		_tagHandlers["sfx"] = HandleSFXTag;
		_tagHandlers["ms"] = HandleMusicTag;
		_tagHandlers["ab"] = HandleAmbienceTag;
		foreach (string itemTag in new[] { "ast", "ll", "coat", "food", "bus", "items" })
		{
			_tagHandlers[itemTag] = _ => { };
		}
	}

	/// <summary>
	///     Parses a list of Ink tags and dispatches them to appropriate handlers.
	///     Tags use underscore-delimited format: "identifier_arg1_arg2"
	/// </summary>
	/// <param name="tags">List of tag strings from the current Ink line.</param>
	private void ParseTags(List<string> tags)
	{
		foreach (string inkTag in tags)
		{
			string[] parts = inkTag.Split('_');
			if (parts.Length < 1)
			{
				Debug.LogWarning($"[DialogueController] Empty tag encountered: {inkTag}");
				continue;
			}

			string identifier = parts[0].ToLower();
			string[] args = parts.Skip(1).ToArray();

			ProcessTag(identifier, args, inkTag);
		}
	}

	/// <summary>
	///     Dispatches a parsed tag to its registered handler.
	/// </summary>
	/// <param name="identifier">The tag type identifier (e.g., "ch", "nm").</param>
	/// <param name="args">Arguments following the identifier.</param>
	/// <param name="rawTag">Original tag string for error reporting.</param>
	private void ProcessTag(string identifier, string[] args, string rawTag)
	{
		if (_tagHandlers.TryGetValue(identifier, out Action<string[]> handler))
		{
			handler(args);
		}
		else
		{
			Debug.LogWarning($"[DialogueController] Unknown tag identifier: '{identifier}' in tag: '{rawTag}'");
		}
	}

	#endregion

	#region Tag Handlers

	/// <summary>
	///     Handles character sprite tags. Defaults to Center position.
	///     Format: #ch_Sera_happy_open → characterKey = "sera_happy_open"
	///     Format: #ch_Sera_clear → Removes character "sera"
	/// </summary>
	private void HandleCharacterTag(string[] args)
	{
		HandlePositionedCharacterTag(args, CharacterPosition.Center);
	}

	/// <summary>
	///     Handles positioned character tags.
	///     Format: #chl_Sera_happy → Position: Left, ID: Sera, Key: Sera_happy
	///     Format: #chl_Sera → Position: Left, ID: Sera, Key: null (Move only)
	///     Format: #chl_Samurai1:Samurai_happy → Position: Left, ID: Samurai1, Key: Samurai_happy (alias syntax)
	///     Format: #chl_Sera_clear → Remove Sera Sprite
	/// </summary>
	private void HandlePositionedCharacterTag(string[] args, CharacterPosition position)
	{
		if (args.Length == 0)
		{
			return;
		}

		string firstArg = args[0];
		string characterId;
		string spriteBase;

		if (firstArg.ToLower() == "clear")
		{
			DialogueState.RemoveAllCharacters(_characterFadeDuration);
			return;
		}

		// Check for alias syntax: "CharacterId:SpriteBase" (e.g., "Sera:Girl_Happy")
		// This allows multiple characters to share the same sprite asset
		if (firstArg.Contains(':'))
		{
			string[] aliasParts = firstArg.Split(':');
			characterId = aliasParts[0]; // Tracking ID (e.g., "Sera")
			spriteBase = aliasParts[1]; // Sprite lookup base (e.g., "Girl_Happy")
		}
		else
		{
			characterId = firstArg;
			spriteBase = firstArg;
		}

		// Strip animation suffix from characterId if present (e.g., "clown+pop" -> "clown")
		if (characterId.Contains('+'))
		{
			characterId = characterId.Split('+')[0];
		}

		// Check for clear command
		if (args.Length > 1 && args[1].ToLower() == "clear")
		{
			DialogueState.RemoveCharacter(characterId, _characterFadeDuration);
			return;
		}

		// Reconstruct sprite key
		string spriteKey = "";
		if (args.Length >= 1)
		{
			// Replace the first arg with spriteBase for sprite lookup
			string[] spriteArgs = new string[args.Length];
			spriteArgs[0] = spriteBase;
			for (int i = 1; i < args.Length; i++)
			{
				spriteArgs[i] = args[i];
			}

			spriteKey = string.Join("_", spriteArgs);
		}

		// Handle inline animation syntax (e.g. Sera_Happy+Shake)
		if (spriteKey.Contains('+'))
		{
			string[] splitParts = spriteKey.Split('+');
			string realSpriteKey = splitParts[0];
			string animationPart = splitParts[1];

			// Update the character with the real sprite key
			DialogueState.UpdateCharacter(characterId, position, realSpriteKey, _characterFadeDuration);

			// Parse animation part (e.g. "shake_10_0.5")
			string[] animParts = animationPart.Split('_');
			string animName = animParts[0];
			string[] animArgs = animParts.Skip(1).ToArray();

			DialogueState.UpdateAnimation(characterId, animName, animArgs);
		}
		else
		{
			DialogueState.UpdateCharacter(characterId, position, spriteKey, _characterFadeDuration);
		}
	}

	private void HandleAnimationTag(string[] args)
	{
		if (args.Length < 2)
		{
			Debug.LogWarning(
				$"[DialogueController] Animation tag requires at least CharacterID and AnimationName (e.g. an_Sera_Shake). Args passed: {string.Join(", ", args)}"
			);
			return;
		}

		string characterId = args[0];
		string animationName = args[1];
		string[] parameters = args.Skip(2).ToArray();

		DialogueState.UpdateAnimation(characterId, animationName, parameters);
	}

	/// <summary>
	///     Handles speaker name tags.
	///     Format: #nm_Sera → Shows "Sera", #nm_none → Hides name
	/// </summary>
	private void HandleNameTag(string[] args)
	{
		if (args.Length == 0 || args[0].ToLower() == "none")
		{
			_currentSpeakerName = "";
			DialogueState.UpdateName("");
			return;
		}

		string characterName = args[0];
		_currentSpeakerName = characterName;
		DialogueState.UpdateName(characterName);
	}

	/// <summary>
	///     Handles dialogue voice tags to indicate spoken dialogue lines.
	///     Format: #d_sera → Marks line as spoken dialogue by "sera"
	/// </summary>
	private void HandleDialogueVoiceTag(string[] args)
	{
		if (args.Length == 0)
		{
			Debug.LogWarning("[DialogueController] Dialogue voice tag requires character name (e.g. #d_sera)");
			return;
		}

		string characterName = args[0].ToLower();
		DialogueState.SetDialogueVoice(characterName);
	}

	/// <summary>
	///     Handles background change tags.
	///     Format: #bg_park → backgroundKey = "park"
	/// </summary>
	private void HandleBackgroundTag(string[] args)
	{
		string backgroundKey = string.Join("_", args);
		DialogueState.UpdateBackground(backgroundKey);
	}

	/// <summary>
	///     Handles sound effect tags.
	///     Format: #sfx_door → sfxKey = "door"
	/// </summary>
	private void HandleSFXTag(string[] args)
	{
		string sfxKey = string.Join("_", args);
		DialogueState.PlaySFX(sfxKey);
	}

	/// <summary>
	///     Handles music change tags.
	///     Format: #ms_morning → musicKey = "morning"
	/// </summary>
	private void HandleMusicTag(string[] args)
	{
		string musicKey = string.Join("_", args);
		DialogueState.PlayMusic(musicKey);
	}

	/// <summary>
	///     Handles ambience change tags.
	///     Format: #ab_park → ambienceKey = "park"
	/// </summary>
	private void HandleAmbienceTag(string[] args)
	{
		string ambienceKey = string.Join("_", args);
		DialogueState.PlayAmbience(ambienceKey);
	}

	#endregion
}
