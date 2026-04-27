using UnityEngine;

namespace Assets.PixelFantasy.PixelTileEngine.Scripts
{
	public class RepeatingBackground : MonoBehaviour
	{
		[SerializeField] private float _speed;

		private float _width;

		public void Start()
		{
			_width = GetComponent<SpriteRenderer>().size.x * transform.localScale.x;
		}

		/// <summary>
		/// The simplest background move example. You should use advanced techniques to make the movement smooth.
		/// </summary>
		public void Update()
		{
			transform.localPosition += _speed * Vector3.right * Time.deltaTime;
			Repeat();
		}

		private void Repeat()
		{
			if (transform.localPosition.x >= _width / 4)
			{
				transform.localPosition += Vector3.left * _width / 2;
			}
			else if (transform.localPosition.x <= -_width / 4)
			{
				transform.localPosition += Vector3.right * _width / 2;
			}
		}
	}
}