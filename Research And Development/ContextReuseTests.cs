using System;
using System.Collections.Concurrent;
using System.Threading;
using HQ;
using HQ.Attributes;
using HQ.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RnD
{
    [TestClass]
    public class ContextReuseTests
    {
        public class NumberContext : IContextObject
        {
            public NumberContext(CommandRegistry registry)
            {
                Registry = registry;
                Storage = new ConcurrentDictionary<object, object>();
            }

            public dynamic this[object key] { get => Retrieve(key); set => Store(key, value); }

            public CommandRegistry Registry { get; }
            public bool Finalized { get; set; }

            public ConcurrentDictionary<object, object> Storage { get; }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public dynamic Retrieve(object key)
            {
                return Storage[key];
            }

            public void Store(object key, object value)
            {
                Storage.AddOrUpdate(key, value, (k, v) => value);
            }

            public bool TryRetrieve(object key, out dynamic value)
            {
                return Storage.TryGetValue(key, out value);
            }
        }

        [CommandClass]
        public class TestCommand
        {
            /// <summary>
            /// A test executor for this command.
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            [CommandExecutor("A unit testing command",
                @"unit-test",
                RegexStringOptions.None,
                "unit-test")]
            public object TestExecutor(NumberContext context)
            {
                context["count"] += 1;
                return context["count"];
            }
        }

        private object _key = new object();
        private object _key2 = new object();

        [TestMethod]
        public void TestPersistedContext()
        {
            using (CommandRegistry registry = new CommandRegistry(new RegistrySettings()))
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                registry.AddCommand(typeof(TestCommand));
                NumberContext context = new NumberContext(registry);
                context["count"] = 0;

                registry.Contexts.Store(_key, context);

                //Two keys should not point to the same context. Unless they're identical
                Assert.AreEqual(registry.Contexts.Retrieve(_key2), default(NumberContext));
                
                //Count should be 1
                registry.HandleInput("unit-test", _key, (result, output) => {mre.Set(); });
                mre.WaitOne();
                mre.Reset();
                //Count should be 2
                registry.HandleInput("unit-test", _key, (result, output) => {mre.Set(); });
                mre.WaitOne();
                mre.Reset();
                //Count should be 3
                registry.HandleInput("unit-test", _key, (result, output) => {mre.Set(); });
                mre.WaitOne();
                mre.Reset();

                NumberContext retrieved = registry.Contexts.Retrieve(_key) as NumberContext;

                //Ensure that context and retrieved actually map to the same objects with the same values
                Assert.AreEqual(context["count"], retrieved["count"]);
                //And that the count is actually 3
                Assert.AreEqual(3, retrieved["count"]);
            }
        }
    }
}
