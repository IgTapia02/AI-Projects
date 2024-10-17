// Carol Álvarez
// Ignacio Tapia
// Alicia Touris
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Timers;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.Random;

public class AIController : MonoBehaviour
{
    Stopwatch _stopwatch = new Stopwatch();

    public GameObject Body;

    // Referencia a la IA
    public PlayerInfo Max;

    // Referencia al otro jugador
    public PlayerInfo Min;

    public GameState GameState;

    public AttackEvent AttackEvent;

    private int contNodos;

    [Header ("Profundidad")]
    [SerializeField]
    private int k = 4;

    // Estructura creada para saber el estado del juego
    private struct States
    {
        public float maxHealth;
        public float maxEnergy;
        public float minHealth;
        public float minEnergy;
        public AttackInfo currentAttack;
        public int currentGeneration;
        public float moveValue;
        public bool isMax;

        public States(float maxHealth, float maxEnergy, float minHealth, float minEnergy, AttackInfo currentAttack, int currentGeneration, float moveValue, bool isMax)
        {
            this.maxHealth = maxHealth;
            this.maxEnergy = maxEnergy;
            this.minHealth = minHealth;
            this.minEnergy= minEnergy;
            this.currentAttack = currentAttack;
            this.currentGeneration = currentGeneration;
            this.moveValue = moveValue;
            this.isMax = isMax;
        }
    }
   
    // Método para crear un objeto Attack a partir de un AttackInfo
    private Attack CreateAttackFromInfo(AttackInfo attackInfo)
    {
        // Creamos un nuevo objeto Attack
        Attack attack = ScriptableObject.CreateInstance<Attack>();
        attack.Source = Max;
        attack.Target = Min;

        // Asignamos la información del ataque al objeto Attack
        attack.AttackMade = attackInfo;

        return attack;
    }

    public void OnGameTurnChange(PlayerInfo currentTurn)
    {
        Max = FindObjectOfType<GameLogic>().PlayerList.Players.Find(p => p != Min);
        Min = FindObjectOfType<GameLogic>().PlayerList.Players.Find(p => p != Max);

        if (currentTurn != Max) return;
        Think();
    }

    // Método para devolver un Movimiento (estado) con la información actual de los dos jugadores
    private States Perceive()
    {
        return new States(Max.HP, Max.Energy, Min.HP, Min.Energy, null, 0, 0, true);
    }

    private void Think()
    {
        ExpectMinMax();
    }

    private void ExpectMinMax()
    {
        Act();
    }

    // Método para el funcionamiento de Max
    private States MaxValue(States state)
    {
        state.currentGeneration++;

        // Si llega al límite de generaciones o GameOver()
        if (GameOver(state) || state.currentGeneration >= k)
        {
            UnityEngine.Debug.Log("generación del nodo: " + state.currentGeneration + ": NodoMax");
            state.moveValue = EvaluateState(state);
            return state;
        }

        float maxValue = float.NegativeInfinity;
        States maxMove = state;

        // Bucle para recorrer todos los ataques posibles que puede realizar Max
        foreach (AttackInfo attack in Max.Attacks)
        {
            if(state.maxEnergy >= attack.Energy)
            {
                // Sumamos uno al contador de nodos
                contNodos++;

                // Creamos un nuevo movimiento pasándole el ataque que sí que se puede realizar
                States movimientoNuevo = state;
                movimientoNuevo.currentAttack = attack;
                movimientoNuevo.isMax = true;
                // Guardamos como resultado el movimiento con su valor en relación al azar
                States result = ChanceValue(movimientoNuevo);

                // Comprobamos si el valor resultante es el mejor valor para Max
                if (result.moveValue > maxValue)
                {
                    maxValue = result.moveValue;
                    maxMove = result;
                }
            }
        }

        // Se devuelve el mejor movimiento para Max
        return maxMove;
    }

    // Método para el funcionamiento de Min
    private States MinValue(States state)
    {
        state.currentGeneration++;

        // Si llega al límite de generaciones o GameOver()
        if (GameOver(state) || state.currentGeneration >= k)
        {
            UnityEngine.Debug.Log("generación del nodo:" + state.currentGeneration + ": NodoMin");
            state.moveValue = EvaluateState(state);
            return state;
        }

        float minValue = float.PositiveInfinity;
        States minMove = state;

        // Bucle para recorrer todos los ataques posibles que puede realizar Min
        foreach (AttackInfo attack in Min.Attacks)
        {
            if (state.minEnergy >= attack.Energy)
            {
                // Sumamos uno al contador de nodos
                contNodos++;

                // Creamos un nuevo movimiento pasándole el ataque que sí que se puede realizar
                States movimientoNuevo = state;
                movimientoNuevo.currentAttack = attack;
                movimientoNuevo.isMax = false;

                // Guardamos como resultado el movimiento con su valor en relación al azar
                States result = ChanceValue(movimientoNuevo);

                // Comprobamos si el valor resultante es el mejor valor para Min
                if (result.moveValue < minValue)
                {
                    minValue = result.moveValue;
                    minMove = result;
                }
            }
        }

        // Se devuelve el mejor movimiento para Min
        return minMove;
    }

    // Método para el funcionamiento del azar
    private States ChanceValue(States estado)
    {
        estado.currentGeneration++;

        // Si llega al límite de generaciones o GameOver()
        if (GameOver(estado) || estado.currentGeneration >= k)
        {
            UnityEngine.Debug.Log("generación del nodo:" + estado.currentGeneration+": NodoChande");
            estado.moveValue = EvaluateState(estado);
            return estado;
        }

        // Si viene de Max
        if (estado.isMax) 
        {
            // Sumamos uno al contador de nodos
            contNodos++;

            // Abrimos un nodo por si el ataque falla
            States failState = estado;
            failState.maxEnergy -= estado.currentAttack.Energy;
            
            // Determinamos el valor del azar quitándole la energía del ataque a Max y multiplicándolo por la probabilidad de fallo
            float chanceValue = EvaluateState(MinValue(failState)) * (1 - failState.currentAttack.HitChance);

            // Abrimos nodos por cada ataque con i de daño que pueda realizar
            for (int i = estado.currentAttack.MinDam; i <= estado.currentAttack.MaxDam; i++)
            {
                // Sumamos uno al contador de nodos
                contNodos++;

                States hitState = estado;
                hitState.maxEnergy -= estado.currentAttack.Energy;
                hitState.minHealth -= i;

                // Ejecutamos el movimiento en función del daño que se haga y de quién sea el movimiento
                // Evaluamos el movimiento y lo multiplicamos por la probabilidad de acierto y todos los daños que se pueden hacer, i
                chanceValue += EvaluateState(MinValue(hitState)) * (hitState.currentAttack.HitChance) * (1 / ((hitState.currentAttack.MaxDam - hitState.currentAttack.MinDam) + 1));               
            }

            // Le pegamos el nuevo valor al movimiento que llegó de Max
            estado.moveValue = chanceValue;
        }

        // Si viene de Min
        else if (!estado.isMax) 
        {
            // Sumamos uno al contador de nodos
            contNodos++;

            // Abrimos un nodo por si el ataque falla
            States failState = estado;
            failState.minEnergy -= estado.currentAttack.Energy;
            float chanceValue = EvaluateState((MaxValue(failState))) * (1 - failState.currentAttack.HitChance);

            // Abrimos nodos por cada ataque con i de daño que pueda realizar
            for (int i = estado.currentAttack.MinDam; i <= estado.currentAttack.MaxDam; i++)
            {
                // Sumamos uno al contador de nodos
                contNodos++;

                States hitState = estado;
                hitState.minEnergy -= estado.currentAttack.Energy;
                hitState.maxHealth -= i;

                // Ejecutamos el movimiento en función del daño que se haga y de quién sea el movimiento
                // Evaluamos el movimiento y lo multiplicamos por la probabilidad de acierto y todos los daños que se pueden hacer, i
                chanceValue += EvaluateState((MaxValue(hitState))) * (hitState.currentAttack.HitChance) * (1 / ((hitState.currentAttack.MaxDam - hitState.currentAttack.MinDam) + 1));
            }

            // Le pegamos el nuevo valor al movimiento que llegó de Min
            estado.moveValue = chanceValue;
        }

        return estado;
    }

    // Método con la función heurística para determinar el valor de un ataque
    private float EvaluateState(States state)
    {
        if(state.minHealth <= 0)
        {
            return 100;

        }else if(state.maxHealth <= 0)
        {
            return -100;
        }

        return ((9 * state.maxHealth) - (10 * state.minHealth)) + (state.maxEnergy/20);
    }

    // Método con el que realizamos el ataque
    private void Act()
    {
        // Iniciamos el cronómetro para ver el tiempo que tarda
        _stopwatch.Start();
        
        // Iniciamos el proceso de búsqueda por recursividad
        States state = MaxValue(Perceive());

        // Paramos el cronómetro cuando haya terminado de ejecutar el algoritmo
        _stopwatch.Stop();
        UnityEngine.Debug.Log("TIEMPO: " + _stopwatch.Elapsed);

        UnityEngine.Debug.Log("Valor del nodo:" + state.moveValue);

        // Mostramos los nodos abiertos en consola
        UnityEngine.Debug.Log("NUMNODOS: " + contNodos);
        contNodos = 0;

        // Se ejecuta el ataque seleccionado
        AttackEvent.Raise(CreateAttackFromInfo(state.currentAttack));
    }

    // Método para detectar si alguno de los dos jugadores se quedan sin vida
    bool GameOver(States state)
    {
        if (state.maxHealth <= 0 || state.minHealth <= 0)
            return true;

        return false;
    }
}
