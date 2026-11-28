using System.Diagnostics.CodeAnalysis;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "Odin.OdinUnknownGroupingPath")]
public class FMODEvents : MonoSingleton<FMODEvents>
{
	#region Music

	[field: SerializeField]
	[field: FoldoutGroup("Music", true)]
	public EventReference MainMenu_Bgm { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Music")]
	public EventReference Gameplay_Bgm { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Music")]
	public EventReference PhoneCall_Bgm { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Music")]
	public EventReference PoliceTheme_Bgm { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Music")]
	public EventReference AssistantTheme_Bgm { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Music")]
	public EventReference HeiressTheme_Bgm { get; private set; }

	#endregion

	#region Ambience

	[field: SerializeField]
	[field: FoldoutGroup("Ambience", expanded: true)]
	public EventReference GameplayRain_Amb { get; private set; }

	#endregion

	#region SFX

	[field: SerializeField]
	[field: FoldoutGroup("SFX", true)]
	public EventReference CameraClick_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX")]
	public EventReference DoorOpening_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX")]
	public EventReference Step_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX")]
	public EventReference Keys_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX")]
	public EventReference NotepadPageTurning_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX")]
	public EventReference PaperRustling_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX")]
	public EventReference PhoneRingPickUp_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX")]
	public EventReference SettingsButton_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX")]
	public EventReference SettingsTest1_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX")]
	public EventReference SettingsTest2_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("SFX")]
	public EventReference SettingsTest3_Sfx { get; private set; }

	#endregion
}
