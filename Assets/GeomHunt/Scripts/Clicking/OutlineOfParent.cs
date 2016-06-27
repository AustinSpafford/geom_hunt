using UnityEngine;
using System.Collections;

[RequireComponent (typeof (ClickTarget))] // The click-target is what sends us messages.
public class OutlineOfParent : MonoBehaviour
{
	// TODO!
	/* 
	public Material OutlineMaterial = null; // Specified by whoever instantiated us.

	public void Start()
	{
		thicknessShaderPropertyId = Shader.PropertyToID("Thickness");

		CloneParentRenderer();
	}

	public void Destroy()
	{
		if (materialInstance != null)
		{
			renderer.material = null;

			Destroy(materialInstance);

			materialInstance = null;
		}
	}

	public void SetFadeState(
		Color outlineColor,
		float outlineThickness)
	{
		if (materialInstance != null)
		{
			materialInstance.color = outlineColor;

			materialInstance.SetFloat(thicknessShaderPropertyId, outlineThickness);
		}
	}

	private int thicknessShaderPropertyId = -1;
	
	private ClickTarget clickTarget = null;
	new private Renderer renderer = null;

	private Material materialInstance = null;

	private void CloneParentRenderer()
	{
		Renderer parentRenderer = transform.parent.GetComponent<Renderer>();

		if (parentRenderer == null)
		{
			throw new System.InvalidOperationException("Our parent lacks a renderer, therefore we cannot generate an outline!");
		}

		renderer = gameObject.AddComponent<Renderer>();

		System.Reflection.PropertyInfo[] rendererProperties = typeof(Renderer).GetProperties();
		
		foreach (System.Reflection.PropertyInfo property in rendererProperties)
		{
			if (property.CanWrite)
			{
				property.SetValue(
					renderer, 
					property.GetValue(parentRenderer, null),
					null);
			}
		}

		materialInstance = new Material(OutlineMaterial);

		renderer.material = materialInstance;
	}
	*/
}
