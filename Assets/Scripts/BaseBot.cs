using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Scanner), typeof(SpawnBot))]
public class BaseBot : MonoBehaviour
{
    [SerializeField] private float _resourceCollectionDelay = 0.1f;
    [SerializeField] private float _spawnRadius = 3f;
    private int _resourceCount = 0;

    private List<Unit> _bots = new List<Unit>();
    private Dictionary<Resource, bool> _resourceStates = new Dictionary<Resource, bool>();

    private SpawnBot _createBot;
    private Scanner _scanner;

    public event UnityAction<int> ResourcesChanged;

    private void Awake()
    {
        _scanner = GetComponent<Scanner>();
        _createBot = GetComponent<SpawnBot>();
    }

    private void Start()
    {
        CreateBot();
        StartCoroutine(CollectResourcesRoutine());
    }

    public void TakeResource(Resource resource)
    {
        _resourceStates[resource] = false;
        _resourceCount++;
        ResourcesChanged?.Invoke(_resourceCount);
    }

    private void CreateBot()
    {
        int startCount = 3;

        for (int i = 0; i < startCount; i++)
        {
            float randomX = Random.Range(-_spawnRadius, _spawnRadius);
            float randomZ = Random.Range(-_spawnRadius, _spawnRadius);
            Vector3 randomPosition = transform.position + new Vector3(randomX, 0, randomZ);
            Unit bot = _createBot.Spawn(randomPosition);
            bot.SetBaseBot(this);
            _bots.Add(bot);
        }
    }

    private IEnumerator CollectResourcesRoutine()
    {
        var waitSeconds = new WaitForSeconds(_resourceCollectionDelay);

        while (true)
        {
            yield return waitSeconds;
            CollectResource();
        }
    }

    private void CollectResource()
    {
        Resource resource = _scanner.GetAllResources().FirstOrDefault(resource => !_resourceStates.ContainsKey(resource) || !_resourceStates[resource]);

        if (resource != null)
        {
            foreach (Unit bot in _bots)
            {
                if (!bot._isBusy)
                {
                    bot.SetDestination(resource);
                    _resourceStates[resource] = true;
                    break;
                }
            }
        }
    }
}
