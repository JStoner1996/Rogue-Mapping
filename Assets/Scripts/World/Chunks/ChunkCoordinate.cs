using System;

// The integer grid coordinate used to identify a generated chunk.
[Serializable]
public readonly struct ChunkCoordinate : IEquatable<ChunkCoordinate>
{
    public readonly int x;
    public readonly int y;

    public ChunkCoordinate(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public bool Equals(ChunkCoordinate other)
    {
        return x == other.x && y == other.y;
    }

    public override bool Equals(object obj)
    {
        return obj is ChunkCoordinate other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (x * 397) ^ y;
        }
    }

    public override string ToString()
    {
        return $"({x}, {y})";
    }
}
