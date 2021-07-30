namespace BusinessLayer.Abstract
{
    public class BrokenRule
    {
        public BrokenRule(string member, string message)
        {
            Member = member;
            Message = message;
        }
        public BrokenRule(string message)
        {
            Message = message;
        }

        public string? Member {get; }
        public string Message {get; }
    }
}
