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
            private ConcurrentDictionary<object, object> _storage;
            private CommandRegistry _registry;

            public NumberContext(CommandRegistry registry)
            {
                _registry = registry;
                _storage = new ConcurrentDictionary<object, object>();
            }

            public dynamic this[object key] { get => Retrieve(key); set => Store(key, value); }

            public CommandRegistry Registry => _registry;

            public bool Finalized { get; set; }

            public ConcurrentDictionary<object, object> Storage => _storage;

            public dynamic Retrieve(object key)
            {
                return _storage[key];
            }

            public void Store(object key, object value)
            {
                _storage.AddOrUpdate(key, value, (k, v) => value);
            }

            public bool TryRetrieve(object key, out dynamic value)
            {
                return _storage.TryGetValue(key, out value);
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

                registry.StoreContext(_key, context);

                //Two keys should not point to the same context. Unless they're identical
                Assert.ThrowsException<ArgumentException>(() => registry.RetrieveContext<NumberContext>(_key2));

                //Count should be 1
                registry.HandleInput("unit-test", _key, (result, output) => { mre.Set(); });
                mre.WaitOne();
                //Count should be 2
                registry.HandleInput("unit-test", _key, (result, output) => { mre.Set(); });
                mre.WaitOne();
                //Count should be 3
                registry.HandleInput("unit-test", _key, (result, output) => { mre.Set(); });
                mre.WaitOne();

                NumberContext retrieved = registry.RetrieveContext<NumberContext>(_key);

                //Ensure that context and retrieved actually map to the same objects with the same values
                Assert.AreEqual(context["count"], retrieved["count"]);
                //And that the count is actually 3
                Assert.AreEqual(3, retrieved["count"]);
            }
        }
    }
}
