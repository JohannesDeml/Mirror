using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirror.Tests.RemoteAttrributeTest
{
    class TargetRpcBehaviour : NetworkBehaviour
    {
        public event Action<int> onSendInt;

        [TargetRpc]
        public void SendInt(int someInt)
        {
            onSendInt?.Invoke(someInt);
        }

        [TargetRpc]
        public void SendIntWithTarget(NetworkConnection target, int someInt)
        {
            onSendInt?.Invoke(someInt);
        }
    }

    public class TargetRpcTest : RemoteTestBase
    {
        [Test]
        public void TargetRpcIsCalled()
        {
            // spawn with owner
            CreateNetworkedAndSpawn(out GameObject _, out NetworkIdentity _, out TargetRpcBehaviour hostBehaviour, NetworkServer.localConnection);

            const int someInt = 20;

            int callCount = 0;
            hostBehaviour.onSendInt += incomingInt =>
            {
                callCount++;
                Assert.That(incomingInt, Is.EqualTo(someInt));
            };
            hostBehaviour.SendInt(someInt);
            ProcessMessages();
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void TargetRpcIsCalledOnTarget()
        {
            // spawn without owner
            CreateNetworkedAndSpawn(out GameObject _, out NetworkIdentity _, out TargetRpcBehaviour hostBehaviour);

            const int someInt = 20;

            int callCount = 0;
            hostBehaviour.onSendInt += incomingInt =>
            {
                callCount++;
                Assert.That(incomingInt, Is.EqualTo(someInt));
            };
            hostBehaviour.SendIntWithTarget(NetworkServer.localConnection, someInt);
            ProcessMessages();
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void ErrorForTargetRpcWithNoOwner()
        {
            // spawn without owner
            CreateNetworkedAndSpawn(out GameObject _, out NetworkIdentity _, out TargetRpcBehaviour hostBehaviour);

            const int someInt = 20;

            hostBehaviour.onSendInt += incomingInt =>
            {
                Assert.Fail("Event should not be invoked with error");
            };
            LogAssert.Expect(LogType.Error, $"TargetRPC {nameof(TargetRpcBehaviour.SendInt)} was given a null connection, make sure the object has an owner or you pass in the target connection");
            hostBehaviour.SendInt(someInt);
        }

        [Test]
        public void ErrorForTargetRpcWithNullArgment()
        {
            // spawn without owner
            CreateNetworkedAndSpawn(out GameObject _, out NetworkIdentity _, out TargetRpcBehaviour hostBehaviour);

            const int someInt = 20;

            hostBehaviour.onSendInt += incomingInt =>
            {
                Assert.Fail("Event should not be invoked with error");
            };
            LogAssert.Expect(LogType.Error, $"TargetRPC {nameof(TargetRpcBehaviour.SendIntWithTarget)} was given a null connection, make sure the object has an owner or you pass in the target connection");
            hostBehaviour.SendIntWithTarget(null, someInt);
        }

        [Test]
        public void ErrorForTargetRpcWhenNotGivenConnectionToClient()
        {
            // spawn without owner
            CreateNetworkedAndSpawn(out GameObject _, out NetworkIdentity _, out TargetRpcBehaviour hostBehaviour);

            const int someInt = 20;

            hostBehaviour.onSendInt += incomingInt =>
            {
                Assert.Fail("Event should not be invoked with error");
            };
            LogAssert.Expect(LogType.Error, $"TargetRPC {nameof(TargetRpcBehaviour.SendIntWithTarget)} requires a NetworkConnectionToClient but was given {typeof(FakeConnection).Name}");
            hostBehaviour.SendIntWithTarget(new FakeConnection(), someInt);
        }
        class FakeConnection : NetworkConnection
        {
            public override string address => throw new NotImplementedException();
            public override void Disconnect() => throw new NotImplementedException();
            internal override void Send(ArraySegment<byte> segment, int channelId = 0) => throw new NotImplementedException();
        }

        [Test]
        public void ErrorForTargetRpcWhenServerNotActive()
        {
            // spawn without owner
            CreateNetworkedAndSpawn(out GameObject _, out NetworkIdentity _, out TargetRpcBehaviour hostBehaviour);

            const int someInt = 20;

            hostBehaviour.onSendInt += incomingInt =>
            {
                Assert.Fail("Event should not be invoked with error");
            };
            NetworkServer.active = false;
            LogAssert.Expect(LogType.Error, $"TargetRPC {nameof(TargetRpcBehaviour.SendInt)} called when server not active");
            hostBehaviour.SendInt(someInt);
        }

        [Test]
        public void ErrorForTargetRpcWhenObjetNotSpawned()
        {
            // create without spawning
            CreateNetworked(out GameObject _, out NetworkIdentity _, out TargetRpcBehaviour hostBehaviour);

            const int someInt = 20;

            hostBehaviour.onSendInt += incomingInt =>
            {
                Assert.Fail("Event should not be invoked with error");
            };
            LogAssert.Expect(LogType.Warning, $"TargetRpc {nameof(TargetRpcBehaviour.SendInt)} called on {hostBehaviour.name} but that object has not been spawned or has been unspawned");
            hostBehaviour.SendInt(someInt);
        }
    }
}
