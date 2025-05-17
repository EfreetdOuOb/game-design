using UnityEngine;
using Cinemachine;
/// <summary>
/// 攝影機一運行就自動跟隨玩家
/// </summary>
public class AutoSetUpCamera : MonoBehaviour
{

    PlayerController playerController;
    private void Awake()
    {
        CinemachineVirtualCamera camera = GetComponent<CinemachineVirtualCamera>();

        playerController = FindFirstObjectByType<PlayerController>();

        if(playerController!= null)
        {
            camera.Follow = playerController.transform; 
        }


    } 
}
