using System;
using System.Collections;
using System.Collections.Generic;
using Enemies;
using Grid;
using Grid.Interface;
using UnityEngine;

public abstract class BaseTrap : BuildableObject
{
    [SerializeField] private BuildableObjectVisuals trapVisuals;
    [SerializeField] private int damage;

    private void Start()
    {
        Enemy.OnAnyEnemyMoved += Enemy_OnAnyEnemyMoved;
    }
    
    public override void Build(Vector2Int positionToBuild)
    {
        trapVisuals.HidePreview();
        
        TilingGrid.grid.PlaceObjectAtPositionOnGrid(gameObject, positionToBuild);
    }

    public override TypeTopOfCell GetType()
    {
        return TypeTopOfCell.Building;
    }
    
    private void Enemy_OnAnyEnemyMoved(object sender, EventArgs e)
    {
        Cell trapCell = TilingGrid.grid.GetCell(TilingGrid.LocalToGridPosition(transform.position));

        List<Enemy> enemiesOnTrapCell = trapCell.GetEnemies();

        if (enemiesOnTrapCell.Count != 0)
        {
            foreach (Enemy toDamage in enemiesOnTrapCell)
            {
                toDamage.Damage(damage);
            }
        }
        
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        Enemy.OnAnyEnemyMoved -= Enemy_OnAnyEnemyMoved;
    }
}
