using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : Enemy
{
    private void FixedUpdate()
    {
        if (isTriggered)
        {
            //Move
            if (DistanceBetweenPlayer() > attackRange)
            {
                Movement(player.transform);

                //Increase speed (Run)
                speed = followSpeed;

                //Handle Run and Move Parameters
                animator.SetBool("Run", true);
            }

            //TrytoAttack
            Attack();

            //For Suspicious Time 
            timeSinceLastSawPlayer = 0;
        }
        else if (timeSinceLastSawPlayer < suspiciousTime)
        {
            //Decrease Speed
            speed = walkSpeed;

            //Handle Run and Move Parameters
            animator.SetBool("Run", false);
            //Wait
        }
        else
        {
            Patrol();
        }
        timeSinceLastSawPlayer += Time.fixedDeltaTime;
    }

    protected override void Movement(Transform target)
    {
        //Move
        rb.MovePosition(transform.position + TargetDirection(target) * speed * Time.fixedDeltaTime);

        SetForwardToTarget(target);

        //Handle Move Parameters
        if (!isTriggered)
        {
            animator.SetBool("Move", true);
        }
        else
        {
            animator.SetBool("Move", false);
        }
    }

    protected override void Attack()
    {
        SetForwardToTarget(player.transform);

        if (DistanceBetweenPlayer() < attackRange && canAttack)
        {
            //Handle Run and Move Parameters
            float forceMultiplier = 2;
            animator.SetBool("Run", false);
            animator.SetBool("Move", false);
            animator.SetTrigger("Attack");
            rb.AddForce(TargetDirection(player) * forceMultiplier);
            canAttack = false;
        }
        if (!canAttack)
        {
            timer += Time.deltaTime;
            if (timer > cooldown)
            {
                canAttack = true;
                canDealDamage = true;
                timer = 0;
            }
        }
    }


}
