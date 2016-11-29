using System.ServiceModel;

namespace BlackTournament.Net.Contract
{
    [ServiceContract]
    public interface IGameClient : IClient
    {
        [OperationContract(IsOneWay = true)]
        void Shoot(int id);

        [OperationContract(IsOneWay = true)]
        void UpdatePosition(int id, float x, float y, float angle);

        [OperationContract(IsOneWay = true)]
        void ChangeLevel(string lvl);
    }
}