
#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(PathfindingField))]
public class PathfindingEditor : Editor {

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		PathfindingField e = (PathfindingField)target;
		if(GUILayout.Button("Создать / Обновить игровое поле")) e.CreateGrid();
	}
}
#endif