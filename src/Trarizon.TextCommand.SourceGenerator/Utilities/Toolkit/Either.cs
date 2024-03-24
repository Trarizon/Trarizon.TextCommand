using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;
internal readonly struct Either<TLeft, TRight>
{
    private readonly bool _isLeft;
    private readonly TLeft? _left;
    private readonly TRight? _right;

    public Either(TLeft left)
    {
        _isLeft = true;
        _left = left;
    }
    public Either(TRight right)
    {
        _isLeft = false;
        _right = right;
    }

    public static implicit operator Either<TLeft, TRight>(TLeft left) => new(left);
    public static implicit operator Either<TLeft, TRight>(TRight right) => new(right);

    [MemberNotNullWhen(true, nameof(_left)), MemberNotNullWhen(false, nameof(_right))]
    public bool TryGetLeft([MaybeNullWhen(false)] out TLeft left, [MaybeNullWhen(true)] out TRight right)
    {
        left = _left;
        right = _right;
        return _isLeft;
    }
}
