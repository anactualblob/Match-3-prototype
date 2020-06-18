using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ScaleToFitGridBackground : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] SpriteRenderer gridBackground = null;
    [SerializeField] RectTransform gameArea = null;
    [Space]
    [Tooltip("Margins relative to the Game Area rectTransform specified above.")]
    [SerializeField] CameraFitMargins margins;
    [Space]
    [SerializeField] bool fitEveryFrame = false;
    
    Camera cam;


    void Start()
    {
        if (gridBackground == null) Debug.LogError("ScaleToFitBackground.cs : Couldn't find gridBackground. Has it been assigned in the inspector?");
        if (gameArea == null) Debug.LogError("ScaleToFitBackground.cs : Couldn't find gameArea. Has it been assigned in the inspector?");

        cam = GetComponent<Camera>();

        ScaleCameraToFitBackground();
    }


    private void Update()
    {
        if (fitEveryFrame)
        {
            ScaleCameraToFitBackground();
        }
    }


    public void ScaleCameraToFitBackground()
    {
        float cameraWidth = cam.orthographicSize * cam.aspect;

        // fitSizes calculations take into account the gridBackground size, the margins defined in inspector, and gameArea.
        float verticalFitSize = ( gridBackground.size.y + margins.vertical * 2 + cam.orthographicSize * 2 * (gameArea.anchorMin.y + (1-gameArea.anchorMax.y)) ) / 2;
        float horizontalFitSize = 1/cam.aspect * (gridBackground.size.x + margins.horizontal * 2 + cameraWidth * 2 * (gameArea.anchorMin.x + (1 - gameArea.anchorMax.x))) / 2;

        // camera size is the greater of the two sizes to ensure that it always frames all of the gridBackground
        cam.orthographicSize = Mathf.Max(verticalFitSize, horizontalFitSize);
    }



    [System.Serializable]
    struct CameraFitMargins
    {
        [Tooltip("Margin from left and right of the screen")]
        public float horizontal; 
        [Tooltip("Margin from top and bottom of the screen")]
        public float vertical;
    }
}
