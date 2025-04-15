using UnityEngine;


public abstract class BoxState
{
    public Box box;


    public BoxState(Box _box)
    {
        box = _box;
    }

    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void OnTriggerEnter2D(Collider2D collision);
    public abstract void OnTriggerStay2D(Collider2D collision);
}


public class Unpoened : BoxState
{
    public Unpoened(Box _box) : base(_box)
    { 
        //¼½©ñ°Êµe
        box.PlayAnimation("idle"); 
    }

    public override void Update()
    {

    }


    public override void FixedUpdate()
    {

    }


    public override void OnTriggerEnter2D(Collider2D collision)
    {

    }

    
    public override void OnTriggerStay2D(Collider2D collision)
    {

    }



}




