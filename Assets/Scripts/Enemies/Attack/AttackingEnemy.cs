using System;
using System.Collections;
using System.Collections.Generic;
using Enemies.Basic;
using Grid;
using Grid.Interface;
using UnityEngine;
using UnityEngine.Assertions;
using Random = System.Random;

namespace Enemies
{
    public abstract class AttackingEnemy : BasicEnemy
    {
        public abstract int AttackDamage { get; set; }

        protected override EnemyChoicesInfo BackendMove()
        {
             Assert.IsTrue(IsServer);
                        
            if (HasReachedTheEnd())
            {
                return new EnemyChoicesInfo()
                {
                    hasReachedEnd = true,
                };
            }

            if (!IsTimeToMove() || isStupefiedState > 0)
            {
                return new EnemyChoicesInfo() { hasMoved = false };
            }

            var attackInfo = ChoseToAttack();
            if ( attackInfo.hasAttacked )
            {
                return new EnemyChoicesInfo()
                {
                    attack = attackInfo,
                };

            }
            
            if (!TryMoveOnNextCell())
            {
                hasPath = false;
                if (!MoveSides())
                {
                    return new EnemyChoicesInfo();
                }
                else
                {
                    return new EnemyChoicesInfo()
                    {
                        hasMoved = true, 
                        destination = TilingGrid.GridPositionToLocal(cell.position),
                    };
                }
            }
            return new EnemyChoicesInfo()
            {
                hasMoved = true,
                destination = TilingGrid.GridPositionToLocal(cell.position),
            };
        }

        public abstract AttackingInfo ChoseToAttack();
        public AttackingInfo ChoseAttack(List<Cell> cellsInRadius)
        {
            foreach (var aCell in cellsInRadius)
            {
                if (TowerIsAtRange(aCell) &&
                    canAttack())
                {
                    hasPath = false;
                    var attackedObjectInfo = Attack(aCell.GetTower());
                    return new AttackingInfo()
                    {
                        hasAttacked = true,
                        toKill = attackedObjectInfo.Item2,
                        isTower = attackedObjectInfo.Item1,
                    };
                }
            }

            return new AttackingInfo();
        }

        private bool TowerIsAtRange(Cell aCell)
        {
            // non walkable building are towers or obstacle.
            return TilingGrid.grid.HasTopOfCellOfType(aCell, TypeTopOfCell.Building) &&
                   cell.HasNonWalkableBuilding();
        }
        
        private bool canAttack()
        {
            return true;
        }
        
        protected (bool, GameObject) Attack(BaseTower toAttack)
        {
            int remainingHP = toAttack.Damage(AttackDamage);
            return (remainingHP <= 0, toAttack.gameObject);

        }
        
        protected (bool, GameObject) Attack(Obstacle toAttack)
        {
            int remainingHP = toAttack.Damage(AttackDamage);
            return (remainingHP <= 0, toAttack.gameObject);
        }

        private IEnumerator AttackAnimation(AttackingInfo infos)
        {
            if (!IsServer) yield break;
            
            hasFinishedMoveAnimation = false;
            animator.SetBool("Attack", true);
            float currentTime = 0.0f;

            //TODO time to attack? et regarder avec anim tour
            while (timeToMove > currentTime)
            {
                currentTime += Time.deltaTime;
                yield return null;
            }
            
            animator.SetBool("Attack", false);
            if (infos.shouldKill)
            {
                
            }
            hasFinishedMoveAnimation = true;
        }

        // Peut detruire obstacle et tower, tous les cells avec obstacles `solides` sont valides 
        public override bool PathfindingInvalidCell(Cell cellToCheck)
        {
            return false;
        }

        protected override bool IsValidCell(Cell toCheck)
        {
            Cell updatedCell = TilingGrid.grid.GetCell(toCheck.position);
            bool hasObstacleOnTop = updatedCell.HasTopOfCellOfType(TypeTopOfCell.Obstacle);
            return base.IsValidCell(toCheck) && !hasObstacleOnTop;
        }


        public override void MoveCorroutine(EnemyChoicesInfo infos)
        {
            if (infos.attack.hasAttacked)
            {
                StartCoroutine(AttackAnimation(infos.attack));
            }
            else
            {
                base.MoveCorroutine(infos);
            }
        }
    }
}
