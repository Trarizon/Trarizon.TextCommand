using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;
internal readonly struct Either<TLeft, TRight>
{
    private readonly bool _isLeft;
    private readonly TLeft? _left;
    private readonly TRight? _right;

    public TLeft? Left => _left;
    public TRight? Right => _right;

    [MemberNotNullWhen(true, nameof(_left), nameof(Left))]
    [MemberNotNullWhen(false, nameof(_right), nameof(Right))]
    public bool IsLeft => _isLeft;

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

    [MemberNotNullWhen(true, nameof(Left)), MemberNotNullWhen(false, nameof(Right))]
    public bool TryGetLeft([MaybeNullWhen(false)] out TLeft left)
    {
        if (IsLeft) {
            left = _left;
            return true;
        }
        left = default;
        return false;
    }

    [MemberNotNullWhen(false, nameof(Left)), MemberNotNullWhen(true, nameof(Right))]
    public bool TryGetRight([MaybeNullWhen(false)] out TRight right)
    {
        if (IsLeft) {
            right = default;
            return false;
        }
        right = _right;
        return true;
    }
}
