

using System.Collections;
using UnityEngine;

public class PathfindingNode : MonoBehaviour {

	public int x { get; set; }
	public int y { get; set; }
	public int cost { get; set; }
	public Transform target { get; set; }
	public bool isLock { get; set; }
	public bool isPlayer { get; set; }
	public MeshRenderer mesh; // указываем меш клетки

}
