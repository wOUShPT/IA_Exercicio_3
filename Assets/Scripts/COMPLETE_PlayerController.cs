using System.Collections;
using Unity.AI.Planner.Controller;
using UnityEngine;
using UnityEngine.AI;

public class COMPLETE_PlayerController : MonoBehaviour
{
    [SerializeField]private NavMeshAgent agent;
    [SerializeField]private DecisionController _decisionController;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        _decisionController = GetComponent<DecisionController>();
    }

    public IEnumerator MoveTo(GameObject player, GameObject target)
    {
        agent.SetDestination(target.transform.position);
        while (agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            yield return null;
        }
    }

    public void PickStick(GameObject stick)
    {
        Destroy(stick);
    }
}