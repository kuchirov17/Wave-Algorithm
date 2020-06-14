
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingField : MonoBehaviour {

	[SerializeField] private Color defaultColor; // цвет клетки по умолчанию
	[SerializeField] private Color pathColor; // подсветка пути
	[SerializeField] private Color cursorColor; // подсветка указателя
	[SerializeField] private LayerMask layerMask; // маска клетки
	[SerializeField] private int width;
	[SerializeField] private int height;
	[SerializeField] [Range(1f, 10f)] private float moveSpeed = 1;
	[SerializeField] [Range(0.1f, 1f)] private float rotationSpeed = 0.25f;
	[SerializeField] private PathfindingNode[] grid;
	private List<PathfindingNode> path;
	private List<TargetPath> pathTarget;
	private PathfindingNode start, end, last;
	private Transform target;
	private int hash, index;
	private bool move;
	private PathfindingNode[,] map;
	private static PathfindingField _inst;

	void Awake()
	{
		_inst = this;
		BuildMap();
	}

	// обновление состояния клеток поля, эту функцию
	// нужно вызывать, если на поле появляются новые объекты или уничтожаются имеющиеся
	public static void UpdateNodeState()
	{
		_inst.UpdateNodeState_inst();
	}

	void BuildMap() // инициализация двумерного массива
	{
		map = new PathfindingNode[width, height];
		int i = 0;
		for(int y = 0; y < height; y++)
		{
			for(int x = 0; x < width; x++)
			{
				grid[i].x = x;
				grid[i].y = y;
				map[x,y] = grid[i];
				i++;
			}
		}

		UpdateNodeState_inst();
	}

	Vector3 NormalizeVector(Vector3 val) // округляем вектор до сотых
	{
		val.x = Mathf.Round(val.x * 100f)/100f;
		val.y = Mathf.Round(val.y * 100f)/100f;
		val.z = Mathf.Round(val.z * 100f)/100f;
		return val;
	}

	struct TargetPath
	{
		public Vector3 direction, position;
	}

	void BuildUnitPath() // создание точек движения и вектора вращения для юнита, на основе найденного пути
	{
		TargetPath p = new TargetPath();
		pathTarget = new List<TargetPath>();
		Vector3 directionLast = (path[0].transform.position - target.position).normalized;

		p.direction = directionLast;
		p.position = NormalizeVector(target.position);
		pathTarget.Add(p);

		if(end.target != null) path.RemoveAt(path.Count-1);

		for(int i = 0; i < path.Count; i++)
		{
			int id = (i+1 < path.Count-1) ? i+1 : path.Count-1;
			Vector3 direction = (path[id].transform.position - path[i].transform.position).normalized;

			if(direction != directionLast)
			{
				p = new TargetPath();
				p.direction = (i < path.Count-1) ? direction : directionLast;
				p.position = NormalizeVector(path[i].transform.position);
				pathTarget.Add(p);
			}

			directionLast = direction;
		}
	}

	void UpdateMove()
	{
		if(!move) return;

		if(NormalizeVector(pathTarget[index].direction) != NormalizeVector(target.forward))
		{
			// анимация поворота юнита по вектору движения
			target.rotation = Quaternion.Lerp(target.rotation, Quaternion.LookRotation(pathTarget[index].direction), rotationSpeed);
		}
		else if(index < pathTarget.Count-1)
		{
			// анимация движения к следующей точке
			target.position = Vector3.MoveTowards(target.position, pathTarget[index+1].position, moveSpeed * Time.deltaTime);
			if(Vector3.Distance(target.position, pathTarget[index+1].position) < 0.1f)
			{
				target.position = pathTarget[index+1].position;
				index++;
			}
		}
		else if(pathTarget.Count > 0 && index == pathTarget.Count-1 && end.target != null && start.isPlayer)
		{
			// если юнита направили на другого юнита, когда он дойдет до него
			// добавляем еще одну точку с текущей позицией и новым направлением, чтобы юнит развернулся "носом" к цели
			TargetPath p = new TargetPath();
			p.direction = (end.target.position - target.position).normalized;
			p.position = target.position;
			pathTarget.Add(p);
			start.isPlayer = false;
			index++;
		}
		else // если юнит достиг цели
		{
			if(end.target == null)
			{
				// анимация ожидания
			}
			else
			{
				// анимация атаки
			}

			UpdateNodeState_inst();
			start.isPlayer = false;
			start = null;
			end = null;
			hash = 0;
			move = false;
		}
	}

	void LateUpdate()
	{
		UpdateMove();
		if(move) return;

		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if(Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
		{
			PathfindingNode node = hit.transform.GetComponent<PathfindingNode>();

			if(node == null || node.isLock && node.target == null) return;

			if(Input.GetMouseButtonDown(0))
			{
				if(node.target != null && end == null)
				{
					if(start != null) start.isPlayer = false;
					start = node;
					node.isPlayer = true;
					node.cost = -1;
					node.mesh.material.color = pathColor;
				}
				else if(end != null)
				{
					target = start.target;
					BuildUnitPath();
					index = 0;
					move = true;
				}
			}
			else if(Input.GetMouseButtonDown(1) && start != null)
			{
				start.isPlayer = false;
				FieldUpdate();
				start = null;
				end = null;
				hash = 0;
			}

			if(hash != node.GetInstanceID())
			{
				if(last != null && !last.isPlayer) last.mesh.material.color = defaultColor;
				if(!node.isPlayer) node.mesh.material.color = cursorColor;

				if(start != null && end != null)
				{
					FieldUpdate();
					end = null;
				}

				if(start != null && node != null)
				{
					end = node;

					path = Pathfinding.Find(start, node, map, width, height);

					if(path == null)
					{
						FieldUpdate();
						start = null;
						end = null;
						hash = 0;
						return;
					}

					for(int i = 0; i < path.Count; i++)
					{
						path[i].mesh.material.color = pathColor;
					}
				}
			}

			last = node;
			hash = node.GetInstanceID();
		}
	}

	void UpdateNodeState_inst() // обновления поля, после совершения действия
	{
		for(int i = 0; i < grid.Length; i++)
		{
			RaycastHit hit; // пускаем луч сверху на клетку, проверяем занята она или нет
			Physics.Raycast(grid[i].transform.position + Vector3.up * 100f, Vector3.down, out hit, 100f, ~layerMask);

			if(hit.collider == null) // пустая клетка
			{
				grid[i].target = null;
				grid[i].isLock = false;
				grid[i].isPlayer = false;
				grid[i].cost = -1; // свободное место
			}
			else if(hit.collider.tag == "Player") // найден юнит
			{
				grid[i].target = hit.transform;
				grid[i].isLock = true;
				grid[i].isPlayer = false;
				grid[i].cost = -2; // препятствие
			}
			else // любой другой объект/препятствие
			{
				grid[i].isLock = true;
				grid[i].cost = -2;
			}

			grid[i].mesh.material.color = defaultColor;
		}
	}

	void FieldUpdate() // обновление поля, перед подсветкой пути
	{
		for(int i = 0; i < grid.Length; i++)
		{
			if(grid[i].isPlayer)
			{
				grid[i].cost = -1;
			}
			else if(grid[i].isLock)
			{
				grid[i].cost = -2;
				grid[i].mesh.material.color = defaultColor;
			}
			else
			{
				grid[i].mesh.material.color = defaultColor;
				grid[i].cost = -1;
			}
		}
	}

	#if UNITY_EDITOR
	[SerializeField] private PathfindingNode sample; // шаблон клетки
	[SerializeField] private float sampleSize = 1; // размер клетки
	public void CreateGrid()
	{
		for(int i = 0; i < grid.Length; i++)
		{
			if(grid[i] != null) DestroyImmediate(grid[i].gameObject);
		}

		grid = new PathfindingNode[width * height];

		float posX = -sampleSize * width / 2f - sampleSize / 2f;
		float posY = sampleSize * height / 2f - sampleSize / 2f;
		float Xreset = posX;
		int z = 0;
		for(int y = 0; y < height; y++)
		{
			posY -= sampleSize;
			for(int x = 0; x < width; x++)
			{
				posX += sampleSize;
				PathfindingNode clone = Instantiate(sample, new Vector3(posX, 0, posY), Quaternion.identity, transform) as PathfindingNode;
				clone.transform.name = "Node-" + z;
				grid[z] = clone;
				z++;
			}
			posX = Xreset;
		}
	}
	#endif
}
