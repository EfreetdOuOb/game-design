using System.Collections;
using UnityEngine;
using System.Collections.Generic;
// using ItemTipPanel; // 移除這個 using 指示詞
using TMPro; // 如果 ItemTipPanel 中使用了 TMPro

public class RoomFlowController : MonoBehaviour
{
    public enum RoomStep
    {
        WaitForBoxOpen,
        ShowItemTip,
        ShowDialogueBeforeBattle,  // 戰鬥前的對話
        ShowDialogueAfterBattle,   // 戰鬥後的對話
        ShowBattleTip,
        StartBattle,
        WaitForBattleEnd,
        RoomComplete,
        Transition
    }

    public RoomStep currentStep = RoomStep.WaitForBoxOpen;

    [Header("流程物件")]
    public GameObject box; // 可為 null
    public GameObject itemTipPanel;
    public DialoguePanel dialoguePanel;
    [Header("對話內容")]
    [TextArea(2, 5)] public string[] dialogueBeforeBattle;  // 戰鬥前的對話
    [TextArea(2, 5)] public string[] dialogueAfterBattle;   // 戰鬥後的對話
    public GameObject battleTipPanel;
    public EnemySpawner enemySpawner;
    public float itemTipDuration = 2f;
    public float battleTipDuration = 2f;
    public GameObject[] exitDoors; // 離開房間的門
    public GameObject[] entranceDoors; // 可以進入的房間的門
    public GameObject[] currentRoomDoors; // 當前房間的門
    public GameObject roomCompletePanel; // 房間完成UI

    private bool hasStarted = false;
    private bool isCompleted = false;
    private static HashSet<string> completedRooms = new HashSet<string>(); // 追蹤已完成的房間

    private void Start()
    {
        // 如果房間已經完成過，直接打開所有門
        if (completedRooms.Contains(gameObject.name))
        {
            OpenAllDoors();
        }
        else
        {
            // 新房間，關閉當前房間的門
            CloseCurrentRoomDoors();
        }
    }

    private void CloseCurrentRoomDoors()
    {
        if (currentRoomDoors != null)
        {
            foreach (var door in currentRoomDoors)
            {
                if (door != null)
                {
                    var doorComp = door.GetComponent<Door>();
                    if (doorComp != null)
                    {
                        Debug.Log($"關閉門：{door.name}");
                        doorComp.Close();
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning($"RoomFlowController {gameObject.name} 的 currentRoomDoors 為空");
        }
    }

    private void OpenAllDoors()
    {
        if (exitDoors != null)
        {
            foreach (var door in exitDoors)
            {
                if (door != null)
                {
                    var doorComp = door.GetComponent<Door>();
                    if (doorComp != null) doorComp.Open();
                }
            }
        }

        if (entranceDoors != null)
        {
            foreach (var door in entranceDoors)
            {
                if (door != null)
                {
                    var doorComp = door.GetComponent<Door>();
                    if (doorComp != null) doorComp.Open();
                }
            }
        }
    }

    private void CloseAllDoors()
    {
        if (exitDoors != null)
        {
            foreach (var door in exitDoors)
            {
                if (door != null)
                {
                    var doorComp = door.GetComponent<Door>();
                    if (doorComp != null) doorComp.Close();
                }
            }
        }

        if (entranceDoors != null)
        {
            foreach (var door in entranceDoors)
            {
                if (door != null)
                {
                    var doorComp = door.GetComponent<Door>();
                    if (doorComp != null) doorComp.Close();
                }
            }
        }
    }

    public void StartRoomFlow()
    {
        if (hasStarted || isCompleted) return;
        hasStarted = true;
        Debug.Log($"玩家進入房間：{gameObject.name}");
        Time.timeScale = 1f; // 確保時間縮放正常
        StartCoroutine(RoomFlow());
    }

    private IEnumerator RoomFlow()
    {
        // 1. 等待玩家開啟寶箱（如果有寶箱）
        currentStep = RoomStep.WaitForBoxOpen;
        string acquiredItemName = "";
        Sprite acquiredItemSprite = null;
        string acquiredItemDescription = "";

        if (box != null)
        {
            yield return new WaitUntil(() => {
                var boxComp = box != null ? box.GetComponent<Box>() : null;
                bool opened = boxComp == null || boxComp.IsOpened == true;
                if (opened && boxComp != null)
                {
                    // 每次都重新取得資訊
                    acquiredItemName = boxComp.itemName;
                    acquiredItemSprite = boxComp.itemImage;
                    acquiredItemDescription = boxComp.itemDescription;
                    Debug.Log($"從寶箱獲取物品：{acquiredItemName}");
                }
                return opened;
            });
        }

        // 2. 顯示裝備提示
        currentStep = RoomStep.ShowItemTip;
        if (itemTipPanel != null)
        {
            ItemTipPanel tipPanel = itemTipPanel.GetComponent<ItemTipPanel>();
            if (tipPanel != null)
            {
                yield return StartCoroutine(tipPanel.ShowItemInfoCoroutine(acquiredItemName, acquiredItemSprite, acquiredItemDescription));
            }
            else
            {
                Debug.LogError("ItemTipPanel GameObject 上沒有 ItemTipPanel 腳本！");
                itemTipPanel.SetActive(true);
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));
                itemTipPanel.SetActive(false);
            }
        }
        // 顯示完畢後延遲銷毀箱子
        if (box != null)
        {
            Destroy(box);
        }

        // 3. 戰鬥前的對話（可選）
        currentStep = RoomStep.ShowDialogueBeforeBattle;
        if (dialoguePanel != null && dialogueBeforeBattle != null && dialogueBeforeBattle.Length > 0)
        {
            // 處理對話文本中的換行符號，將 '\n' 替換為 \n
            string[] processedDialogue = new string[dialogueBeforeBattle.Length];
            for (int i = 0; i < dialogueBeforeBattle.Length; i++)
            {
                // 如果您在 Inspector 中輸入的是實際換行，這裡可能不需要替換 '\\n'
                // 但為了兼容性，這裡保留替換邏輯，確保 '\n' 被識別為換行
                processedDialogue[i] = dialogueBeforeBattle[i].Replace("\\n", "\n");
            }
            yield return StartCoroutine(dialoguePanel.ShowDialogue(processedDialogue));
        }

        // 4. 顯示戰鬥提示
        currentStep = RoomStep.ShowBattleTip;
        if (battleTipPanel != null) battleTipPanel.SetActive(true);
        yield return new WaitForSeconds(battleTipDuration);
        if (battleTipPanel != null) battleTipPanel.SetActive(false);

        // 4.5 關閉所有門
        CloseCurrentRoomDoors();

        // 5. 開始戰鬥
        currentStep = RoomStep.StartBattle;
        if (enemySpawner != null)
        {
            enemySpawner.StartSpawning();
        }

        // 6. 等待戰鬥結束
        currentStep = RoomStep.WaitForBattleEnd;
        yield return new WaitUntil(() => enemySpawner != null 
            && enemySpawner._allWavesCompleted 
            && GameObject.FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length == 0);

        // 7. 戰鬥後的對話（可選）
        currentStep = RoomStep.ShowDialogueAfterBattle;
        if (dialoguePanel != null && dialogueAfterBattle != null && dialogueAfterBattle.Length > 0)
        {
            // 處理對話文本中的換行符號，將 '\n' 替換為 \n
            string[] processedDialogue = new string[dialogueAfterBattle.Length];
            for (int i = 0; i < dialogueAfterBattle.Length; i++)
            {
                // 如果您在 Inspector 中輸入的是實際換行，這裡可能不需要替換 '\\n'
                // 但為了兼容性，這裡保留替換邏輯，確保 '\n' 被識別為換行
                processedDialogue[i] = dialogueAfterBattle[i].Replace("\\n", "\n");
            }
            yield return StartCoroutine(dialoguePanel.ShowDialogue(processedDialogue));
        }

        // 8. 房間完成
        currentStep = RoomStep.RoomComplete;
        isCompleted = true;
        completedRooms.Add(gameObject.name);
        
        // 打開所有門
        OpenAllDoors();

        if (roomCompletePanel != null) roomCompletePanel.SetActive(true);
        UIManager.Instance?.ShowRoomCompleteMenu();
        yield return new WaitForSeconds(1.5f);
        if (roomCompletePanel != null) roomCompletePanel.SetActive(false);

        // 9. 過場/切換房間
        currentStep = RoomStep.Transition;
        LevelFlowController.Instance?.OnLevelCompleted();
    }
} 