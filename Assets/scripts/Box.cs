using UnityEngine;

public class Box : MonoBehaviour
{

    private Animator animator;
    private BoxState currentState;


    private void Awake()
    { 
        animator = GetComponent<Animator>();
    }
    void Start()
    {

        SetCurrentState(new Unpoened(this));
    }

    void Update()
    {
        currentState.Update();
    }

    void FixedUpdate()
    {
        currentState.FixedUpdate(); 

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        currentState.OnTriggerEnter2D(collision);
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        currentState.OnTriggerStay2D(collision);
    }
    public void PlayAnimation(string clip)
    {
        animator.Play(clip);
    }

    //�P�_�ʵe�O�_���񧹲�(�u�Ω�@���ʰʵe) 
    public bool IsAnimationDone(string _aniName)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return (stateInfo.IsName(_aniName) && stateInfo.normalizedTime >= 1.0);
    }


    //�]�m��e���A
    public void SetCurrentState(BoxState state)
    {
        currentState = state;
    }

}
