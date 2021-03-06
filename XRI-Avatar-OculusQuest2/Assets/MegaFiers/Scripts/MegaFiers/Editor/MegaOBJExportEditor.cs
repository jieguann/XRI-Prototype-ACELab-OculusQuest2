
using UnityEngine;
using UnityEditor;

namespace MegaFiers
{
	[CustomEditor(typeof(MegaOBJExport))]
	public class MegaOBJExportEditor : Editor
	{
		[MenuItem("GameObject/Mega Export OBJ File")]
		static void ExportOBJFile()
		{
			if ( Selection.activeGameObject )
			{
				MeshFilter mf = (MeshFilter)Selection.activeGameObject.GetComponent<MeshFilter>();

				if ( mf )
				{
					string path = EditorUtility.SaveFilePanel("OBJ Export Filename", "", mf.gameObject.name + ".obj", "obj");

					if ( path.Length != 0 )
						MegaOBJExport.MeshToFile(mf, path);
				}
			}
		}

		public override void OnInspectorGUI()
		{
			MegaOBJExport mod = (MegaOBJExport)target;

			DrawDefaultInspector();

			if ( GUILayout.Button("Set Path") )
			{
				string path = EditorUtility.SaveFolderPanel("OBJ Export Path", mod.path, "Path");
				if ( path.Length != 0 )
					mod.path = path;
			}
		}
	}
}