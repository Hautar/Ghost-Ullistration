using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostImage : MonoBehaviour
{
    [SerializeField] private GameObject ghostPlayer;

    private List<GameObject> kolobokPool;  // pool of reusable ghost player objects
    private List<Transform> pivotPool;  // reference to each pivot point on a ghost objects
    private Transform pivot;  // reference to the player's weapon pivot for gun rotation    

    [SerializeField] private float ghostLifeTime = 0.15f;
    private int ghostQuantity = 5; // always 5, because it looks good;

    public float AllGhostGap()
    {
        return ghostLifeTime;
    }

    // spawns ghosts and invokes looping through them to update each ghost position and rotation every ghostLifeTime seconds
    void Start()
    {
        SpawnGhostsAddThemToPool();
        InvokeRepeating("Initiator", 0f, ghostLifeTime);  // delay is always zero
    }

    private void SpawnGhostsAddThemToPool()
    {
        pivot = transform.GetChild(0);
        kolobokPool = new List<GameObject>();
        pivotPool = new List<Transform>();
        for (int k = 0; k < ghostQuantity; k++)
        {
            kolobokPool.Add(Instantiate(ghostPlayer, transform.position, Quaternion.identity)); // add ghost to pool
            // player sorting layer is 10 and their gun is 11
            kolobokPool[k].GetComponent<SpriteRenderer>().sortingOrder = 2 * k;  // set ghost sorting layers to 0, 2, 4, 6, 8
            pivotPool.Add(kolobokPool[k].transform.GetChild(0)); // get pivot to rotate it like player's pivot
            pivotPool[k].transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 2 * k + 1; // get weapon obj, set it layer to 1, 3, 5, 7, 9
            kolobokPool[k].SetActive(false);
        }
    }

    public void Initiator()
    {
        for (int k = 0; k < ghostQuantity; k++)
        {
            float spawnDelay = k * ghostLifeTime / ghostQuantity;
            StartCoroutine(Wait(spawnDelay, k));
        }
    }

    private IEnumerator Wait(float waitTime, int ghostNumberInPool)
    {
        yield return new WaitForSeconds(waitTime);
        kolobokPool[ghostNumberInPool].SetActive(false);
        kolobokPool[ghostNumberInPool].transform.position = transform.position;
        pivotPool[ghostNumberInPool].rotation = pivot.rotation;
        kolobokPool[ghostNumberInPool].SetActive(true);
    }


    private void OnEnable()
    {
        EventManager.OnPlayerDied += DisableGhosts;
        EventManager.OnPlayerDamaged += DisableGhostsWithReInvoke;
    }

    private void OnDisable()
    {
        EventManager.OnPlayerDied -= DisableGhosts;
        EventManager.OnPlayerDamaged -= DisableGhostsWithReInvoke;
    }

    private void DisableGhostsWithReInvoke(float invulnerabilityTime)
    {
        StartCoroutine(CancelAndReinvokeInitiator(invulnerabilityTime));
    }

    IEnumerator CancelAndReinvokeInitiator(float invulnerabilityTime)
    {
        DisableGhosts(0f);
        yield return new WaitForSeconds(invulnerabilityTime);
        InvokeRepeating("Initiator", 0f, ghostLifeTime);
    }

    private void DisableGhosts(float param)
    {
        CancelInvoke("Initiator");
        for (int k = 0; k < kolobokPool.Count; k++)
        {
            kolobokPool[k].SetActive(false);
        }
    }
}