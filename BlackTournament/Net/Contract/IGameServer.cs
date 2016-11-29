using System;
using System.ServiceModel;
namespace BlackTournament.Net.Contract
{
    [ServiceContract(CallbackContract = typeof(IGameClient))]
    public interface IGameServer : IServer
    {
        [OperationContract]
        string GetLevel();

        [OperationContract(IsOneWay = true)]
        void Shoot(int id);

        [OperationContract(IsOneWay = true)]
        void UpdatePosition(int id, float x, float y, float angle);

        [OperationContract(IsOneWay = true)]
        void ChangeLevel(int id, string lvl);
    }
}