using UnityEngine;

public class CourseStarBehaviour : MonoBehaviour
{
	public SpriteRenderer Upper;

	public SpriteRenderer Lower;

	public float Speed = 30f;

	public void Update()
	{
		((Component)Upper).transform.Rotate(0f, 0f, Time.deltaTime * Speed);
		((Component)Lower).transform.Rotate(0f, 0f, Time.deltaTime * Speed);
	}
}
