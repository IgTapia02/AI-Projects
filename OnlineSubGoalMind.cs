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
    public class OnlineSubGoalMind : AbstractPathMind
    {
        private List<EnemyBehaviour> enemyBehaviourArray = new List<EnemyBehaviour>(); // Lista de enemigos
        Stack<CellInfo> path = new Stack<CellInfo>();// Pila para almacenar el camino resultante que se debe seguir
        List<CellInfo> openList = new List<CellInfo>();// Lista que representa el conjunto abierto en el algoritmo HillClimb
        Dictionary<CellInfo, CellInfo> visited = new Dictionary<CellInfo, CellInfo>(); // Diccionario para almacenar los predecesores de cada nodo visitado

        int openNodesInMove = 0; //Controlador de los nodos abiertos en cada iteraccion del bucle

        double totaltime = 0f;  //controladores para calcular el tiempo medio de ejecucci�n
        int numTime = 0;

        //M�todo para obtener el movimiento
        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            enemyBehaviourArray = boardInfo.Enemies;
            Stopwatch sw;
            CellInfo move;

            if (enemyBehaviourArray.Count <= 0)
            {
                numTime++;
                sw = Stopwatch.StartNew();// Reiniciamos el temporizador para saber el tiempo de ejecuci�n m�s adelante

                searchSubGoal(boardInfo, currentPos, goals[0]);//Llamar a la funci�n que establece el recorrido a seguir
                UnityEngine.Debug.Log($"LONGITUD DEL PATH: {path.Count}");//Llamamos a la funci�n que nos da la distandia de Manhattan
                UnityEngine.Debug.Log($"TIME ELAPSED: {sw.Elapsed}");// Imprimir el tiempo transcurrido
                UnityEngine.Debug.Log($"NUMERO DE NODOS2: {openNodesInMove}");// Imprimir el n�mero de nodos dentro del path a seguir

            }
            else
            {
                numTime++;
                sw = Stopwatch.StartNew();// Reiniciamos el temporizador para saber el tiempo de ejecuci�n m�s adelante

                searchSubGoal(boardInfo, currentPos, enemyBehaviourArray[0].CurrentPosition());//Llamar a la funci�n que establece el recorrido a seguir
                UnityEngine.Debug.Log($"LONGITUD DEL PATH: {path.Count}");//Llamamos a la funci�n que nos da la distandia de Manhattan
                UnityEngine.Debug.Log($"TIME ELAPSED: {sw.Elapsed}");// Imprimir el tiempo transcurrido               
                UnityEngine.Debug.Log($"NUMERO DE NODOS1: {openNodesInMove}");// Imprimir el n�mero de nodos dentro del path a seguir

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

        // M�todo para realizar la b�squeda del camino utilizando el algoritmo SubGoal
        void searchSubGoal(BoardInfo boardInfo, CellInfo startPos, CellInfo targetPos)
        {
            openNodesInMove = 0;
            openList.Clear(); // Limpiar la cola "abierta" para reempezar la b�squeda
            visited.Clear(); // Limpiar el diccionario de predecesores

            openList.Add(startPos); // Se inicializa la cola "abierta" desde el nodo de inicio

            // Bucle principal del algoritmo SubGoal si la lista de abiertos no est� vac�a
            while (openList.Count > 0)
            {
                CellInfo current = openList[0]; // Obtener el primer nodo de la cola 'openList' para marcarlo como el nodo actual

                // Explorar los nodos vecinos del nodo actual
                foreach (CellInfo next in current.WalkableNeighbours(boardInfo))
                {
                     if (next != null && !visited.ContainsKey(next)) // Si el nodo vecino no ha sido visitado
                    {
                        openList.Add(next); // A�adir el nodo vecino a la cola 'openList' que funciona como la lista "abierta"
                        openNodesInMove++;
                        visited[next] = current; // Almacenar el predecesor del nodo vecino
                        if(next == targetPos) //Si el nodo vecino es el objetivo
                        {
                            retracePath(startPos, next); // Llamar a la funci�n que reconstruye el camino desde la posici�n inicial hasta el nodo meta
                            return; // Salir del bucle
                        }
                    }
                }
                    openList.Remove(current); //Se saca el nodo actual de la lista
            }
        }

        // M�todo para reconstruir el camino desde la posici�n inicial hasta el nodo meta
        void retracePath(CellInfo startNode, CellInfo endNode)
        {
            path.Clear();
            CellInfo currentNode = endNode; // Comenzar desde el nodo meta

            // Bucle para retroceder desde el nodo meta hasta la posici�n inicial
            while (currentNode != startNode)
            {
                path.Push(currentNode); // A�adir el nodo actual a la pila 'path'
                currentNode = visited[currentNode]; // Obtener el predecesor del nodo actual en el camino
            }
        }

    }
}
