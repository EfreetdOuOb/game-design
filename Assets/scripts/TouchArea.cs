using UnityEngine;

public class TouchArea : MonoBehaviour
{
    private Box box;
    
    public void SetBox(Box _box)
    {
        box = _box;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 玩家進入範圍，執行相應邏輯
            Debug.Log("玩家進入觸發區域");
            if (box != null)
            {
                box.SetPlayerInRange(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 玩家離開範圍，執行相應邏輯
            Debug.Log("玩家離開觸發區域");
            if (box != null)
            {
                box.SetPlayerInRange(false);
            }
        }
    }
}