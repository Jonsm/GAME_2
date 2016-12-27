using UnityEngine;
using UnityEditor;
using System.Collections;

public class TileWindow : EditorWindow {
	static Texture2D tiles = null;
	static int offset = 100;
	static TileWindow window;

	[MenuItem ("Window/Tile Editor &t")]
	public static void  ShowWindow () {
		if (!window) window = ScriptableObject.CreateInstance <TileWindow> () as TileWindow;
		window.ShowUtility ();
	}
	
	void OnGUI () {
		tiles = (Texture2D) EditorGUILayout.ObjectField ("Tileset", tiles, typeof(Texture2D), true);
		if (tiles) {
			string path = AssetDatabase.GetAssetPath (tiles);
			Object [] sprites = AssetDatabase.LoadAllAssetsAtPath (path);

			int x = 0;
			int y = 0;
			int size = Constants.tileSize;
			int dim = tiles.width / Constants.tileSize;
			float inc = 1.0f / dim;

			foreach (Object sprite in sprites) {
				if (sprite == sprites[0]) continue;
				Rect coord = new Rect (offset + x * size, offset + y * size, size, size);
				Rect textureCoord = new Rect (x * inc, (dim - y - 1) * inc, inc, inc);

				if (GUI.Button (coord, "")) {
					TileEditor.currentTile = (Sprite) sprite;
					window.Close ();
				}
				GUI.DrawTextureWithTexCoords (coord, tiles, textureCoord);

				x++;
				if (x == dim) {
					x = 0;
					y++;
				}
			}
		}
	}
}
