using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyDebug : Debug
{
	public static void DrawRect(Vector3 center, Vector3 size)
	{
		bool depthTest = true;
		float duration = 0f;
		Color color = Color.white;
		DrawRect(center, size, color, duration, depthTest);
	}
	public static void DrawRect(Vector3 center, Vector3 size, Color color)
	{
		bool depthTest = true;
		float duration = 0f;
		DrawRect(center, size, color, duration, depthTest);
	}
	public static void DrawRect(Vector3 center, Vector3 size, Color color, float duration)
	{
		bool depthTest = true;
		DrawRect(center, size, color, duration, depthTest);
	}
	public static void DrawRect(Vector3 center, Vector3 size, Color color, float duration, bool depthTest)
	{
		#region 변수 선언
		Vector3 left = Vector3.left * size.x * 0.5f;
		Vector3 right = Vector3.right * size.x * 0.5f;
		Vector3 top = Vector3.up * size.y * 0.5f;
		Vector3 bottom = Vector3.down * size.y * 0.5f;
		Vector3 front = Vector3.forward * size.z * 0.5f;
		Vector3 back = Vector3.back * size.z * 0.5f;

		Vector3 frontLeftTop, frontRightTop, frontLeftBottom, frontRightBottom;
		Vector3 backLeftTop, backRightTop, backLeftBottom, backRightBottom;

		frontLeftTop = center + front + left + top;
		frontRightTop = center + front + right + top;
		frontLeftBottom = center + front + left + bottom;
		frontRightBottom = center + front + right + bottom;

		backLeftTop = center + back + left + top;
		backRightTop = center + back + right + top;
		backLeftBottom = center + back + left + bottom;
		backRightBottom = center + back + right + bottom;
		#endregion

		Debug.DrawLine(frontLeftTop, frontRightTop, color, duration, depthTest);
		Debug.DrawLine(frontRightTop, frontRightBottom, color, duration, depthTest);
		Debug.DrawLine(frontRightBottom, frontLeftBottom, color, duration, depthTest);
		Debug.DrawLine(frontLeftBottom, frontLeftTop, color, duration, depthTest);

		Debug.DrawLine(backLeftTop, backRightTop, color, duration, depthTest);
		Debug.DrawLine(backRightTop, backRightBottom, color, duration, depthTest);
		Debug.DrawLine(backRightBottom, backLeftBottom, color, duration, depthTest);
		Debug.DrawLine(backLeftBottom, backLeftTop, color, duration, depthTest);

		Debug.DrawLine(frontLeftTop, backLeftTop, color, duration, depthTest);
		Debug.DrawLine(frontRightTop, backRightTop, color, duration, depthTest);
		Debug.DrawLine(frontRightBottom, backRightBottom, color, duration, depthTest);
		Debug.DrawLine(frontLeftBottom, backLeftBottom, color, duration, depthTest);
	}
}