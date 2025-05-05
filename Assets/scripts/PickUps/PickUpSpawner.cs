using UnityEngine;

public class PickUpSpawner : MonoBehaviour
{
    public PropPrefab[] propPrefabs; //儲存不同道具的預制體
    //開始生成掉落道具
    public void DropItems()
    {
        foreach(var propPrefab in propPrefabs)
        {
            if(Random.Range(0f, 100f)<=propPrefab.dropPercentage)//根據機率來
            {
                Instantiate(propPrefab.prefab,transform.position, Quaternion.identity);
            }
        }
    
    }

}

[System.Serializable] //沒有MonoBehaviour，想在編輯器上顯示就加[System.Serializable]
public class PropPrefab
{
    public GameObject prefab;//掉落道具預制體
    [Range(0f,100f)]public float dropPercentage;//掉落機率 0~100%
}
