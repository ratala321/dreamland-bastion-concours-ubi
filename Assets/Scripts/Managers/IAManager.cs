using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class IAManager : MonoBehaviour
{
    public static IAManager Instance{ get; private set; }


    private void Awake()
    {
        Instance = this;
    }
    
        public void MoveEnemies()
        {
            Ennemy enemy;
            List<GameObject> enemies = Ennemy.GetEnemiesInGame();
            foreach (var enemyObj in enemies)
            {
                enemy = enemyObj.GetComponent<Ennemy>();
                StartRoutineMoveEnemy(enemy);

            }
        }

        private void StartRoutineMoveEnemy(Ennemy enemy)
        {
            enemy.Move();
        }


    

}
