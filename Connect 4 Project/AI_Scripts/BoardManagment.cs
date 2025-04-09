using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace ConnectFour
{
    public static class BoardManagment
    {
        const int WIN_SCORE = 100000;
        const int THREE_SCORE = 2000;
        const int TWO_SCORE = 100;

        // Obtener el GameController una sola vez
        static GameController gameController = GameObject.FindFirstObjectByType<GameController>();

        static int EvaluateLine(int[] line, int player)
        {
            int score = 0;

            for (int i = 0; i <= line.Length - 4; i++)
            {
                int[] subline = line.Skip(i).Take(4).ToArray();
                int playerCount = subline.Count(cell => cell == player);
                int opponentCount = subline.Count(cell => cell == -player);
                int emptyCount = subline.Count(cell => cell == 0);

                if (playerCount == 4) return WIN_SCORE; // Victoria inmediata.
                if (opponentCount == 4) return -WIN_SCORE; // Derrota inmediata.

                // Asignar puntuaciones según el progreso hacia el 4 en raya.
                if (playerCount == 3 && emptyCount == 1 && opponentCount == 0)
                    score += THREE_SCORE;
                if (playerCount == 2 && emptyCount == 2 && opponentCount == 0)
                    score += TWO_SCORE;

                // Penalización para el oponente
                if (opponentCount == 3 && emptyCount == 1 && playerCount == 0)
                    score -= THREE_SCORE;
                if (opponentCount == 2 && emptyCount == 2 && playerCount == 0)
                    score -= TWO_SCORE;
            }
            return score;
        }

        public static int EvaluateBoard(int[,] board, int player)
        {
            int score = 0;
            object lockObj = new object();

            // Evaluar filas
            Parallel.For(0, board.GetLength(0), row =>
            {
                int[] line = GetRow(board, row);
                int multi = board.GetLength(0) - Math.Abs(row - board.GetLength(0) / 2); // Priorizar filas más cercanas al centro
                int localScore = EvaluateLine(line, player) * multi;

                lock (lockObj)
                {
                    score += localScore;
                }
            });

            // Evaluar columnas
            Parallel.For(0, board.GetLength(1), col =>
            {
                int[] line = GetColumn(board, col);
                int localScore = EvaluateLine(line, player);

                lock (lockObj)
                {
                    score += localScore;
                }
            });

            // Evaluar diagonales principales (↘)
            for (int row = 0; row < board.GetLength(0) - 3; row++)
            {
                for (int col = 0; col < board.GetLength(1) - 3; col++)
                {
                    int[] line = GetDiagonal(board, row, col, 1, 1);
                    score += EvaluateLine(line, player);
                }
            }

            // Evaluar diagonales inversas (↙)
            for (int row = 0; row < board.GetLength(0) - 3; row++)
            {
                for (int col = 3; col < board.GetLength(1); col++)
                {
                    int[] line = GetDiagonal(board, row, col, 1, -1);
                    score += EvaluateLine(line, player);
                }
            }

            return score;
        }

        // Obtener una fila específica del tablero.
        static int[] GetRow(int[,] board, int row)
        {
            return Enumerable.Range(0, board.GetLength(1)).Select(col => board[row, col]).ToArray();
        }

        // Obtener una columna específica del tablero.
        static int[] GetColumn(int[,] board, int col)
        {
            return Enumerable.Range(0, board.GetLength(0)).Select(row => board[row, col]).ToArray();
        }

        // Obtener una diagonal desde una posición inicial con un paso (↘ o ↙).
        static int[] GetDiagonal(int[,] board, int startRow, int startCol, int rowStep, int colStep)
        {
            // Check for out-of-bound access
            if (startRow + (3 * rowStep) >= board.GetLength(0) ||
                startRow + (3 * rowStep) < 0 ||
                startCol + (3 * colStep) >= board.GetLength(1) ||
                startCol + (3 * colStep) < 0)
            {
                return new int[0]; // Return an empty line to ignore invalid diagonals.
            }

            int[] line = new int[4];
            for (int i = 0; i < 4; i++)
            {
                line[i] = board[startRow + i * rowStep, startCol + i * colStep];
            }
            return line;
        }

        public static int[,] SimulatedMove(int[,] board, int player, int move)
        {
            int[,] newBoard = CloneBoard(board);
            for (int i = newBoard.GetLength(0) - 1; i >= 0; i--)
            {
                if (newBoard[i, move] == 0)
                {
                    newBoard[i, move] = player; // Colocar la ficha
                    break; // Detener el bucle después de colocar
                }
            }
            return newBoard;
        }

        private static int[,] CloneBoard(int[,] original)
        {
            int rows = original.GetLength(0);
            int cols = original.GetLength(1);
            int[,] copy = new int[rows, cols];
            for (int x = 0; x < rows; x++)
            {
                for (int y = 0; y < cols; y++)
                {
                    copy[x, y] = original[x, y];
                }
            }
            return copy;
        }

        public static void PrintBoard(int[,] board)
        {
            string boardStr = "";
            for (int j = 0; j < board.GetLength(0); j++)
            {
                for (int i = 0; i < board.GetLength(1); i++)
                {
                    boardStr += board[j, i] + " ";
                }
                boardStr += "\n";
            }
            Debug.Log(boardStr);
        }

        public static bool IsGameOver(int[,] board)
        {
            int numPiecesToWin = GameObject.FindFirstObjectByType<GameController>().numPiecesToWin;

            // Comprobar filas
            for (int row = 0; row < board.GetLength(0); row++)
            {
                for (int col = 0; col <= board.GetLength(1) - numPiecesToWin; col++)
                {
                    if (CheckLine(board, row, col, 0, 1, numPiecesToWin)) // Horizontal
                        return true;
                }
            }

            // Comprobar columnas
            for (int col = 0; col < board.GetLength(1); col++)
            {
                for (int row = 0; row <= board.GetLength(0) - numPiecesToWin; row++)
                {
                    if (CheckLine(board, row, col, 1, 0, numPiecesToWin)) // Vertical
                        return true;
                }
            }

            // Comprobar diagonales (↘)
            for (int row = 0; row <= board.GetLength(0) - numPiecesToWin; row++)
            {
                for (int col = 0; col <= board.GetLength(1) - numPiecesToWin; col++)
                {
                    if (CheckLine(board, row, col, 1, 1, numPiecesToWin)) // Diagonal derecha
                        return true;
                }
            }

            // Comprobar diagonales (↙)
            for (int row = 0; row <= board.GetLength(0) - numPiecesToWin; row++)
            {
                for (int col = numPiecesToWin - 1; col < board.GetLength(1); col++)
                {
                    if (CheckLine(board, row, col, 1, -1, numPiecesToWin)) // Diagonal izquierda
                        return true;
                }
            }

            for (int row = 0; row < board.GetLength(0); row++)
            {
                for (int col = 0; col < board.GetLength(1); col++)
                {
                    if (board[row, col] == 0) return false; // No hay ganador
                }
            }
            return true;
        }

        private static bool CheckLine(int[,] board, int startRow, int startCol, int rowStep, int colStep, int numPiecesToWin)
        {
            int player = board[startRow, startCol];
            if (player == 0) return false; // No hay jugador en esta posición
            for (int i = 1; i < numPiecesToWin; i++)
            {
                int newRow = startRow + i * rowStep;
                int newCol = startCol + i * colStep;
                if (newRow < 0 || newRow >= board.GetLength(0) || newCol < 0 || newCol >= board.GetLength(1))
                    return false;
                if (board[newRow, newCol] != player) return false; // No hay conexión
            }
            return true; // Conexión encontrada
        }

        public static int[] GetPossibleMoves(int[,] board)
        {
            return Enumerable.Range(0, board.GetLength(1)).Where(col => board[0, col] == 0).ToArray();
        }
    }
}
