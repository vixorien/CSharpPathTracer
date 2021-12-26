using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpPathTracer
{
	interface IRayIntersectable
	{
		bool RayIntersection(Ray ray, out RayHit hit);
	}
}
