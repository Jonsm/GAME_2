using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[CustomEditor (typeof(TileEditorMono))]
public class TileEditor : Editor {
	public static Sprite currentTile;

	/*
	 * GUI
	 */

	public override void OnInspectorGUI () {
		TileEditorMono mono = (TileEditorMono)target;
		if (mono.tiles == null)
			UpdateDictionary ();
		
		mono.draw = EditorGUILayout.Toggle ("DRAW", mono.draw);

		mono.drawOrder = EditorGUILayout.IntField ("Draw Order", mono.drawOrder);
		if (mono.drawOrder != mono.oldDrawOrder) {
			mono.oldDrawOrder = mono.drawOrder;
			SetDrawOrder ();
		}

		if (GUILayout.Button ("CLEAR")) Clear ();
	}

	public void OnSceneGUI () {
		TileEditorMono mono = (TileEditorMono)target;
		if (mono.tiles == null)
			UpdateDictionary ();

		if (mono.draw) {
			Event current = Event.current;
			int controlID = GUIUtility.GetControlID (FocusType.Passive);
			HandleUtility.AddDefaultControl(controlID);
			EventType type = current.GetTypeForControl (controlID);

			//update state
			if (type == EventType.KeyDown && current.keyCode == KeyCode.D) {
				mono.deleting = true;
			}
			if (type == EventType.KeyUp && current.keyCode == KeyCode.D) {
				mono.deleting = false;
			}
			
			if (current.button == 0 && type == EventType.MouseDown) {
				Selection.activeGameObject = mono.GetGameObject ();
				mono.drawing = true;
				GUIUtility.hotControl = 0;
				current.Use ();
			} 
			if (mono.drawing && type == EventType.MouseUp) {
				mono.drawing = false;
				GUIUtility.hotControl = controlID;
				current.Use ();
			}
		
			if (mono.drawing) {
				Selection.activeGameObject = mono.GetGameObject ();

				Vector2 mousePos = current.mousePosition;
				mousePos.y = Camera.current.pixelHeight - mousePos.y;
				Vector3 pos = Camera.current.ScreenPointToRay(mousePos).origin;

				if (mono.deleting) EraseAtPos (pos);
				else if (currentTile != null) AddTileAtPos (pos);
			}	
		}
	}

	/*
	 * EDITOR FUNCTIONS
	 */

	void AddTileAtPos (Vector3 pos) {
		TileEditorMono mono = (TileEditorMono)target;
		Vector2 roundedPos = RoundPos (pos);

		if (!mono.tiles.ContainsKey (roundedPos) || 
		    ((SpriteRenderer) mono.tiles [roundedPos].GetComponent(typeof(SpriteRenderer))).sprite != currentTile) {
			GameObject newSprite = new GameObject ();
			string name = "Tile " + roundedPos.ToString ();
			newSprite.name = name;
			newSprite.AddComponent (typeof(SpriteRenderer));
			SpriteRenderer spriteRenderer = newSprite.GetComponent <SpriteRenderer> () as SpriteRenderer;
			spriteRenderer.sprite = currentTile;
			spriteRenderer.sortingOrder = mono.drawOrder;

			GameObject sceneSprite = Instantiate (newSprite, roundedPos, Quaternion.identity) as GameObject;
			sceneSprite.transform.parent = mono.GetGameObject ().transform;
			DestroyImmediate (newSprite);

			if (mono.tiles.ContainsKey (roundedPos)) DestroyImmediate (mono.tiles [roundedPos]);
			mono.tiles [roundedPos] = sceneSprite;
		}
	}

	void EraseAtPos (Vector3 pos) {
		TileEditorMono mono = (TileEditorMono)target;
		Vector2 roundedPos = RoundPos (pos);

		if (mono.tiles.ContainsKey (roundedPos)) {
			DestroyImmediate (mono.tiles [roundedPos]);
			mono.tiles.Remove (roundedPos);
		}
	}

	void Clear () {
		TileEditorMono mono = (TileEditorMono)target;

		List <Vector2> toRemove = new List <Vector2> ();
		foreach (Vector2 v in mono.tiles.Keys) toRemove.Add (v);
		foreach (Vector2 v in toRemove) {
			DestroyImmediate (mono.tiles [v]);
			mono.tiles.Remove (v);
		}
	}

	void SetDrawOrder () {
		TileEditorMono mono = (TileEditorMono)target;

		foreach (GameObject obj in mono.tiles.Values) {
			SpriteRenderer spriteRenderer = obj.GetComponent <SpriteRenderer> () as SpriteRenderer;
			spriteRenderer.sortingOrder = mono.drawOrder;
		}
	}

	/*
	 * UTILITY
	 */

	Vector2 RoundPos (Vector3 pos) {
		float inc = (float) Constants.tileSize / Constants.pixelsPerUnit;
		float roundedX = Mathf.Round (pos.x / inc) * inc;
		float roundedY = Mathf.Round (pos.y / inc) * inc;
		return new Vector2 (roundedX, roundedY);
	}

	void UpdateDictionary () {
		TileEditorMono mono = (TileEditorMono)target;
		mono.tiles = new Dictionary <Vector2, GameObject> ();
		
		foreach (Transform child in mono.GetGameObject ().transform) {
			mono.tiles [RoundPos (child.transform.position)] = child.gameObject;
		}
	}
}
