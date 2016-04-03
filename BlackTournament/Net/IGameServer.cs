using System;
using System.ServiceModel;
namespace BlackTournament.Net
{
    [ServiceContract(CallbackContract = typeof(IGameClient))]
    interface IGameServer
    {
        [OperationContract(IsOneWay = true)]
        void Move(int id, float x, float y);
        [OperationContract]
        bool Subscribe();
    }
}