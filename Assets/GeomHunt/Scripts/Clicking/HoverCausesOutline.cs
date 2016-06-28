using UnityEngine;
using System.Collections;

[RequireComponent (typeof (ClickTarget))] // The click-target is what sends us messages.
public class HoverCausesOutline : MonoBehaviour
{
	public Shader OutlineShader = null;

	public Color OutlineColor = Color.magenta;
	public float OutlineThickness = 0.005f;

	public float FadeInHalflife = 0.05f;
	public float FadeOutHalflife = 0.25f;

	public bool DebugEnabled = false;

	public void Start()
	{
		outlineColorPropertyId = Shader.PropertyToID("_OutlineColor");
		outlineThicknessPropertyId = Shader.PropertyToID("_OutlineThickness");

		if (OutlineShader == null)
		{
			throw new System.InvalidOperationException("An OutlineShader must be specified for this component to function!");
		}

		clickTarget = gameObject.GetComponent<ClickTarget>();
		renderer = gameObject.GetComponent<Renderer>();

		if (renderer != null)
		{
			originalSharedMaterial = renderer.sharedMaterial;
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
		if (clickTarget.IsHovered)
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

			fadeInstancedMaterial.shader = OutlineShader;

			renderer.material = fadeInstancedMaterial;

			if (DebugEnabled)
			{
				Debug.Log("Instantiated fade-material.");
			}
		}
		else if ((shouldHaveInstancedMaterial == false) &&
			(fadeInstancedMaterial != null))
		{
			renderer.material = originalSharedMaterial;

			Destroy(fadeInstancedMaterial);

			fadeInstancedMaterial = null;

			if (DebugEnabled)
			{
				Debug.Log("Destroyed fade-material since we've faded out.");
			}
		}

		if (fadeInstancedMaterial != null)
		{
			Color currentOutlineColor = 
				Color.Lerp(
					Color.black, 
					OutlineColor, 
					fadeFraction);

			fadeInstancedMaterial.SetColor(
				outlineColorPropertyId,
				currentOutlineColor);
			
			fadeInstancedMaterial.SetFloat(
				outlineThicknessPropertyId,
				OutlineThickness);
		}
	}

	private int outlineColorPropertyId = -1;
	private int outlineThicknessPropertyId = -1;
	
	private ClickTarget clickTarget = null;
	new private Renderer renderer = null;

	private Material originalSharedMaterial = null;

	private Material fadeInstancedMaterial = null;
	private float fadeFraction = 0.0f;
	private float fadeVelocity = 0.0f;
}
