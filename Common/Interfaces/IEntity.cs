namespace Common.Interfaces
{
    public interface IEntity
    {
        private static readonly Dictionary<char, char> map = new Dictionary<char, char> {
            {'(', ')' }, {'{', '}'}, {'[', ']'}
        };
    }
}
