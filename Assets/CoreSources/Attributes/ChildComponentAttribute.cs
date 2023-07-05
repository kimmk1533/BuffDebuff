using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Field (with this Attribute) requires Serialized.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ChildComponentAttribute : PropertyAttribute
{
	public readonly string childName;
	public readonly bool showInInspector;
	public readonly bool autoCreateChild;

	public ChildComponentAttribute(bool _showInInspector = true, bool _autoCreateChild = true)
	{
		childName = "Child";
		showInInspector = _showInInspector;
		autoCreateChild = _autoCreateChild;
	}
	public ChildComponentAttribute(string _childName, bool _showInInspector = true, bool _autoCreateChild = true)
	{
		childName = _childName;
		showInInspector = _showInInspector;
		autoCreateChild = _autoCreateChild;
	}
}