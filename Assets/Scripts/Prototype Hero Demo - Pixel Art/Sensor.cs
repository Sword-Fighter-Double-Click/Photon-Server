using System;
using UnityEngine;

public class Sensor : MonoBehaviour
{
	private int m_ColCount = 0;

	private float m_DisableTimer;

	private void OnEnable()
	{
		m_ColCount = 0;
	}

	public bool State()
	{
		if (m_DisableTimer > 0)
			return false;
		return m_ColCount > 0;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.layer != 8) return;

		m_ColCount++;
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.layer != 8) return;

		m_ColCount--;
	}

	private void Update()
	{
		m_DisableTimer -= Time.deltaTime;
	}

	public void Disable(float duration)
	{
		m_DisableTimer = duration;
	}
}
