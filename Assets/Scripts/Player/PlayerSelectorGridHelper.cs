using System;
using System.Collections.Generic;
using Grid.Interface;
using UnityEngine;

namespace Grid
{
    public class PlayerSelectorGridHelper : GridHelper
    {
        // Permetterait de parcourir la File pour avoir les deplacements qui se jouent dans l'ordre durant la phase 
        // d'action
        private readonly Recorder<Cell> _recorder;

        public PlayerSelectorGridHelper(Vector2Int position2d) : base(position2d)
        {
            currentCell = TilingGrid.grid.GetCell(position2d);
        }

        public PlayerSelectorGridHelper(Vector2Int position, Recorder<Cell> recorder) : base(position)
        {
            _recorder = recorder;
            _recorder.Add(currentCell);
        }


        public override bool IsValidCell(Vector2Int position)
        {
            position = currentCell.position + position;
            var cell = TilingGrid.grid.GetCell(position);

            // Comme ca on selectionne pas le vide 
            return cell.type != BlockType.None;
        }

        public Vector2Int PositionAtDirection(Vector2Int direction)
        {
            return currentCell.position + direction;
        }
        public override Vector2Int GetHelperPosition()
        {
            return currentCell.position;
        }

        public override void SetHelperPosition(Vector2Int direction)
        {
            var next = currentCell.position + direction;
            currentCell = TilingGrid.grid.GetCell(next);
        }
        public void SetHelperPosition(Cell cell)
        {
            var next = cell;
            currentCell = TilingGrid.grid.GetCell(cell.position);
        }
        public static List<ITopOfCell> GetElementsOnTopOfCell(Vector2Int position)
        {
            try
            {
                var cell = TilingGrid.grid.GetCell(position);
                var objectsOnTop = cell.ObjectsTopOfCell;
                if (objectsOnTop == null)
                {
                    Debug.Log("List was null");
                    return new List<ITopOfCell>();
                }

                return objectsOnTop;
            }
            catch (ArgumentException)
            {
                Debug.Log("Got wrong position?");
                return new List<ITopOfCell>();
            }
        }


    }
}