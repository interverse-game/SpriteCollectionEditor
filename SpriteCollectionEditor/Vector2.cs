using System;
using System.Text.Json.Serialization;

namespace TML.Engine;
public struct Vector2 : IEquatable<Vector2> {
	public Vector2(float x, float y) {
		this.x = x;
		this.y = y;
	}
	[JsonIgnore]
	public float x;
	[JsonIgnore]
	public float y;

	[JsonInclude]
	public float X {
		get => x;
		set => x = value;
	}
	[JsonInclude]
	public float Y {
		get => y;
		set => y = value;
	}

	public override bool Equals(object? obj) {
		if (obj == null) {
			return false;
		}
		if (obj is not Vector2 vec) {
			return false;
		}
		return Equals(vec);
	}
	public bool Equals(Vector2 obj) {
		return x == obj.x
			&& y == obj.y;
	}
	public override int GetHashCode() {
		return HashCode.Combine(x, y);
	}
	public static bool operator ==(Vector2 a, Vector2 b) {
		return a.Equals(b);
	}
	public static bool operator !=(Vector2 a, Vector2 b) {
		return !(a.Equals(b));
	}


	[JsonIgnore]
	public float LengthSquared => x * x + y * y;
	[JsonIgnore]
	public float Length => MathF.Sqrt(LengthSquared);
	[JsonIgnore]
	public Vector2 Normalized {
		get {
			float len = Length;
			return new Vector2(x / len, y / len);
		}
	}
	public void Normalize() {
		float len = Length;
		x /= len;
		y /= len;
	}

	public static Vector2 Zero => new(0, 0);
	public static Vector2 One => new(1, 1);
	public static Vector2 Up => new(0, 1);
	public static Vector2 Down => new(0, -1);
	public static Vector2 Left => new(-1, 0);
	public static Vector2 Right => new(1, 0);
	public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.x + b.x, a.y + b.y);
	public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.x - b.x, a.y - b.y);
	public static Vector2 operator *(Vector2 a, Vector2 b) => new(a.x * b.x, a.y * b.y);
	public static Vector2 operator /(Vector2 a, Vector2 b) => new(a.x / b.x, a.y / b.y);
	public static Vector2 operator *(Vector2 a, float b) => new(a.x * b, a.y * b);
	public static Vector2 operator /(Vector2 a, float b) => new(a.x / b, a.y / b);

	public static Vector2 Lerp(Vector2 a, Vector2 b, float time) => (b - a) * time + a;
	public static float Dot(Vector2 a, Vector2 b) {
		return (a.x * b.x + a.y * b.y);
	}
	public static float Cross(Vector2 a, Vector2 b) {
		return (a.x * b.y - a.y * b.x);
	}
	public static float AngleBetween(Vector2 a, Vector2 b) {
		return MathF.Acos(Dot(a.Normalized, b.Normalized));
	}

	public override string ToString() {
		return string.Format("({0}, {1})", x, y);
	}
}