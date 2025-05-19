using UnityEngine;

public class RoomEntranceTrigger : MonoBehaviour
{
    [SerializeField] private RoomFlowController roomFlowController; // 使用 SerializeField 確保在編輯器中可見

    private void Awake()
    {
        // 檢查必要組件
        if (roomFlowController == null)
        {
            // 嘗試在父物件中尋找 RoomFlowController
            roomFlowController = GetComponentInParent<RoomFlowController>();
            
            if (roomFlowController == null)
            {
                Debug.LogError($"RoomEntranceTrigger {gameObject.name} 缺少 RoomFlowController 引用！請在 Inspector 中設置。");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        
        Debug.Log($"觸發器 {gameObject.name} 被觸發，碰撞物體：{other.gameObject.name}");

        if (other.CompareTag("Player"))
        {
            Debug.Log($"玩家進入房間觸發器：{gameObject.name}");

            if (roomFlowController != null)
            {
                Debug.Log($"啟動房間流程：{roomFlowController.gameObject.name}");
                roomFlowController.StartRoomFlow();
            }
            else
            {
                Debug.LogError($"RoomFlowController 未設置在 {gameObject.name} 上！");
            }
        }
    }
} 