using System;
using System.Collections;
using System.Text;
using Grid;
using Grid.Interface;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.LowLevel;
using Utils;
using Random = System.Random;

namespace Enemies.Basic
{
    public abstract class BasicEnemy : Enemy
    {
        private Random _rand = new();
        protected float timeToMove = 0.3f;

        public BasicEnemy()
        {
            ennemyType = EnnemyType.PetiteMerde;
        }
        
        
        //public override IEnumerator Move()
        //{
        //    hasFinishedToMove = false;
        //    if (!IsServer)
        //    {
        //        yield break;
        //    }

        //    if (!IsTimeToMove())
        //    {
        //        timeSinceLastAction++;
        //        hasFinishedToMove = true;
        //        yield break;
        //    }
        //    
		//	if (isStupefiedState > 0)
		//	{
		//		hasFinishedToMove = true;
		//		yield break; 
		//	}
        //    
        //    yield return new WaitUntil(AnimationSpawnIsFinished);
        //    
        //    if (!TryMoveOnNextCell())
        //    {
        //        hasPath = false;
        //        if (!MoveSides())
        //        {
        //            hasFinishedMoveAnimation = true;
        //        }
        //    }

        //    yield return new WaitUntil(hasFinishedMovingAnimation);
        //    hasFinishedToMove = true;
        //    EmitOnAnyEnemyMoved();
        //}

        protected override (bool moved, bool attacked, Vector3 destination) BackendMove()
        {
            Assert.IsTrue(IsServer);
            if (!IsTimeToMove() || isStupefiedState > 0)
            {
                timeSinceLastAction++;
                return (false, false, Vector3.zero);
            }

            if (!TryMoveOnNextCell())
            {
                hasPath = false;
                if (!MoveSides())
                {
                    return (false, false, Vector3.zero);
                }
                else
                {
                    return (true, false, TilingGrid.GridPositionToLocal(cell.position));
                }
            }

            return (true, false, TilingGrid.GridPositionToLocal(cell.position));
        }

        public override bool PathfindingInvalidCell(Cell cellToCheck)
        {
            return cellToCheck.HasTopOfCellOfType(TypeTopOfCell.Obstacle) ||
                   cellToCheck.HasNonWalkableBuilding();
        }

        protected bool IsTimeToMove()
        {
            return timeSinceLastAction % MoveRatio == 0;
        }


        // Essaie de bouger vers l'avant
        protected bool TryMoveOnNextCell()
        {
            if (path == null || path.Count == 0)
            {
                return true;
            }
            Cell nextCell = path[0];
            path.RemoveAt(0);
            if (IsValidCell(nextCell))
            {
                TilingGrid.grid.RemoveObjectFromCurrentCell(this.gameObject);
                cell = nextCell;
                TilingGrid.grid.PlaceObjectAtPositionOnGrid(gameObject, cell.position);
                return true;
            }
            return false;
        }


        //Commence a aller vers la droite ou la gauche aleatoirement
        protected bool MoveSides()
        {
            if (_rand.NextDouble() < 0.5)
            {
                if (!TryMoveOnNextCell(_gauche2d))
                {
                    return TryMoveOnNextCell(_droite2d);
                }
            }
            else
            {
                if (!TryMoveOnNextCell(_droite2d))
                {
                    return TryMoveOnNextCell(_gauche2d);
                }
            }

            return true;
        }

        protected override bool TryStepBackward()
        {
            Vector2Int nextPosition = new Vector2Int(cell.position.x, cell.position.y);
            Cell nextCell = TilingGrid.grid.GetCell(nextPosition);

            if (IsValidCell(nextCell))
            {
                TilingGrid.grid.RemoveObjectFromCurrentCell(this.gameObject);
                cell = TilingGrid.grid.GetCell(nextPosition);
                TilingGrid.grid.PlaceObjectAtPositionOnGrid(gameObject, cell.position);
                return true;
            }

            // TODO REVENIR SUR PLACE SI PEUT PAS RECULER ??
            return false;
        }


        //Essayer de bouger vers direction
        private bool TryMoveOnNextCell(Vector2Int direction)
        {
            Vector2Int nextPosition = new Vector2Int(cell.position.x + direction.x, cell.position.y);
            Cell nextCell = TilingGrid.grid.GetCell(nextPosition);
            
            if (IsValidCell(nextCell))
            {
                TilingGrid.grid.RemoveObjectFromCurrentCell(this.gameObject);
                cell = TilingGrid.grid.GetCell(nextPosition);
                TilingGrid.grid.PlaceObjectAtPositionOnGrid(gameObject, cell.position);
                return true;
            }
            return false;
        }

        protected IEnumerator RotateThenMove(Vector3 direction)
        {
            RotationAnimation rotationAnimation = new RotationAnimation();
            StartCoroutine(rotationAnimation.TurnObjectTo(this.gameObject, direction));
            yield return new WaitUntil(rotationAnimation.HasMoved);
            StartCoroutine(MoveEnemy(direction));
            yield return new WaitUntil(hasFinishedMovingAnimation);
        }


        /*
         * Bouge l'ennemi
         */
        protected virtual IEnumerator MoveEnemy(Vector3 direction)
        {
            if (!IsServer) yield break;
            hasFinishedMoveAnimation = false;
            animator.SetBool("Move", true);
            float currentTime = 0.0f;
            Vector3 origin = transform.position;
            while (timeToMove > currentTime)
            {
                transform.position = Vector3.Lerp(
                    origin, direction, currentTime / timeToMove);
                currentTime += Time.deltaTime;
                yield return null;
            }

            animator.SetBool("Move", false);
            hasFinishedMoveAnimation = true;
        }

        protected bool hasFinishedMovingAnimation()
        {
            return hasFinishedMoveAnimation;
        }

         protected virtual bool IsValidCell(Cell toCheck)
        {
            Cell updatedCell = TilingGrid.grid.GetCell(toCheck.position);
            bool isValidBlockType = (updatedCell.type & BlockType.EnemyWalkable) > 0;
            bool hasNoEnemy = !TilingGrid.grid.HasTopOfCellOfType(updatedCell, TypeTopOfCell.Enemy);
            return isValidBlockType && hasNoEnemy && !PathfindingInvalidCell(updatedCell);
        }
    }
}
