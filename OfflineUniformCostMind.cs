//Carol �lvarez
//Jose Antonio Reyes
//Ignacio Tapia
//Alicia Touris

using Assets.Scripts.DataStructures;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Assets.Scripts.SampleMind
{
    public class OfflineUniformCostMind : AbstractPathMind
    {
        Stack<CellInfo> path = new Stack<CellInfo>();// Pila para almacenar el camino resultante que se debe seguir
        List<float> listaCoste = new List<float>(); //lista de costes
        List<CellInfo> openList = new List<CellInfo>();// Lista que representa el conjunto abierto en el algoritmo BFS
        Dictionary<CellInfo, CellInfo> visited = new Dictionary<CellInfo, CellInfo>(); // Diccionario para almacenar los predecesores de cada nodo visitado

        bool pathSearched = false;//Variable para saber si se ha buscado el camino a seguir

        //M�todo para obtener el movimiento
        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            if (!pathSearched)//Si a�n no se ha buscado el camino a seguir
            {
                UnityEngine.Debug.Log($"LONGITUD DEL PATH: {getDistanceToEndPoint(currentPos, goals[0])}");//Llamamos a la funci�n que nos da la distandia de Manhattan

                Stopwatch sw = Stopwatch.StartNew();// Reiniciamos el temporizador para saber el tiempo de ejecuci�n m�s adelante

                searchPathUniformCost(boardInfo, currentPos, goals[0]);//Llamar a la funci�n que establese el recorrido a seguir
                UnityEngine.Debug.Log($"TIME ELAPSED: {sw.Elapsed}");// Imprimir el tiempo transcurrido
                UnityEngine.Debug.Log($"NUMERO DE NODOS: {path.Count}");// Imprimir el n�mero de nodos dentro del path a seguir

                pathSearched = true;// Establecer que la b�squeda del camino ha sido realizada
            }
            if (path.Count > 0)// Extraer el siguiente movimiento de la pila 'path' y devolverlo como direcci�n de movimiento
            {
                CellInfo move = path.Pop();

                if (move.RowId > currentPos.RowId)
                    return Locomotion.MoveDirection.Up;
                else if (move.RowId < currentPos.RowId)
                    return Locomotion.MoveDirection.Down;
                else if (move.ColumnId > currentPos.ColumnId)
                    return Locomotion.MoveDirection.Right;
                else if (move.ColumnId < currentPos.ColumnId)
                    return Locomotion.MoveDirection.Left;
            }

            return Locomotion.MoveDirection.None;// Si no hay movimientos disponibles, no se devuelve movimiento
        }

        // M�todo para realizar la b�squeda del camino utilizando el algoritmo BFS
        void searchPathUniformCost(BoardInfo boardInfo, CellInfo startPos, CellInfo targetPos)
        {
            openList.Clear(); // Limpiar la cola "abierta" para reempezar la b�squeda
            visited.Clear(); // Limpiar el diccionario de predecesores
            openList.Add(startPos); // Se inicializa la cola "abierta" desde el nodo de inicio
            listaCoste.Add(0);

            // Bucle principal del algoritmo UniformCost si la lista de abiertos no est� vac�a
            while (openList.Count > 0)
            {
                CellInfo current = openList[0]; // Obtener y quitar el primer nodo de la cola 'openList' para marcarlo como el nodo actual

                if (current == targetPos) // Si se ha encontrado el nodo meta
                {
                    retracePath(startPos, targetPos); // Llamar a la funci�n que reconstruye el camino desde la posici�n inicial hasta el nodo meta
                    return; // Salir del bucle
                }

                // Explorar los nodos vecinos del nodo actual
                foreach (CellInfo next in current.WalkableNeighbours(boardInfo))
                {
                    if (next != null && !visited.ContainsKey(next)) // Si el nodo vecino no ha sido visitado
                    {
                        for (int i = openList.Count -1; i >= 0; i--)
                        {
                            if (i == 0)
                            {
                                openList.Insert(i, next);
                                listaCoste.Insert(i, listaCoste[0] + current.WalkCost);
                                visited[next] = current;
                                break; // Exit the loop after inserting the node
                            }
                            if (listaCoste[i] <= listaCoste[0] + current.WalkCost)
                            {
                                openList.Insert(i, next);
                                listaCoste.Insert(i, listaCoste[0] + current.WalkCost);
                                visited[next] = current;
                                break; // Exit the loop after inserting the node
                            }
                        }
                    }
                }
                openList.Remove(current);
                listaCoste.RemoveAt(0);
            }
        }

        // M�todo para reconstruir el camino desde la posici�n inicial hasta el nodo meta
        void retracePath(CellInfo startNode, CellInfo endNode)
        {
            CellInfo currentNode = endNode; // Comenzar desde el nodo meta

            // Bucle para retroceder desde el nodo meta hasta la posici�n inicial
            while (currentNode != startNode)
            {
                path.Push(currentNode); // A�adir el nodo actual a la pila 'newPath'
                currentNode = visited[currentNode]; // Obtener el predecesor del nodo actual en el camino
            }
        }

        // M�todo para calcular la distancia entre dos celdas en el tablero (distancia de Manhattan)
        float getDistanceToEndPoint(CellInfo A, CellInfo B)
        {
            float distRows = Mathf.Abs(A.RowId - B.RowId);
            float distColumns = Mathf.Abs(A.ColumnId - B.ColumnId);

            return distRows + distColumns;
        }
    }
}

