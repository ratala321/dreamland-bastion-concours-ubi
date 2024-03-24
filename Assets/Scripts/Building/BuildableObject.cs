using System.Collections;
using System.Collections.Generic;
using Grid.Interface;
using UnityEngine;

public abstract class BuildableObject : MonoBehaviour, IBuildable, ITopOfCell
{
    [field: Header("Buildable Object")]
    [SerializeField] protected BuildableObjectSO buildableObjectSO;
    
    public abstract void Build(Vector2Int positionToBuild);

    public  BuildableObjectSO GetBuildableObjectSO()
    {
        return buildableObjectSO;
    }

    public new abstract TypeTopOfCell GetType();

    public GameObject ToGameObject()
    {
        return gameObject;
    }
}
