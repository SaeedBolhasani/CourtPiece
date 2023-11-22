public interface IPlayer : IGrainWithIntegerKey
{
    Task<string> GetFullName();
}


