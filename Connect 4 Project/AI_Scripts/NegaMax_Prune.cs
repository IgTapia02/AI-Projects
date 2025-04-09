using ConnectFour;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class NegaMax_Prune : PlayerBase
{
    int depth = 5;
    int nNodes = 0;
    struct Move_
    {
        private int _depth;
        public int Depth
        {
            get { return _depth; }
            set { _depth = value; }
        }

        private int[,] _board;
        public int[,] Board
        {
            get { return _board; }
            set { _board = value; }
        }

        private int _value;
        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }

        private int _column;
        public int Column
        {
            get { return _column; }
            set { _column = value; }
        }

        private int _turn;
        public int Turn
        {
            get { return _turn; }
            set { _turn = value; }
        }

        // Constructor opcional para inicializar
        public Move_(int depth, int[,] board, int value, int column, int turn)
        {
            _depth = depth;
            _board = board;
            _value = value;
            _column = column;
            _turn = turn;
        }
    }

    override protected GameObject SpawnPiece()
    {
        Vector3 spawnPos;

        Debug.Log("Soy el NegaMax_Poda");
        Move_ actualmove = new Move_(0, CloneBoard(controller.GetArrayField()), int.MinValue, 0, 1);
        //PrintBoard(actualmove.Board);
        //PrintBoard(controller.GetArrayField());
        Move_ nextMove = NegaMaxAlgorithm_Prune(actualmove, int.MinValue, int.MaxValue);

        Debug.Log("Nodos: " + nNodes + "valor:" + nextMove.Value + "Movimiento: " + nextMove.Column);
        spawnPos = new Vector3(nextMove.Column, 0, 0);

        Debug.Log("columna elegida: " + nextMove.Column);

        GameObject g = Instantiate(piece,
                new Vector3(
                Mathf.Clamp(spawnPos.x, 0, controller.numColumns - 1),
                controller.GetField().transform.position.y + 1, 0),
                Quaternion.identity) as GameObject;

        //PrintBoard(nextMove.Board);
        nNodes = 0;
        return g;
    }

    int[,] CloneBoard(int[,] original)
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


    override public void Move()
    {
        if (gameObjectTurn == null)
        {
            gameObjectTurn = SpawnPiece();
        }
        else
        {
            if (!isDropping)
                StartCoroutine(dropPiece(gameObjectTurn));
        }
    }

    override protected IEnumerator dropPiece(GameObject gObject)
    {
        isDropping = true;

        Vector3 startPosition = gObject.transform.position;
        Vector3 endPosition = new Vector3();

        int x = Mathf.RoundToInt(startPosition.x);
        startPosition = new Vector3(x, startPosition.y, startPosition.z);

        bool foundFreeCell = false;
        for (int i = controller.numRows - 1; i >= 0; i--)
        {
            if (piece == Resources.Load<GameObject>($"prefabs/redPiece"))
            {
                if (controller.CanUpdateField(i, x, 1))
                {
                    endPosition = controller.fieldObjects[i, x].transform.position;
                    endPosition.z -= 1;
                    foundFreeCell = true;
                    break;
                }
            }
            else
            {
                if (controller.CanUpdateField(i, x, -1))
                {
                    Debug.Log(x + " " + i);

                    endPosition = controller.fieldObjects[i, x].transform.position;
                    foundFreeCell = true;
                    break;
                }
            }
        }

        if (foundFreeCell)
        {
            GameObject g = Instantiate(gObject) as GameObject;
            gameObjectTurn.GetComponent<Renderer>().enabled = false;

            float distance = Vector3.Distance(startPosition, endPosition);

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * controller.dropTime * ((controller.numRows - distance) + 1);

                g.transform.position = Vector3.Lerp(startPosition, endPosition, t);

                yield return null;
            }

            g.transform.parent = controller.GetField().transform;

            DestroyImmediate(gameObjectTurn);

            StartCoroutine(controller.Won());

            while (controller.isCheckingForWinner)
                yield return null;


            controller.ChangePlayer();
        }

        isDropping = false;

        yield return 0;
    }
    void PrintBoard(int[,] board)
    {
        string boardStr = "";
        for (int j = 0; j < board.GetLength(0); j++)
        {
            for (int i = 0; i < board.GetLength(1); i++)
            {
                //boardStr += " [ Row:" + j + " Col:" + i + " ] " + board[j, i] + " ";
                boardStr += board[j, i] + " ";
            }
            boardStr += "\n";
        }
        Debug.Log(boardStr);
    }

    Move_ NegaMaxAlgorithm_Prune(Move_ _actualMove, int alpha, int beta)
    {
        int[] moves = BoardManagment.GetPossibleMoves(_actualMove.Board);

        foreach (int move in moves)
        {
            //if (move != 0)
            //    return null;

        }
        nNodes++;

        if (_actualMove.Depth == depth || BoardManagment.IsGameOver(_actualMove.Board))
        {
            _actualMove.Value = _actualMove.Turn * BoardManagment.EvaluateBoard(_actualMove.Board, _actualMove.Turn);
            Debug.Log("Mov Final");
            //PrintBoard(_actualMove.Board);
            ///Debug.Log(_actualMove.Value);
            return _actualMove;
        }

        Move_ bestMove = new Move_(_actualMove.Depth, _actualMove.Board, int.MinValue, 0, _actualMove.Turn);

        foreach (int move in moves)
        {
            int[,] newBoard = BoardManagment.SimulatedMove(CloneBoard(_actualMove.Board), _actualMove.Turn, move);

            Move_ newMove = new Move_(_actualMove.Depth + 1, newBoard, -_actualMove.Value, move, -_actualMove.Turn);

            //Debug.Log("Movimiento: " + move);
            //PrintBoard(newMove.Board);

            Move_ resultMove = NegaMaxAlgorithm_Prune(newMove, -beta, -alpha);
            int score = -resultMove.Value;

            if (resultMove.Value > bestMove.Value)
            {
                bestMove = resultMove;
                bestMove.Column = move;
            }

            alpha = Mathf.Max(alpha, score);
            if (alpha >= beta)
            {
                Debug.Log("HE HECHO UNA PODA");
                break;
            }

        }


        return bestMove;
    }
}
