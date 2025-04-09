
using Assets.Scripts.DataStructures;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Assets.Scripts.SampleMind
{
    public class OnlineHillClimbMind : AbstractPathMind
    {
        private List<EnemyBehaviour> enemyBehaviourArray = new List<EnemyBehaviour>();
        Stack<CellInfo> path = new Stack<CellInfo>();// Pila para almacenar el camino resultante que se debe seguir
        List<CellInfo> openList = new List<CellInfo>();// Lista que representa el conjunto abierto en el algoritmo HillClimb
        List<int> deepList = new List<int>();
        Dictionary<CellInfo, CellInfo> visited = new Dictionary<CellInfo, CellInfo>(); // Diccionario para almacenar los predecesores de cada nodo visitado

        private float shorterDistance = 5000f; //Para almacenar el nodo mas cercano a la meta
        private CellInfo shortestNode = null;

        [SerializeField] int k = 1; //Representa el nivel de capas al que vamos a poder llegar

        double totaltime = 0f;  //controladores para calcular el tiempo medio de ejecucción
        int numTime = 0;

        //Método para obtener el movimiento
        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            enemyBehaviourArray = boardInfo.Enemies;
            Stopwatch sw;
            CellInfo move;

            if (enemyBehaviourArray.Count <= 0)
            {
                numTime++;
                sw = Stopwatch.StartNew();// Reiniciamos el temporizador para saber el tiempo de ejecución más adelante

                searchPathHillClimb(boardInfo, currentPos, goals[0]);//Llamar a la función que establece el recorrido a seguir
                UnityEngine.Debug.Log($"LONGITUD DEL PATH: {path.Count}");//Miramos la distancia en funcion de las casillas hasta neustro objetivo
                UnityEngine.Debug.Log($"TIME ELAPSED: {sw.Elapsed}");// Imprimir el tiempo transcurrido              
                UnityEngine.Debug.Log($"NUMERO DE NODOS2: {openList.Count}");// Imprimir el número de nodos dentro del path a seguir

            }
            else
            {
                numTime++;
                sw = Stopwatch.StartNew();// Reiniciamos el temporizador para saber el tiempo de ejecución más adelante

                searchPathHillClimb(boardInfo, currentPos, enemyBehaviourArray[0].CurrentPosition());//Llamar a la función que establece el recorrido a seguir
                UnityEngine.Debug.Log($"LONGITUD DEL PATH: {path.Count}");//Miramos la distancia en funcion de las casillas hasta neustro objetivo
                UnityEngine.Debug.Log($"TIME ELAPSED: {sw.Elapsed}");// Imprimir el tiempo transcurrido
                UnityEngine.Debug.Log($"NUMERO DE NODOS1: {openList.Count}");// Imprimir el número de nodos dentro del path a seguir
            }

            totaltime += sw.ElapsedMilliseconds;
            UnityEngine.Debug.Log("MediaTotal: " + totaltime / numTime); //Imprimir el tiempo medio

            if (path.Count > 0)
            {
                move = path.Peek();

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

        // Método para realizar la búsqueda del camino utilizando el algoritmo HillClimb
        void searchPathHillClimb(BoardInfo boardInfo, CellInfo startPos, CellInfo targetPos)
        {
            openList.Clear(); // Limpiar la cola "abierta" para reempezar la búsqueda
            visited.Clear(); // Limpiar el diccionario de predecesores
            deepList.Clear(); // Limpiar la lista de profundidad

            openList.Add(startPos); // Se inicializa la cola "abierta" desde el nodo de inicio

            deepList.Add(0); // Se inicializa la cola de profundidad del algoritmo desde el nodo de inicio

            shorterDistance = 5000f; // Establecer una distancia suficientemente alta para que siempre varie con la primera casilla

            // Bucle principal del algoritmo HillClimb si la lista de abiertos no está vacía

            while (openList.Count > 0 && deepList[0] != k)
            {
                    CellInfo current = openList[0]; // Obtener y quitar el primer nodo de la cola 'openList' para marcarlo como el nodo actual

                    if (current == targetPos) // Si se ha encontrado el nodo meta
                    {
                        retracePath(startPos, targetPos); // Llamar a la función que reconstruye el camino desde la posición inicial hasta el nodo meta
                        return; // Salir del bucle
                    }

                    // Explorar los nodos vecinos del nodo actual
                    foreach (CellInfo next in current.WalkableNeighbours(boardInfo))
                    {
                        if (next != null && !visited.ContainsKey(next)) // Si el nodo vecino no ha sido visitado
                        {
                            if (deepList[0] < k) // Si la profundidad sigue siendo menor que k
                            {
                                openList.Add(next); // Añadir el nodo vecino a la cola 'openList' que funciona como la lista "abierta"
                                visited[next] = current; // Almacenar el predecesor del nodo vecino
                                deepList.Add(deepList[0] + 1); // Sumamos uno en cada nueva generacion
                            }

                        }
                    }
                    openList.Remove(current); // sacar el nodo actual de la lista
                    deepList.RemoveAt(0); // sacar el nodo de la lista de profundidad

            }

            foreach(CellInfo current in openList) //Buscar en la lista de la ultima generacion de nodos el mas cercano
            {
                if (getDistanceToEndPoint(current, targetPos) < shorterDistance)
                {
                    shorterDistance = getDistanceToEndPoint(current, targetPos); // se llama a la funcion que calcula la distancia de Manhattan
                    shortestNode = current;
                }
            }

            retracePath(startPos, shortestNode); // Llamar a la función que reconstruye el camino desde la posición inicial hasta el nodo meta
            return; // Salir del bucle
        }
        
        // Método para reconstruir el camino desde la posición inicial hasta el nodo meta
        void retracePath(CellInfo startNode, CellInfo endNode)
        {
            path.Clear();
            CellInfo currentNode = endNode; // Comenzar desde el nodo meta

            // Bucle para retroceder desde el nodo meta hasta la posición inicial
            while (currentNode != startNode)
            {
                path.Push(currentNode); // Añadir el nodo actual a la pila 'newPath'
                currentNode = visited[currentNode]; // Obtener el predecesor del nodo actual en el camino
            }
        }

        // Método para calcular la distancia entre dos celdas en el tablero (distancia de Manhattan)
        float getDistanceToEndPoint(CellInfo A, CellInfo B)
        {
            float distRows = Mathf.Abs(A.RowId - B.RowId);
            float distColumns = Mathf.Abs(A.ColumnId - B.ColumnId);

            return distRows + distColumns;
        }

    }
}
