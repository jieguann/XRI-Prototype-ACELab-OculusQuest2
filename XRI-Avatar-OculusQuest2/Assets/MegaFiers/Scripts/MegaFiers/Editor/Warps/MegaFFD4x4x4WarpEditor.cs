using UnityEditor;

namespace MegaFiers
{
	[CustomEditor(typeof(MegaFFD4x4x4Warp))]
	public class MegaFFD4x4x4WarpEditor : MegaFFDWarpEditor
	{
		[MenuItem("GameObject/Create Other/MegaFiers/Warps/FFD 4x4x4")]
		static void CreateFFDWarp444()
		{
			CreateFFDWarp("FFD 4x4x4", typeof(MegaFFD4x4x4Warp));
		}

		public override string GetHelpString() { return "FFD4x4x4 Warp by Chris West"; }
	}
}