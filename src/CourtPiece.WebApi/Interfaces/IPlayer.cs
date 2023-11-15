public interface IPlayer : IGrainWithIntegerKey
{
    Task Join(IRoom room);

    //Task Action(CourtPiece.Common.Model.Card card);
}


