using Headquarters.Communications;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//Consider a master-slave system? One 'brain' provider, spins up processes with other outposts on its own
//HQ->Master via redis/whatever connector, master->slaves via named pipes

namespace Headquarters.Outposts.Connectors.Pipes
{
    /// <summary>
    /// Implements <see cref="PubSubProvider"/> to provide Pub/Sub communication over named pipes
    /// </summary>
    public class PipeConnector : PubSubProvider
    {
        private Dictionary<ChannelBase, NamedPipeServerStream> _pipeServers;
        private CancellationTokenSource _cancellation;

        /// <summary>
        /// Constructors a new <see cref="PubSubProvider"/> with the given <see cref="Brain"/>
        /// </summary>
        /// <param name="brain"></param>
        public PipeConnector(Brain brain) : base(brain)
        {
            _pipeServers = new Dictionary<ChannelBase, NamedPipeServerStream>();
        }

        /// <summary>
        /// Provides the channel type that this <see cref="PubSubProvider"/> uses
        /// </summary>
        public override Type ChannelType => typeof(PipeChannel);

        /// <summary>
        /// Asynchronously establishes a <see cref="NamedPipeServerStream"/> and waits for connections.
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public override async Task ConnectAsync(string connectionString)
        {
            NamedPipeServerStream pubSubServer =
                new NamedPipeServerStream(Brain.BrainChannel,
                                          PipeDirection.InOut,
                                          maxNumberOfServerInstances: 1,
                                          PipeTransmissionMode.Message,
                                          PipeOptions.Asynchronous)
                {
                    ReadMode = PipeTransmissionMode.Message
                };

            ChannelBase channel = new PipeChannel(Brain.BrainChannel);

            _pipeServers.Add(channel, pubSubServer);

            //Need threads for connection & messaging.
            //Probably split this into a different class if we go with pipes for each command
            await pubSubServer.WaitForConnectionAsync(_cancellation.Token);
        }

        public override async Task PublishAsync(ChannelBase channel, IPublication publication)
        {
            if (_pipeServers.ContainsKey(channel))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(publication.ToString());
                await _pipeServers[channel].WriteAsync(bytes, 0, bytes.Length, _cancellation.Token);
            }
        }

        public override async Task SubscribeAsync(ChannelBase channel, Func<ChannelBase, IPublication, Task> callback)
        {
            NamedPipeServerStream sub =
                new NamedPipeServerStream(channel.ToString(),
                                          PipeDirection.InOut);

            _pipeServers.Add(channel, sub);
            await sub.WaitForConnectionAsync(_cancellation.Token);

            //Extract this from method body
            new Thread(async () => await ThreadLoop(sub, channel, callback)).Start();

            async Task ThreadLoop(NamedPipeServerStream pipe, ChannelBase chan, Func<ChannelBase, IPublication, Task> msgCallback)
            {
                //Use a queue to maintain message order
                Queue<ArraySegment<byte>> bytes = new Queue<ArraySegment<byte>>();
                while (!_cancellation.IsCancellationRequested)
                {
                    byte[] buffer = new byte[100];
                    await pipe.ReadAsync(buffer, 0, 100, _cancellation.Token);
                    bytes.Enqueue(buffer);

                    if (pipe.IsMessageComplete)
                    {
                        //... invoke an event or somesuch for the message here
                        IPublication pub = (PipePublication)Encoding.UTF8.GetString(bytes.SelectMany(seg => seg.Array).ToArray());
                        bytes.Clear();

                        await msgCallback(chan, pub);
                    }
                }
            }
        }

        public override void Dispose()
        {
            _cancellation.Cancel();

            foreach (var pair in _pipeServers)
            {
                pair.Value.Dispose();
            }

            _pipeServers.Clear();
        }
    }
}
