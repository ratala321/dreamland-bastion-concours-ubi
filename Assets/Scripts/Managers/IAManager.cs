using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Enemies;
using Grid;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Managers
{
    public class IAManager : MonoBehaviour
    {
        public static IAManager Instance{ get; private set; }

        private bool hasMovedEveryEnemies = false;
        private Dictionary<Enemy, EnemyChoicesInfo> EnemyChoices = new();
        private void Awake()
        {
            Instance = this;
        }

        public void BackendMoveEnemies()
        {
            EnemyChoices = new();
            foreach (var enemy in Enemy.GetEnemiesInGame())
            {
                var e = enemy.GetComponent<Enemy>();
                SetEnemyPath(e);
                var enemyChoicesInfo = e.CalculateChoices();
                EnemyChoices.Add(e, enemyChoicesInfo);
            }

        }

        public IEnumerator MoveEnemies()
        {
            hasMovedEveryEnemies = false;
            List<Enemy> movingEnemies = new();
            List<GameObject> allEnemies = Enemy.GetEnemiesInGameCopy();
            while(allEnemies.Count > 0) 
            {
                GameObject enemyGO = allEnemies[0];
                allEnemies.RemoveAt(0);
                Enemy e = enemyGO.GetComponent<Enemy>();
                
                var info = EnemyChoices[e];
                if (!info.hasReachedEnd) 
                    movingEnemies.Add(e);
                else
                {
                    TilingGrid.grid.RemoveObjectFromCurrentCell(enemyGO); 
                    e.RemoveInGame();
                }
                e.MoveCorroutine(info);
            }

            while (movingEnemies.Count > 0)
            {
                for (int i = 0; i < movingEnemies.Count; i++)
                {
                    var enemy = movingEnemies[i];
                    if (enemy.hasFinishedMoveAnimation)
                    {
                        movingEnemies.Remove(enemy);
                    }
                }
                yield return null;
            }

            foreach (var enemy in Enemy.GetEnemiesInGame())
            {
                enemy.GetComponent<Enemy>().ResetAnimationStates();
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

            Instance.hasMovedEveryEnemies = false;
        }
  
    }
}
