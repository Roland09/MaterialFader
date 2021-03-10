using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rowlan
{
	/// <summary>
	/// Fade a material property value in and out with specified easing functions.
	/// Supported property types:
	/// + Float
	/// + Color
	/// 
	/// Please note that there's also the option to Lerp between materials in Unity:
	/// https://docs.unity3d.com/ScriptReference/Material.Lerp.html
	/// 
	/// </summary>
	public class MaterialFader : MonoBehaviour
	{
		#region Public Variables
		[Header("GameObject")]

		public GameObject sceneGameObject;

		[Header("Material")]

		public MaterialPropertyType propertyType = MaterialPropertyType.Float;

		[Tooltip("The property name in the shader, e. g. _EmissionColor")]
		public string propertyNameID;

		[Header( "Fade")]

		public float minimumValue = 0f;
		public float maximumValue = 1f;

		[Tooltip("The fade duration in seconds")]
		public float duration = 1f;

		[Tooltip("The easing meachinsm")]
		public Ease ease = Ease.Linear;

		[Header("Input")]

		[Tooltip("The input key that triggers the fading")]
		public KeyCode toggleKeyCode = KeyCode.None;

		#endregion Public Variables

		#region Internal Variables
		private FadeDirection fadeDirection = FadeDirection.In;

		private Coroutine currentFadeCoroutine = null;

		private readonly List<FadeMaterial> fadeMaterials = new List<FadeMaterial>();

		#endregion Internal Variables

		#region Initialization
		void Start()
		{
			RegisterFadeMaterials();
		}

		private void RegisterFadeMaterials()
		{
			Renderer[] rendererChildren = sceneGameObject.GetComponentsInChildren<Renderer>();

			foreach (Renderer renderer in rendererChildren)
			{
				FadeMaterial fadeMaterial = null;

				switch (propertyType)
				{
					case MaterialPropertyType.Float:
						fadeMaterial = new FloatFadeMaterial(renderer, propertyNameID);
						break;
					case MaterialPropertyType.Color:
						fadeMaterial = new ColorFadeMaterial(renderer, propertyNameID);
						break;
				}

				if (fadeMaterial.IsValid())
				{
					fadeMaterials.Add(fadeMaterial);
				}
			}
		}
		#endregion Initialization

		#region Fade Trigger
		void Update()
		{
			if (Input.anyKeyDown)
			{
				if (Input.GetKeyDown(toggleKeyCode))
				{
					if (currentFadeCoroutine != null)
					{
						StopCoroutine(currentFadeCoroutine);
					}

					// start fading
					if (fadeDirection == FadeDirection.In)
					{
						currentFadeCoroutine = StartCoroutine(Fade(minimumValue, maximumValue));
					}
					else if (fadeDirection == FadeDirection.Out)
					{
						currentFadeCoroutine = StartCoroutine(Fade(maximumValue, minimumValue));
					}

					// toggle fade direction
					fadeDirection = fadeDirection == FadeDirection.In ? FadeDirection.Out : FadeDirection.In;

				}
			}
		}
		#endregion Fade Trigger

		#region Fade Logic
		IEnumerator Fade( float beginValue, float endValue)
		{
			float timeElapsed = 0;

			while (timeElapsed < duration)
			{
				float value = ease.Lerp(beginValue, endValue, timeElapsed / duration);

				timeElapsed += Time.deltaTime;

				UpdateMaterials(value);

				yield return null;
			}

			// set end value explicitly, otherwise it might not be the specified one because of the time increments
			UpdateMaterials(endValue);
		}

		void UpdateMaterials(float value)
		{
			foreach(FadeMaterial fadeMaterial in fadeMaterials) {
				fadeMaterial.UpdateMaterials(value);
			}
		}

		#endregion Fade Logic

		#region FadeMaterial
		public abstract class FadeMaterial {

			protected string propertyNameID;
			protected Renderer renderer;
			protected List<Material> materials;

			public FadeMaterial(Renderer renderer, string propertyNameID)
			{
				this.renderer = renderer;
				this.propertyNameID = propertyNameID;

				RegisterMaterials();
			}

			private void RegisterMaterials() {

				materials = new List<Material>();

				foreach (Material material in renderer.materials)
				{
					if (material.HasProperty(propertyNameID))
					{
						materials.Add(material);
					}
				}
			}

			public bool IsValid()
			{
				return materials.Count > 0;
			}

			public abstract void UpdateMaterials(float value);
		}

		public class FloatFadeMaterial: FadeMaterial
		{
			public FloatFadeMaterial(Renderer renderer, string propertyNameID) : base(renderer, propertyNameID)
			{
			}

			public override void UpdateMaterials(float value)
			{
				for (int i = 0; i < materials.Count; i++)
				{

					Material material = materials[i];
					material.SetFloat(propertyNameID, value);
				}

				renderer.UpdateGIMaterials();
			}
		}

		public class ColorFadeMaterial: FadeMaterial
		{
			private readonly List<Color> baseColors = new List<Color>();

			public ColorFadeMaterial(Renderer renderer, string propertyNameID) : base(renderer, propertyNameID)
			{
				RegisterBaseColors();
			}

			private void RegisterBaseColors() {

				foreach (Material material in materials)
				{
					baseColors.Add(material.GetColor(propertyNameID));
				}
			}

			public override void UpdateMaterials(float value)
			{
				for (int i = 0; i < materials.Count; i++)
				{

					Material material = materials[i];

					Color baseColor = baseColors[i];
					Color color = baseColor * value; // note about gamma: for gamma you could use baseColor * Mathf.LinearToGammaSpace( value)

					material.SetColor(propertyNameID, color);
				}

				// update the material; this is an important step, otherwise the material changes wouldn't show up
				renderer.UpdateGIMaterials();
			}
		}
		#endregion FadeMaterial
	}

	#region Enums
	public enum FadeDirection
	{
		In,
		Out
	}

	public enum MaterialPropertyType
	{
		Float,
		Color
	}

	public enum Ease
	{
		Linear,
		EaseInQuad,
		EaseOutQuad
	}

	public static class EaseExtensions
	{
		public static float Lerp(this Ease ease, float start, float end, float value)
		{
			switch (ease)
			{
				case Ease.Linear: return Mathf.Lerp(start, end, value);
				case Ease.EaseInQuad: end -= start; return end * value * value + start;
				case Ease.EaseOutQuad: end -= start; return -end * value * (value - 2) + start;
				default: throw new ArgumentOutOfRangeException("Unsupported parameter " + ease);
			}
		}

	}
	#endregion Enums
}