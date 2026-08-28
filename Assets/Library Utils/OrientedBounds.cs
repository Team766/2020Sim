using UnityEngine;
using System;

public struct OrientedBounds : IEquatable<OrientedBounds>, IFormattable
{
	/// <summary>
	///   <para>The center of the bounding box.</para>
	/// </summary>
	public Vector3 center;
	/// <summary>
	///   <para>The orientation of the box.</para>
	/// </summary>
	public Quaternion rotation;
	/// <summary>
	///   <para>The total size of the box. This is always twice as large as the extents.</para>
	/// </summary>
	public Vector3 size;

	/// <summary>
	///   <para>The extents of the Bounding Box. This is always half of the size of the Bounds.</para>
	/// </summary>
	public Vector3 extents
	{
		get => size / 2f;
		set => size = value * 2f;
	}

	/// <summary>
	///   <para>Creates a new Bounds.</para>
	/// </summary>
	/// <param name="center">The location of the origin of the Bounds.</param>
	/// <param name="size">The dimensions of the Bounds.</param>
	public OrientedBounds(Vector3 center, Quaternion rotation, Vector3 size)
    {
		this.center = center;
		this.rotation = rotation;
		this.size = size;
    }

	public OrientedBounds(Bounds bounds)
    {
		this.center = bounds.center;
		this.rotation = Quaternion.identity;
		this.size = bounds.size;
    }

	public override int GetHashCode()
    {
		return HashCode.Combine(center.GetHashCode(), rotation.GetHashCode(), size.GetHashCode());
	}

	public override bool Equals(object obj) => obj is OrientedBounds other && this.Equals(other);

	public bool Equals(OrientedBounds other)
    {
        return center == other.center && rotation == other.rotation && size == other.size;
    }

	public static bool operator ==(OrientedBounds lhs, OrientedBounds rhs) => lhs.Equals(rhs);

	public static bool operator !=(OrientedBounds lhs, OrientedBounds rhs) => !(lhs == rhs);

    /// <summary>
    ///   <para>Grows the Bounds to include the point.</para>
    /// </summary>
    /// <param name="point"></param>
    //public void Encapsulate(Vector3 point)
	//{
	//	var localPoint = Quaternion.Inverse(rotation) * (point - center);
	//	var localBounds = new Bounds(Vector3.zero, size);
	//	localBounds.Encapsulate(localPoint);
	//	center += rotation * localBounds.center;
	//	size = localBounds.size;
	//}

    /// <summary>
    ///   <para>Grow the bounds to encapsulate the bounds.</para>
    /// </summary>
    /// <param name="bounds"></param>
    //public void Encapsulate(Bounds bounds);

    /// <summary>
    ///   <para>Does another bounding box intersect with this bounding box?</para>
    /// </summary>
    /// <param name="bounds"></param>
    //public bool Intersects(Bounds bounds);

    /// <summary>
    ///   <para>Returns a formatted string for the bounds.</para>
    /// </summary>
    /// <param name="format">A numeric format string.</param>
    /// <param name="formatProvider">An object that specifies culture-specific formatting.</param>
    public override string ToString()
    {
		return $"OrientedBounds{{center {center} rotation {rotation} size {size}}}";
    }

	/// <summary>
	///   <para>Returns a formatted string for the bounds.</para>
	/// </summary>
	/// <param name="format">A numeric format string.</param>
	public string ToString(string format)
    {
		return $"OrientedBounds{{center {center.ToString(format)} rotation {rotation.ToString(format)} size {size.ToString(format)}}}";
	}

	/// <summary>
	///   <para>Returns a formatted string for the bounds.</para>
	/// </summary>
	/// <param name="format">A numeric format string.</param>
	/// <param name="formatProvider">An object that specifies culture-specific formatting.</param>
	public string ToString(string format, IFormatProvider formatProvider)
    {
		return $"OrientedBounds{{center {center.ToString(format, formatProvider)} rotation {rotation.ToString(format, formatProvider)} size {size.ToString(format, formatProvider)}}}";
	}

	/// <summary>
	///   <para>Is point contained in the bounding box?</para>
	/// </summary>
	/// <param name="point"></param>
	public bool Contains(Vector3 point)
    {
		var localPoint = Quaternion.Inverse(rotation) * (point - center);
		return new Bounds(Vector3.zero, size).Contains(localPoint);
	}

	/// <summary>
	///   <para>The smallest squared distance between the point and this bounding box.</para>
	/// </summary>
	/// <param name="point"></param>
	public float SqrDistance(Vector3 point)
    {
		var localPoint = Quaternion.Inverse(rotation) * (point - center);
		return new Bounds(Vector3.zero, size).SqrDistance(localPoint);
    }

	/// <summary>
	///   <para>The closest point on the bounding box.</para>
	/// </summary>
	/// <param name="point">Arbitrary point.</param>
	/// <returns>
	///   <para>The point on the bounding box or inside the bounding box.</para>
	/// </returns>
	public Vector3 ClosestPoint(Vector3 point)
    {
		var localPoint = Quaternion.Inverse(rotation) * (point - center);
		var localProjection = new Bounds(Vector3.zero, size).ClosestPoint(localPoint);
		return rotation * localProjection + center;
	}
}