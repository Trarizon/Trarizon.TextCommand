using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.Utilities;
internal static class EnumerableExtensions
{
    public static IndexedEnumerator<T> GetIndexedEnumerator<TEnumerable, T>(TEnumerable source) where TEnumerable : IEnumerable<T>
        => new(source.GetEnumerator());

    public struct IndexedEnumerator<T>(IEnumerator<T> enumerator) : IDisposable
    {
        private int _index = -1;

        public readonly int IteratedCount => _index + 1;

        public bool TryMoveNext(out int index, [MaybeNullWhen(false)] out T current)
        {
            if (enumerator.MoveNext()) {
                _index++;
                index = _index;
                current = enumerator.Current;
                return true;
            }
            index = default;
            current = default;
            return false;
        }

        public readonly void Dispose()
        {
            enumerator.Dispose();
        }
    }
}
