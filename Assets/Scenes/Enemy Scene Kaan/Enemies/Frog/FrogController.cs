using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogController : MonoBehaviour
{

    public GameObject frog;
    public GameObject frogsBody;
    SkinnedMeshRenderer skinnedMeshRenderer;

    Animator anim;

    public Material blue;
    public Material balckOnRedSpot;
    public Material orangeBlackBlue;
    public Material redGreenBlack;
    public Material yellow;
    public Material yellowOnBlack;

    public GameObject guts;
    GameObject gutsEx;
    bool smashed = false;

    private void Awake()
    {
        anim = frog.GetComponent<Animator>();
        skinnedMeshRenderer = frogsBody.GetComponent<SkinnedMeshRenderer>();
    }


    public void Idle()
    {
        RootMotion();
        DestroyGuts();
        anim.SetTrigger("Idle");
    }

    public void Jump()
    {
        RootMotion();
        DestroyGuts();
        anim.SetTrigger("Jump");
    }

    public void Crawl()
    {
        RootMotion();
        DestroyGuts();
        anim.SetTrigger("Crawl");
    }

    public void Tongue()
    {
        RootMotion();
        DestroyGuts();
        anim.SetTrigger("Tongue");
    }

    public void Swim()
    {
        RootMotion();
        DestroyGuts();
        anim.SetTrigger("Swim");
    }

    public void Smashed()
    {
        RootMotion();
        DestroyGuts();
        anim.SetTrigger("Smashed");
        Guts();
    }

    public void TurnLeft()
    {
        anim.applyRootMotion = true;
        DestroyGuts();
        anim.SetTrigger("TurnLeft");
    }

    public void TurnRight()
    {
        anim.applyRootMotion = true;
        DestroyGuts();
        anim.SetTrigger("TurnRight");
    }

    public void Guts()
    {
        Invoke("SpreadGuts", 0.1f);
    }

    void SpreadGuts()
    {
        smashed = false;
        if (!smashed)
        {
            Instantiate(guts, frog.transform.position, frog.transform.rotation);
            smashed = true;
        }
    }

    void RootMotion()
    {
        if (anim.applyRootMotion)
        {
            anim.applyRootMotion = false;
        }
    }


    void DestroyGuts()
    {
        gutsEx = GameObject.FindGameObjectWithTag("Guts");
        if (gutsEx != null)
        {
            Destroy(gutsEx);
            smashed = false;
        }
    }


    public void Blue()
    {
        skinnedMeshRenderer.material = blue;
    }
    public void BalckOnRedSpot()
    {
        skinnedMeshRenderer.material = balckOnRedSpot;
    }
    public void OrangeBlackBlue()
    {
        skinnedMeshRenderer.material = orangeBlackBlue;
    }
    public void RedGreenBlack()
    {
        skinnedMeshRenderer.material = redGreenBlack;
    }
    public void Yellow()
    {
        skinnedMeshRenderer.material = yellow;
    }
    public void YellowOnBlack()
    {
        skinnedMeshRenderer.material = yellowOnBlack;
    }

}