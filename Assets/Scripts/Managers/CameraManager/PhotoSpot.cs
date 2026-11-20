using UnityEngine;

/// <summary>
/// A clue in the room. Click it while holding the camera to see a photo of the past.
/// Needs an Image with Raycast Target ON (alpha 0 is fine).
/// </summary>
public class PhotoSpot : MonoBehaviour
{
	[Tooltip("The PNG shown in the polaroid (the past).")]
	[SerializeField]
	private Sprite _pastImage;

	[Tooltip("Optional text written under the polaroid.")]
	[SerializeField]
	[TextArea]
	private string _caption;

	public Sprite PastImage => _pastImage;

	public string Caption => _caption;

	public bool HasBeenPhotographed { get; private set; }

	public void MarkPhotographed()
	{
		HasBeenPhotographed = true;
	}
}
