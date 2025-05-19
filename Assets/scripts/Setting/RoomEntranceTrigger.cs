using UnityEngine;

public class RoomEntranceTrigger : MonoBehaviour
{
    public RoomFlowController roomFlowController; // 拖曳對應房間的 RoomFlowController

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (roomFlowController != null && !roomFlowController.enabled)
            {
                roomFlowController.enabled = true;
                roomFlowController.StartRoomFlow(); // 啟動該房間流程
            }
        }
    }
} 