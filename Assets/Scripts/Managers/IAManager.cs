using System;
using System.Collections;
using System.Collections.Generic;
using Enemies;
using Grid;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Managers
{
    public class IAManager : MonoBehaviour
    {
        public static IAManager Instance{ get; private set; }


        private bool hasMovedEveryEnemies = false;
        private void Awake()
        {
            Instance = this;
        }
    
        public IEnumerator MoveEnemies(int totalEnergy)
        {
            hasMovedEveryEnemies = false;
            List<GameObject> enemies = Enemy.GetEnemiesInGame();
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i].GetComponent<Enemy>();
                SetEnemyPath(enemy);
                StartCoroutine(enemy.Move(totalEnergy));
                yield return new WaitUntil(enemy.hasFinishedMoving);
            }

            hasMovedEveryEnemies = true;
        }

        public bool hasMovedEnemies()
        {
            return hasMovedEveryEnemies;
        }

        private static void SetEnemyPath(Enemy enemy)
        {
         
            if (enemy.hasPath) return;
            
            Cell origin = enemy.GetCurrentPosition();
            Cell destination = enemy.GetDestination();
            Func<Cell, bool> invalidCellPredicate = enemy.PathfindingInvalidCell;
            enemy.path = AStarPathfinding.GetPath(origin, destination, invalidCellPredicate);
            enemy.hasPath = true;
            enemy.path.RemoveAt(0); // cuz l'olgo donne la cell d'origine comme premier element
        }
     
        private static void Highlight(Cell cell)
        {
            Instantiate(TowerDefenseManager.highlighter, TilingGrid.CellPositionToLocal(cell), quaternion.identity);
         
        }

        public static void ResetEnemies()
        {
            List<GameObject> enemies = Enemy.GetEnemiesInGame();
            foreach (var enemy in enemies)
            {
                enemy.GetComponent<Enemy>().hasPath = false;
            }
        }
    }
}
