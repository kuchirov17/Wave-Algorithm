using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Pathfinding {

	public static List<PathfindingNode> Find(PathfindingNode start, PathfindingNode end, PathfindingNode[,] map, int width, int height)
	{
		int x, y, cost = 0, step = 0;

		map[end.x, end.y].cost = 0; // начало поиска с точки назначения

		if(start.x - 1 >= 0)
		{
			if(map[start.x-1, start.y].cost == -2) step++;
		}
		else step++;

		if(start.y-1 >= 0)
		{
			if(map[start.x, start.y-1].cost == -2) step++;
		}
		else step++;

		if(start.x+1 < width)
		{
			if(map[start.x+1, start.y].cost == -2) step++;
		}
		else step++;

		if(start.y+1 < height)
		{
			if(map[start.x, start.y+1].cost == -2) step++;
		}
		else step++;

		// проверка на доступность (например, юнит окружен)
		if(step == 4) return null; else step = 0;

		while(true) // цикл поиска
		{
			for(y = 0; y < height; y++)
			{
				for(x = 0; x < width; x++)
				{
					if(map[x, y].cost == step) // находим клетку, соответствующую текущему шагу
					{
						if(x-1 >= 0) // если не выходим за границы массива
						if(map[x-1, y].cost == -1) // если клетка еще не проверялась
						{
							cost = step + 1; // сохраняем проходимость клетки
							map[x-1, y].cost = cost; // назначаем проходимость
						}

						if(y-1 >= 0)
						if(map[x, y-1].cost == -1)
						{
							cost = step + 1;
							map[x, y-1].cost = cost;
						}

						if(x+1 < width)
						if(map[x+1, y].cost == -1)
						{
							cost = step + 1;
							map[x+1, y].cost = cost;
						}

						if(y+1 < height)
						if(map[x, y+1].cost == -1)
						{
							cost = step + 1;
							map[x, y+1].cost = cost;
						}
					}
				}
			}

			step++; // следующий шаг/волна

			if(map[start.x, start.y].cost != -1) break; // если путь найден, выходим из цикла
			if(step != cost || step > width * height) return null; // если путь найти невозможно, возврат
		}

		List<PathfindingNode> result = new List<PathfindingNode>(); // массив пути

		// начало поиска со старта
		x = start.x;
		y = start.y;

		step = map[x, y].cost; // определяем базовую проходимость

		while(x != end.x || y != end.y) // прокладка пути
		{
			if(x-1 >= 0 && y-1 >= 0) // если не выходим за границы массива
			if(map[x-1, y-1].cost >= 0) // если клетка проходима
			if(map[x-1, y-1].cost < step) // если эта проходимость меньше, базовой проходимости
			{
				step = map[x-1, y-1].cost; // новая базовая проходимость
				result.Add(map[x-1, y-1]); // добавляем клетку в массив пути
				x--;
				y--;
				continue; // переходим на следующий цикл
			}

			if(y-1 >= 0 && x+1 < width)
			if(map[x+1, y-1].cost >= 0)
			if(map[x+1, y-1].cost < step)
			{
				step = map[x+1, y-1].cost;
				result.Add(map[x+1, y-1]);
				x++;
				y--;
				continue;
			}

			if(y+1 < height && x+1 < width)
			if(map[x+1, y+1].cost >= 0)
			if(map[x+1, y+1].cost < step)
			{
				step = map[x+1, y+1].cost;
				result.Add(map[x+1, y+1]);
				x++;
				y++;
				continue;
			}

			if(y+1 < height && x-1 >= 0)
			if(map[x-1, y+1].cost >= 0)
			if(map[x-1, y+1].cost < step)
			{
				step = map[x-1, y+1].cost;
				result.Add(map[x-1, y+1]);
				x--;
				y++;
				continue;
			}

			if(x-1 >= 0)
			if(map[x-1, y].cost >= 0)
			if(map[x-1, y].cost < step)
			{
				step = map[x-1, y].cost;
				result.Add(map[x-1, y]);
				x--;
				continue;
			}

			if(y-1 >= 0)
			if(map[x, y-1].cost >= 0)
			if(map[x, y-1].cost < step)
			{
				step = map[x, y-1].cost;
				result.Add(map[x, y-1]);
				y--;
				continue;
			}

			if(x+1 < width)
			if(map[x+1, y].cost >= 0)
			if(map[x+1, y].cost < step)
			{
				step = map[x+1, y].cost;
				result.Add(map[x+1, y]);
				x++;
				continue;
			}

			if(y+1 < height)
			if(map[x, y+1].cost >= 0)
			if(map[x, y+1].cost < step)
			{
				step = map[x, y+1].cost;
				result.Add(map[x, y+1]);
				y++;
				continue;
			}

			return null; // текущая клетка не прошла проверку, ошибка пути, возврат
		}

		return result; // возвращаем найденный маршрут
	}
}
