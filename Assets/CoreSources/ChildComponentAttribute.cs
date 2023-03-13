using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ChildComponentAttribute : PropertyAttribute
{
	public readonly string childName;
	public readonly bool checkChildren;
	public readonly bool autoCreateChild;

	public ChildComponentAttribute(bool _checkChildren = false, bool _autoCreateChild = true)
	{
		childName = "Child";
		checkChildren = _checkChildren;
		autoCreateChild = _autoCreateChild;
	}
	public ChildComponentAttribute(string _childName, bool _checkChildren = false, bool _autoCreateChild = true)
	{
		childName = _childName;
		checkChildren = _checkChildren;
		autoCreateChild = _autoCreateChild;
	}
}