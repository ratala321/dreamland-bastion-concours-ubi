using System;
using Grid;
using Grid.Interface;
using UnityEngine;
using Random = System.Random;

namespace Enemies
{
    public sealed class BasicEnemy : Enemy
    {
        private Random _rand = new();
        private Vector2Int testBug = new Vector2Int(13,12);
        public BasicEnemy()
        {
            ennemyType = EnnemyType.Basic;
            health = 1;
        }


        protected override void Initialize()
        {
            AddInGame(this.gameObject);
        }


        /**
         * Deplace un ennemi d'un block :
         *      Vers l'avant si aucun obstacle
         *      Gauche ou droite si un obstacle
         */
        public override void Move(int energy)
        {
            {
                if (!IsServer) return;
                if (!IsTimeToMove(energy)) return;

                if (!TryMoveOnNextCell())
                {
                    if (!MoveSides())
                    {
                        throw new Exception("moveside did not work, case not implemented yet !");
                    }
                }
            }
        }

        public override bool PathfindingInvalidCell(Cell cellToCheck)
        {
            return cellToCheck.HasTopOfCellOfType(TypeTopOfCell.Obstacle) ||
                   cellToCheck.HasTopOfCellOfType(TypeTopOfCell.Building);
        }

        private bool IsTimeToMove(int energy)
        {
            return energy % ratioMovement == 0;
        }

        //Commence a aller vers la droite ou la gauche aleatoirement
        private bool MoveSides()
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

            return false;
        }

        // Besoin de direction 2d pour valider ce quil a sur la cell
        //Retourne true si a pu effectuer le deplacement
        private bool TryMoveOnNextCell()
        {
            if (path == null || path.Count == 0)
                return true;

            Cell nextCell = path[0];
            path.RemoveAt(0);
            if (IsValidCell(nextCell))
            {
                cell = nextCell;
                MoveEnemy(TilingGrid.GridPositionToLocal(nextCell.position));
                return true;
            }

            return false;
        }

        private bool TryMoveOnNextCell(Vector2Int direction)
        {
            Vector2Int nextPosition = new Vector2Int(cell.position.x + direction.x, cell.position.y + direction.y);
            Cell nextCell = TilingGrid.grid.GetCell(new Vector2Int());
            Debug.Log("cellPos + direction == " + nextPosition);

            if (IsValidCell(nextCell))
            {
                cell = TilingGrid.grid.GetCell(nextPosition);
                MoveEnemy(TilingGrid.GridPositionToLocal(nextPosition));
                return true;
            }

            return false;
        }

        /*
         * Bouge l'ennemi
         * Enregistre sa nouvelle position dans le recorder
         */
        private void MoveEnemy(Vector3 direction)
        {
            if (!IsServer) return;
            Debug.Log("BASIC PLACE AVANT : " + transform.position);
            TilingGrid.grid.PlaceObjectAtPositionOnGrid(this.gameObject, direction);
            Debug.Log("BASIC PLACE APRES : " + transform.position);
        }

        private bool IsValidCell(Cell cell)
        {
            PathfindingInvalidCell(cell);
            bool isValidBlockType = (cell.type & BlockType.EnemyWalkable) > 0;
            bool hasNoObstacle = !cell.HasTopOfCellOfType(TypeTopOfCell.Obstacle);
            bool hasNoEnemy = !cell.HasTopOfCellOfType(TypeTopOfCell.Enemy);
            Debug.Log("BASIC Has NO enemy on top : " + hasNoEnemy);
            if (!hasNoEnemy)
            {
                
                if (cell.position == testBug)
                {
                   // hasNoEnemy = true;
                   Debug.Log("BASIC Has enemy on top : " + true);
                   Debug.Log("next CELL POS " + cell.position);
                   Debug.Log("BASIC CELL POS: " + TilingGrid.LocalToGridPosition(transform.position));
                }

            }

            return isValidBlockType && hasNoObstacle && hasNoEnemy && !PathfindingInvalidCell(cell);
        }
    }
}