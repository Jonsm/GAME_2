using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[ExecuteInEditMode]
[CustomEditor (typeof(TileCollisionMono))]
public class TileCollisionEditor : Editor {
	static float alphaThreshold = .4f;

	/*
	 * GUI
	 */

	public override void OnInspectorGUI () {
		TileCollisionMono mono = (TileCollisionMono) target;

		mono.oneWayCollision = GUILayout.Toggle(mono.oneWayCollision, "One Way Collision");
		mono.upAngle1 = EditorGUILayout.FloatField ("Up angle", mono.upAngle1);
		mono.upAngle2 = EditorGUILayout.FloatField ("Up angle", mono.upAngle2);
		if (GUILayout.Button ("GENERATE COLLISION")) GenerateCollision (mono.GetGameObject());
	}

	/*
	 * HELPER METHODS
	 */

	void GenerateCollision (GameObject obj) {
		TileCollisionMono mono = (TileCollisionMono) target;

		Rect extents = GetExtents (obj);
		bool[][] totalPixels = 
			new bool[Mathf.RoundToInt(extents.width * Constants.pixelsPerUnit)][];
		for (int i = 0; i < totalPixels.Length; i++) {
			totalPixels[i] = 
				new bool[Mathf.RoundToInt(extents.height * Constants.pixelsPerUnit)];
		}

		foreach (Transform child in obj.transform) {
			AddTileToArray(child.gameObject, extents, totalPixels);
		}

		MarchingSquares marchingSquares = (MarchingSquares) ScriptableObject.CreateInstance (typeof(MarchingSquares));
		List<List<Vector2>> paths = marchingSquares.Make (totalPixels, mono.oneWayCollision, mono.upAngle1, mono.upAngle2);
		TransformPoints (extents, paths);
		MakeColliders (paths, obj);

		ScriptableObject.DestroyImmediate (marchingSquares);
	}

	/*
	 * HELPER METHODS
	 */

	void MakeColliders(List<List<Vector2>> paths, GameObject obj) {
		Collider2D[] colliders = obj.GetComponents<Collider2D> ();
		foreach (Collider2D collider in colliders) DestroyImmediate (collider);

		foreach (List<Vector2> path in paths) {
			EdgeCollider2D collider2 = obj.AddComponent<EdgeCollider2D> () as EdgeCollider2D;
			collider2.points = path.ToArray ();
		}
	}

	void MakeCollidersOneWay(List<List<Vector2>> paths, GameObject obj) {

	}

	void AddTileToArray (GameObject tile, Rect extents, bool[][] arr) {
		int pixelBeginX = Mathf.RoundToInt((tile.transform.position.x 
								- (float)Constants.tileSize / (float) Constants.pixelsPerUnit / 2f
								- extents.x) * (float) Constants.pixelsPerUnit);
		int pixelBeginY = Mathf.RoundToInt((tile.transform.position.y 
								- (float) Constants.tileSize / (float) Constants.pixelsPerUnit / 2f
								- extents.y) * (float) Constants.pixelsPerUnit);

		if (tile.GetComponent<SpriteRenderer> () != null) {
			SpriteRenderer renderer = tile.GetComponent<SpriteRenderer> () as SpriteRenderer;
			int spriteBeginX = (int) renderer.sprite.rect.x;
			int spriteBeginY = (int) renderer.sprite.rect.y;

			Color[] pixels = renderer.sprite.texture.GetPixels(
				spriteBeginX, spriteBeginY, Constants.tileSize, Constants.tileSize);

			for (int i = 0; i < Constants.tileSize; i++) {
				for (int j = 0; j < Constants.tileSize; j++) {
					if (pixels[i*Constants.tileSize + j].a > alphaThreshold)
						arr[pixelBeginX + j][pixelBeginY + i] = true;
				}
			}
		}
	}

	//not normal rect, starts at lower left corner
	Rect GetExtents(GameObject obj) {
		Rect extents = new Rect (0, 0, 0, 0);
		Transform firstChild = obj.transform.GetChild(0);
		extents.x = firstChild.position.x;
		extents.y = firstChild.position.y;
		extents.width = firstChild.position.x;
		extents.height = firstChild.position.y;

		foreach (Transform child in obj.transform) {
			if (child.position.x < extents.x) extents.x = child.position.x;
			if (child.position.x > extents.width) extents.width = child.position.x;
			if (child.position.y < extents.y) extents.y = child.position.y;
			if (child.position.y > extents.height) extents.height = child.position.y;
		}

		extents.width = extents.width - extents.x + (float)Constants.tileSize / Constants.pixelsPerUnit;
		extents.height = extents.height - extents.y + (float)Constants.tileSize / Constants.pixelsPerUnit;
		extents.x -= (float)Constants.tileSize / Constants.pixelsPerUnit / 2f;
		extents.y -= (float)Constants.tileSize / Constants.pixelsPerUnit / 2f;

		return extents;
	}

	void TransformPoints (Rect extents, List<List<Vector2>> paths) {
		Vector2 start = new Vector2 (extents.x, extents.y);

		foreach (List<Vector2> path in paths) {
			for (int i = 0; i < path.Count; i++) {
				path [i] = path [i] / Constants.pixelsPerUnit + start;
			}
		}
	}
}
