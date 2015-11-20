﻿using UnityEngine;
using System.Collections;

public class MisCharacter : MonoBehaviour {

	protected Animator _animator;
	protected Vector2  _velocity;

	protected Vector2 _deltaPos;
	public Vector2 GetDeltaPos() { return _deltaPos; }

	protected bool  _isAttacking;
	protected bool  _isOnGround;
	protected bool  _isDead;
	protected float _moveX;

	public  float _moveSpeed;
	public  float _jumpSpeed;
	
	// Use this for initialization
	public virtual void Start () {
		
		_animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	public virtual void Update () {
	
		if (!_isDead) {

			// Add gravity and friction
			CalculateVelocity ();
		
			// Detect collisiong and modify velocity vector
			_deltaPos = DetectCollision ();

			// Detect collision and integrate velocity to position
			transform.position += (Vector3)_deltaPos;
		
			_animator.SetBool ("isRunning", IsRunning () && !_isAttacking);
			_animator.SetBool ("isJumping", IsJumping () && !_isAttacking);

			_animator.SetBool ("isShooting", _isAttacking);
		}
	}
	
	private void CalculateVelocity() {

		_velocity.x  = Mathf.Lerp(_velocity.x, _moveSpeed * _moveX, 15f * Time.deltaTime);
		_velocity.y += MisConstants.GRAVITY;

		_velocity.x = Mathf.Clamp (_velocity.x, -MisConstants.MAX_SPEED, MisConstants.MAX_SPEED);
		_velocity.y = Mathf.Clamp (_velocity.y, -MisConstants.MAX_SPEED, MisConstants.MAX_SPEED);
	}
	
	private Vector2 DetectCollision() {
		
		BoxCollider2D col = GetComponent<BoxCollider2D> ();
		
		Vector2 size = col.bounds.size;
		Vector2 center = col.offset;
		Vector2 entityPosition = transform.position;
		
		float correY = DetectVerticalCollition (entityPosition, center, size, MisConstants.COLLISION_RAYS);
		
		float correX = 0f;
		if(_velocity.x != 0f)
			correX = DetectHorizontalCollition (entityPosition, center, size, MisConstants.COLLISION_RAYS);

		return new Vector2(correX, correY);
	}
	
	private float DetectHorizontalCollition(Vector2 entityPosition, Vector2 center, Vector2 size, int nRays) {
		
		float deltaX = _velocity.x * Time.deltaTime;
		
		float dirX = transform.localScale.x;

		for (float i = 0f; i < nRays; i++)
			if(xAxisRaycasts (entityPosition, center, size, i, ref deltaX, dirX))
				break;

		return deltaX;
	}

	private bool xAxisRaycasts(Vector2 entityPosition, Vector2 center, Vector2 size, float i, ref float deltaX, float dirX) {
	
		float x = entityPosition.x + (center.x + size.x / 2f) * dirX;
		float y = (entityPosition.y + center.y - size.y / 2f) + size.y / 2 * i;
		
		Vector2 rayX = new Vector2 (x, y);
		
		RaycastHit2D hit = Physics2D.Raycast (rayX, new Vector2 (dirX, 0), Mathf.Abs (deltaX));
		Debug.DrawRay (rayX, new Vector2 (dirX, 0));
		
		if (hit.collider) {
			
			if (!hit.collider.isTrigger) {
				
				_velocity.x = 0f;
				
				float distance = Mathf.Abs (x - hit.point.x);
				
				if (distance >= MisConstants.PLAYER_SKIN)
					
					deltaX = (distance - MisConstants.PLAYER_SKIN) * dirX;
				else
					deltaX = 0f;
				
				return true;
			}
			
			TriggerEvent (hit.collider);
		}
		
		return false;
	}
	
	private float DetectVerticalCollition(Vector2 entityPosition, Vector2 center, Vector2 size, int nRays) {
		
		_isOnGround = false;
		
		float deltaY = _velocity.y * Time.deltaTime;

		float dirX = -transform.localScale.x;
		float dirY =  Mathf.Sign(_velocity.y);

		if (dirX == 1f) {
			for (float i = nRays - 1f; i >= 0f; i--)
				if(yAxisRaycasts (entityPosition, center, size, i, ref deltaY, dirX, dirY))
					break;
		} 
		else {
			for (float j = 0f; j < nRays; j++)
				if(yAxisRaycasts (entityPosition, center, size, j, ref deltaY, dirX, dirY))
					break;
		}
		
		return deltaY;
	}
	
	private bool yAxisRaycasts(Vector2 entityPosition, Vector2 center, Vector2 size, float i, ref float deltaY, float dirX, float dirY) {

		float x = (entityPosition.x + center.x * -dirX - size.x / 2f) + size.x / 2f * i;
		float y = entityPosition.y + center.y + size.y / 2f * dirY;
		
		Vector2 rayY = new Vector2(x, y);
		
		RaycastHit2D hit = Physics2D.Raycast(rayY, new Vector2(0, dirY), Mathf.Abs(deltaY));
		Debug.DrawRay(new Vector2(x, y),  new Vector2(0, dirY));

		if (hit.collider) {
				
			if (!hit.collider.isTrigger) {
			
				_isOnGround = true;
				_velocity.y = 0f;
			
				float distance = Mathf.Abs (y - hit.point.y);
			
				if (distance >= MisConstants.PLAYER_SKIN)

					deltaY = distance * dirY + MisConstants.PLAYER_SKIN;
				else
					deltaY = 0f;

				return true;
			}
		
			TriggerEvent (hit.collider);
		}

		return false;
	}

	protected virtual void TriggerEvent(Collider2D collider) {
	
	}

	protected bool IsFlipped() {
		
		return (transform.localScale.x == -1f);
	}
	
	protected bool IsJumping() {
		
		return (_velocity.y > 0f);
	}
	
	protected bool IsRunning() {
		
		return (_moveX != 0f);
	}

}
