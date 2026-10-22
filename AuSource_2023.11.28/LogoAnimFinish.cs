using System;
using UnityEngine;

public class LogoAnimFinish : MonoBehaviour
{
	public Action FinishLogoAnim;

	public void FinishAnim()
	{
		FinishLogoAnim?.Invoke();
	}
}
