using System.ServiceModel;

namespace BlackTournament.Net
{
    [ServiceContract]
    interface IGameClient
    {
        [OperationContract(IsOneWay = true)]
        void Move(int id, float x, float y);
    }
}