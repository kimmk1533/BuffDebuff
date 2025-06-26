using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeReadOnlyAttribute : PropertyAttribute
{
	public readonly bool runtimeOnly;

	public RuntimeReadOnlyAttribute(bool runtimeOnly = false)
	{
		this.runtimeOnly = runtimeOnly;
	}
}