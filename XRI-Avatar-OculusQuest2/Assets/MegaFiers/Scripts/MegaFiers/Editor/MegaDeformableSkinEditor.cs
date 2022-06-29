using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.Collections;

namespace MegaFiers
{
	[CustomEditor(typeof(MegaDeformableSkin))]
	public class MegaDeformableSkinEditor : Editor
	{
		static GUIStyle foldoutStyle;
		string	search = "";
		float setvalue = 0;

		public static GUIStyle FoldoutStyle
		{
			get
			{
				if ( foldoutStyle == null )
				{
					foldoutStyle = new GUIStyle(EditorStyles.foldout);
					//foldoutStyle.font = new GUIStyle("Label").font;
					foldoutStyle.fontSize = 14;
					//foldoutStyle.border = new RectOffset(15, 7, 4, 4);
					//foldoutStyle.fixedHeight = 22;
					//foldoutStyle.contentOffset = new Vector2(20.0f, -2.0f);
				}

				return foldoutStyle;
			}
		}

		public override void OnInspectorGUI()
		{
			MegaDeformableSkin mod = (MegaDeformableSkin)target;
			MegaEditorGUILayout.Toggle(mod, "Use Bone Weights", ref mod.useBoneWeights);

			MegaModifyObject mo = mod.GetComponent<MegaModifyObject>();
			if ( mo )
			{
				MegaEditorGUILayout.Slider(mo, "Selection Weight", ref mo.selectionWeight, 0.0f, 1.0f);
			}

			if ( mod.useBoneWeights )
			{
				//MegaEditorGUILayout.Toggle(mod, "Show Weights", ref mod.showWeights);
				mod.showWeights = EditorGUILayout.Foldout(mod.showWeights, "Show Bone Weights", FoldoutStyle);

				if ( mod.showWeights )
				{
					search = EditorGUILayout.TextField("Search Bones", search);

					setvalue = EditorGUILayout.Slider("Set Value", setvalue, 0.0f, 1.0f);
					if ( GUILayout.Button("Set to Value") )
					{
						for ( int b = 0; b < mod.bones.Count; b++ )
						{
							if ( search.Length == 0 || mod.bones[b].transform.name.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0 )
							{
								EditorGUILayout.BeginHorizontal("box");

								EditorGUILayout.LabelField(mod.bones[b].transform.name);
								mod.bones[b].weight = setvalue;
								EditorGUILayout.EndHorizontal();
							}
						}
					}

					for ( int b = 0; b < mod.bones.Count; b++ )
					{
						if ( search == "." )
						{
							if ( !mod.bones[b].weight.Equals(1.0f) )
							{
								EditorGUILayout.BeginHorizontal("box");

								EditorGUILayout.LabelField(mod.bones[b].transform.name);
								mod.bones[b].weight = EditorGUILayout.Slider("", mod.bones[b].weight, 0.0f, 1.0f);
								EditorGUILayout.EndHorizontal();
							}
						}
						else
						{
							if ( search.Length == 0 || mod.bones[b].transform.name.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0 )
							{
								EditorGUILayout.BeginHorizontal("box");

								EditorGUILayout.LabelField(mod.bones[b].transform.name);
								mod.bones[b].weight = EditorGUILayout.Slider("", mod.bones[b].weight, 0.0f, 1.0f);
								EditorGUILayout.EndHorizontal();
							}
						}
					}
				}
			}

			if ( GUI.changed )
			{
				mod.CalcWeights();

				EditorUtility.SetDirty(target);
			}
		}
	}
}
