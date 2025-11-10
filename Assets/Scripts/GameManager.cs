using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Grid & Target Settings")]
    [SerializeField] private GridManager gridManager;

    [Header("Target Prefabs")]
    [SerializeField] private GameObject redTargetPrefab;
    [SerializeField] private GameObject blueTargetPrefab;

    [Header("Blocks")]
    [SerializeField] private GameObject redBlock;
    [SerializeField] private GameObject blueBlock;

    [System.Serializable]
    public class LevelTargetData
    {
        public Vector2Int redTarget;
        public Vector2Int blueTarget;
    }

    [SerializeField] private List<LevelTargetData> levelTargets = new List<LevelTargetData>();

    private int currentLevel = 0;
    private bool levelComplete = false;

    [Header("Audio")]
    [SerializeField] private AudioClip winSoundEffect;
    [SerializeField] private AudioClip startSoundEffect;
    [SerializeField] private AudioClip moveSoundEffect;

    private AudioSource audioSource;
    private bool audioPlayed = false;

    

    // Starting positions
    [SerializeField] private Vector2Int redStartPos = new Vector2Int(1, 4);
    [SerializeField] private Vector2Int blueStartPos = new Vector2Int(7, 4);

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (startSoundEffect != null)
            audioSource.PlayOneShot(startSoundEffect);

        StartLevel(currentLevel);
    }

    private void Update()
    {
        if (levelComplete && Input.GetKeyDown(KeyCode.Return))
            NextLevel();

        if (Input.GetKeyDown(KeyCode.R))
            RestartLevel();
    }

    // Level Management
    public void StartLevel(int levelIndex)
    {
        currentLevel = levelIndex;
        ClearTargets();
        SpawnTargets(levelTargets[levelIndex]);

        // Reset blocks to starting grid positions
        redBlock.GetComponent<Breakable>().ResetToStart(redStartPos);
        blueBlock.GetComponent<Breakable>().ResetToStart(blueStartPos);

        levelComplete = false;
        audioPlayed = false;

        Debug.Log($"Started Level {levelIndex + 1}");
    }

    private void SpawnTargets(LevelTargetData targetData)
    {
        SpawnTarget(redTargetPrefab, targetData.redTarget, "Red");
        SpawnTarget(blueTargetPrefab, targetData.blueTarget, "Blue");
    }

    private void SpawnTarget(GameObject prefab, Vector2Int pos, string colorName)
    {
        if (pos.x >= 0 && pos.x < gridManager.gridList.Count &&
            pos.y >= 0 && pos.y < gridManager.gridList[0].Count)
        {
            GameObject cellObj = gridManager.gridList[pos.x][pos.y];
            GameObject targetObj = Instantiate(prefab, cellObj.transform);
            targetObj.transform.localPosition = Vector3.zero;

            Debug.Log($"Spawned {colorName} target at grid ({pos.x},{pos.y})");
        }
    }

    private void ClearTargets()
    {
        foreach (var col in gridManager.gridList)
        {
            foreach (var cellObj in col)
            {
                foreach (Target t in cellObj.GetComponentsInChildren<Target>())
                    Destroy(t.gameObject);
            }
        }
    }

    public void NextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        //if you reach the last scene -> loop back to the first level
        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
            nextSceneIndex = 0;

        SceneManager.LoadScene(nextSceneIndex);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //Win Condition
    private void CheckWinCondition()
    {
        Vector2Int redPos = redBlock.GetComponent<Breakable>().gridPos;
        Vector2Int bluePos = blueBlock.GetComponent<Breakable>().gridPos;

        LevelTargetData targets = levelTargets[currentLevel];

        Debug.Log($"Checking Win: Red {redPos} vs {targets.redTarget}, Blue {bluePos} vs {targets.blueTarget}");

        if (redPos == targets.redTarget && bluePos == targets.blueTarget)
        {
            levelComplete = true;

            if (!audioPlayed && winSoundEffect != null)
            {
                audioPlayed = true;
                audioSource.PlayOneShot(winSoundEffect);
            }

            Debug.Log("🎉 Level Complete!");

            StartCoroutine(NextLevelAfterDelay(1.8f));
        }
    }

    //Only check win condition when the blocks are moved
    public void OnBlockMoved()
    {
        if(redBlock != null && blueBlock != null)
            CheckWinCondition();

        if (moveSoundEffect != null && audioSource != null)
            audioSource.PlayOneShot(moveSoundEffect);
    }

    private System.Collections.IEnumerator NextLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        NextLevel();
    }
}
