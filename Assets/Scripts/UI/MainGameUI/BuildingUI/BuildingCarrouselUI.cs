using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Grid;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class BuildingCarrouselUI : MonoBehaviour
{
    [SerializeField] private Button chooseBuildingButton;
    
    [SerializeField] private Image centerImage;
    [SerializeField] private Image leftImage;
    [SerializeField] private Image rightImage;

    [SerializeField] private TextMeshProUGUI selectedBuildingText;

    [Header("Tweening")]
    [SerializeField] private float endScale;
    [SerializeField] private float tweeningTime;
    
    private LinkedList<BuildableObjectSO> _buildableObjectsSO;
    private LinkedListNode<BuildableObjectSO> _selectedBuilding;

    private GameObject _preview;

    public event EventHandler<OnBuildingSelectedEventArgs> OnBuildingSelected;
    public class OnBuildingSelectedEventArgs : EventArgs
    {
        public BuildableObjectSO SelectedBuildableObjectSO;
    }
    
    private void Awake()
    {
        _buildableObjectsSO = new LinkedList<BuildableObjectSO>();
    }

    private void Start()
    {
        InitializeBuildableObjectsList();
        
        CentralizedInventory.Instance.OnNumberResourceChanged += CentralizedInventory_OnNumberResourceChanged;
        
        InputManager.Instance.OnUserInterfaceShoulderRightPerformed += InputManager_OnUserInterfaceShoulderRightPerformed;
        InputManager.Instance.OnUserInterfaceShoulderLeftPerformed += InputManager_OnUserInterfaceShoulderLeftPerformed;
        InputManager.Instance.OnPlayerInteractPerformed += InputManager_OnPlayerInteractPerformed;
        
        _selectedBuilding = _buildableObjectsSO.First;
        
        BasicShowHide.Hide(gameObject);
    }

    public void Show()
    {
        transform.LeanScale(Vector3.one * endScale, tweeningTime).setEaseOutExpo().setLoopPingPong(1);
        
        UpdateUI();
        
        BasicShowHide.Show(gameObject);
    }

    private void UpdateUI()
    {
        SetCarrouselImages();
        
        ShowPreview();
        
        ShowDescription();
        
        ShowMaterialCost(_selectedBuilding.Value);
    }

    public void HideForNextBuildStep()
    {
        HidePreview();
        
        HideDescription();
        
        BasicShowHide.Hide(gameObject);
    }
    
    public void Hide()
    {
        HidePreview();
        
        HideDescription();
        
        ClearOldMaterialCostUI();
        
        BasicShowHide.Hide(gameObject);
    }

    private void SetCarrouselImages()
    {
        if (_buildableObjectsSO.Count == 1)
        {
            centerImage.sprite = _selectedBuilding.Value.icon;
            return;
        }

        if (_selectedBuilding.Previous == null)
        {

            leftImage.sprite = _buildableObjectsSO.Last.Value.icon;
            rightImage.sprite = _selectedBuilding.Next.Value.icon;

            centerImage.sprite = _selectedBuilding.Value.icon;
            return;
        }

        if (_selectedBuilding.Next == null)
        {
            leftImage.sprite = _selectedBuilding.Previous.Value.icon;
            rightImage.sprite = _buildableObjectsSO.First.Value.icon;

            centerImage.sprite = _selectedBuilding.Value.icon;
            return;
        }
        
        leftImage.sprite = _selectedBuilding.Previous.Value.icon;
        rightImage.sprite = _selectedBuilding.Next.Value.icon;

        centerImage.sprite = _selectedBuilding.Value.icon;
    }
    
    private void ShowMaterialCost(BuildableObjectSO selectedBuildingValue)
    {
        CentralizedInventory.Instance.ShowCostForBuildableObject(selectedBuildingValue);
    }
    
    private void ClearOldMaterialCostUI()
    {
        CentralizedInventory.Instance.ClearAllMaterialsCostUI();
    }

    private const string MISSING_RESOURCE_ERROR = "Resources Missing For Building !";
    private void ShowDescription()
    {
        if (HasResourceForBuilding())
        {
            selectedBuildingText.color = new Color(69, 69, 69);
            selectedBuildingText.text = _selectedBuilding.Value.description;
        
        }
        else
        {
            selectedBuildingText.color = ColorPaletteUI.Instance.ColorPaletteSo.errorColor;
            selectedBuildingText.text = MISSING_RESOURCE_ERROR;
        }
        
        BasicShowHide.Show(selectedBuildingText.gameObject);
    }
    
    private void HideDescription()
    {
        BasicShowHide.Hide(selectedBuildingText.gameObject);
    }
    
    private void ShowPreview()
    {
        HidePreview();

        _preview = Instantiate(_selectedBuilding.Value.visuals);
        
        MovePreviewInFrontOfCamera();
    }

    private const float PREVIEW_FRONT_DISTANCE = 10f;
    private const float PREVIEW_RIGHT_DISTANCE = 9f;
    private const float PREVIEW_ROTATION_SPEED = 6f;
    
    private void MovePreviewInFrontOfCamera()
    {
        GameObject toFollow = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.VirtualCameraGameObject;

        _preview.AddComponent<RotateObject>().SetRotationParameter(PREVIEW_ROTATION_SPEED);
        
        _preview.AddComponent<FollowTransform>().SetFollowParameters(
            toFollow,
            new Vector3(PREVIEW_RIGHT_DISTANCE, 0, PREVIEW_FRONT_DISTANCE), 
            FollowTransform.DirectionOfFollow.FrontRight
        );
            
        _preview.GetComponent<BuildableObjectVisuals>().ShowPreview();
    }

    private void HidePreview()
    {
        if (_preview != null)
        {
            Destroy(_preview);
            _preview = null;
        }
    }
    
    private void InitializeBuildableObjectsList()
    {
        foreach (BuildableObjectSO buildableObjectSo in SynchronizeBuilding.Instance.GetAllBuildableObjectSo().list)
        {
            _buildableObjectsSO.AddLast(buildableObjectSo);
        }
    }

    private void InputManager_OnUserInterfaceShoulderRightPerformed(object sender, EventArgs e)
    {
        if (!gameObject.activeSelf) { return; }
        
        _selectedBuilding = _selectedBuilding.Next;

        if (_selectedBuilding == null)
        {
            _selectedBuilding = _buildableObjectsSO.First;
        }
        
        UpdateUI();
    }
    
    private void InputManager_OnUserInterfaceShoulderLeftPerformed(object sender, EventArgs e)
    {
        if (!gameObject.activeSelf) { return; }

        _selectedBuilding = _selectedBuilding.Previous;
        
        if (_selectedBuilding == null)
        {
            _selectedBuilding = _buildableObjectsSO.Last;
        }
        
        UpdateUI();
    }
    
    private void InputManager_OnPlayerInteractPerformed(object sender, EventArgs e)
    {
        if (!gameObject.activeSelf) { return; }

        if (HasResourceForBuilding())
        {
            EmitOnBuildingSelected();
        }
    }
    
    private bool HasResourceForBuilding()
    {
        return CentralizedInventory.Instance.HasResourcesForBuilding(_selectedBuilding.Value);
    }

    
    private void EmitOnBuildingSelected()
    {
        OnBuildingSelected?.Invoke(this, new OnBuildingSelectedEventArgs
        {
            SelectedBuildableObjectSO = _selectedBuilding.Value,
        });
    }

    private void CentralizedInventory_OnNumberResourceChanged(object sender, CentralizedInventory.OnNumberResourceChangedEventArgs e)
    {
        if (gameObject.activeSelf)
        {
            UpdateUI();
        }
    }
}
