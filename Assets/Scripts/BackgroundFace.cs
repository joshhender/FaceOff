﻿using UnityEngine;
using System.Collections;

public class BackgroundFace : MonoBehaviour {

	public float speed, jumpForce;
	public LayerMask ground;
	public bool showPath;
	public float fireRate;

	[HideInInspector]
	public Transform[] path;
	[HideInInspector]
	public bool shouldAttack, facingRight, lost, engaged;
	[HideInInspector]
	public CharacterHealth health;
	[HideInInspector]
	public CharacterPower power;
	[HideInInspector]
	public int horizontal;
	[HideInInspector]
	public PlatformLevel level;
	[HideInInspector]
	public Weapon weapon;
	[HideInInspector]
	public Animator anim;
	[HideInInspector]
	public Rigidbody2D _rigidbody;
	[HideInInspector]
	public float distToGround;
	[HideInInspector]
	public Collider2D _collider;
	[HideInInspector]
	public SpecialPower SP;
	[HideInInspector]
	public Transform image;
	[HideInInspector]
	public BackgroundFace target;
	[HideInInspector]
	public BackgroundManager BM;
	[HideInInspector]
	public int team;

	bool toHigherLevel;
	Vector2 side, velocity;
	PathFinding pathFinding;
	bool canAttack = true;

	void Awake()
	{
		health = GetComponent<CharacterHealth>();
		power = GetComponent<CharacterPower>();
		SP = GetComponent<SpecialPower>();
	}

	void Start()
	{
		image = transform.FindChild("Image");
		level = GetComponent<PlatformLevel>();
		_rigidbody = GetComponent<Rigidbody2D>();
		_collider = GetComponent<CircleCollider2D>();
		weapon = GetComponentInChildren<Weapon>();
		anim = GetComponent<Animator>();
		pathFinding = GameObject.Find("Path").GetComponent<PathFinding>();
		distToGround = _collider.bounds.extents.y;
		horizontal = 1;
	}

	void Update()
	{
		if(!target)
		{
			//BM.ReplenishTeams();
			//Win();
			return;
		}
		if (horizontal > 0)
			image.localScale = new Vector3(1, 1, 1);
		else if (horizontal < 0)
			image.localScale = new Vector3(-1, 1, 1);

		if (shouldAttack && canAttack && !lost)
		{
			Attack();
		}
		if (anim.GetCurrentAnimatorStateInfo(0).IsName("PlayerIdle"))
			weapon.isAttacking = false;
		else
			weapon.isAttacking = true;

		if (target.level.level == level.level)
		{
			MoveToTarget();
			toHigherLevel = false;
			return;
		}

		if (level.level <= target.level.level)
		{
			Vector3 playerSide = target.transform.InverseTransformPoint(0, 0, 0);
			playerSide.Normalize();
			if (playerSide.x > 0)
			{
				path = pathFinding.rightPath;
				horizontal = 1;
			}
			else
			{
				path = pathFinding.leftPath;
				horizontal = -1;
			}
			toHigherLevel = true;
			MoveToNode(path[(int)level.level].position);
		}
		else
		{
			velocity = new Vector2(side.x * speed, _rigidbody.velocity.y);
			_rigidbody.velocity = velocity;
		}

		power.UpdatePowerSlider(engaged);
	}

	void MoveToTarget()
	{
		if (lost)
			return;
		side = transform.InverseTransformPoint(target.transform.position);
		side.Normalize();
		if (side.x > 0)
		{
			horizontal = 1;
		}
		else
		{
			horizontal = -1;
		}
		velocity = new Vector2(side.x * speed, _rigidbody.velocity.y);
		_rigidbody.velocity = velocity;
	}

	void MoveToNode(Vector2 node)
	{
		if (lost)
			return;
		Vector2 side = transform.InverseTransformPoint(node);
		side.Normalize();
		if (side.x > 0)
		{
			horizontal = 1;
		}
		else
		{
			horizontal = -1;
		}
		Vector2 velocity = new Vector2(side.x * speed, _rigidbody.velocity.y);
		_rigidbody.velocity = velocity;
	}

	void Attack()
	{
		if (power.power.num == power.power.max)
		{
			SP.Engage();
		}
		weapon.Attack(IsGrounded());
		canAttack = false;
		StartCoroutine(DelayAttack(fireRate));
	}

	void Jump()
	{
		if (lost)
			return;
		if (IsGrounded() && _rigidbody.velocity == new Vector2(_rigidbody.velocity.x, 0) && toHigherLevel)
			_rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
	}

	public void Lose()
	{
		if(team == 1)
		{
			BM.team1.Remove(this);
		}
		else
		{
			BM.team2.Remove(this);
		}
		Destroy(this.gameObject);
	}

	public void Win()
	{
		BM.ReplenishTeams();
	}

	IEnumerator DelayAttack(float delay)
	{
		yield return new WaitForSeconds(delay);
		canAttack = true;
	}

	public bool IsGrounded()
	{
		return Physics2D.Raycast(transform.position, -Vector3.up,
			distToGround + 0.1f, ground);
	}

	void OnDrawGizmos()
	{
		if (!showPath)
			return;
		if (path.Length > 0)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, path[0].transform.position);
			for (int i = 1; i < path.Length; i++)
			{
				Gizmos.DrawLine(path[i - 1].transform.position, path[i].transform.position);
			}
		}
	}

	void OnTriggerStay2D(Collider2D col)
	{
		if (lost)
			return;
		if (col.tag == "JumpBox")
		{
			Jump();
		}
	}
}
