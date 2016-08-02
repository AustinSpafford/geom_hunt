using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ClickTarget))] // The click-target is what sends us messages.
public class HoverCausesColorFade : MonoBehaviour
{
	[ColorUsageAttribute(true, true, 0.0f, 8.0f, 0.125f, 3.0f)] // Enable HDR color-selections to permit textured objects to be brightened.
	public Color FadeTargetColor = new Color(1.5f, 1.5f, 1.5f);

	public float FadeInHalflife = 0.05f;
	public float FadeOutHalflife = 0.25f;

	public bool DebugEnabled = false;

	public void Start()
	{
		clickTarget = gameObject.GetComponent<ClickTarget>();
		rendererComponent = gameObject.GetComponent<Renderer>();
		optionalProgressCondition = gameObject.GetComponent<ProgressCondition>();

		if (rendererComponent != null)
		{
			originalSharedMaterial = rendererComponent.sharedMaterial;
			originalColor = originalSharedMaterial.color;
		}
	}

	public void Destroy()
	{
		if (fadeInstancedMaterial != null)
		{
			Destroy(fadeInstancedMaterial);

			fadeInstancedMaterial = null;
			
			if (DebugEnabled)
			{
				Debug.Log("Destroyed fade-material as part of cleanup.");
			}
		}
	}

	public void Update()
	{
		bool canAcceptHover = (
			(optionalProgressCondition == null) ||
			optionalProgressCondition.ConditionIsTrue);

		if (clickTarget.IsHovered &&
			canAcceptHover)
		{
			fadeFraction = 
				Mathf.SmoothDamp(
					fadeFraction,
					1.0f, // target
					ref fadeVelocity,
					FadeInHalflife);
		}
		else
		{
			fadeFraction = 
				Mathf.SmoothDamp(
					fadeFraction,
					0.0f, // target
					ref fadeVelocity,
					FadeOutHalflife);

			// Snap down when near zero so we can gracefully destroy our material instances.
			if (fadeFraction < 0.001f)
			{
				fadeFraction = 0.0f;
				fadeVelocity = 0.0f;
			}
		}

		fadeFraction = Mathf.Clamp01(fadeFraction);

		bool shouldHaveInstancedMaterial = (fadeFraction > 0.0f);

		if (shouldHaveInstancedMaterial &&
			(fadeInstancedMaterial == null))
		{
			fadeInstancedMaterial = new Material(originalSharedMaterial);

			rendererComponent.material = fadeInstancedMaterial;

			if (DebugEnabled)
			{
				Debug.Log("Instantiated fade-material.");
			}
		}
		else if ((shouldHaveInstancedMaterial == false) &&
			(fadeInstancedMaterial != null))
		{
			rendererComponent.material = originalSharedMaterial;

			Destroy(fadeInstancedMaterial);

			fadeInstancedMaterial = null;

			if (DebugEnabled)
			{
				Debug.Log("Destroyed fade-material since we've faded out.");
			}
		}

		if (fadeInstancedMaterial != null)
		{
			fadeInstancedMaterial.color = 
				Color.Lerp(
					originalColor, 
					FadeTargetColor, 
					fadeFraction);
		}
	}
	
	private ClickTarget clickTarget = null;
	private ProgressCondition optionalProgressCondition = null;
	private Renderer rendererComponent = null; // NOTE: "renderer" is present but deprecated, and the "new" keyword generates a warning in release builds.

	private Material originalSharedMaterial = null;
	private Color originalColor = Color.white;

	private Material fadeInstancedMaterial = null;
	private float fadeFraction = 0.0f;
	private float fadeVelocity = 0.0f;
}
