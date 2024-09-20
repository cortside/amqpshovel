using System;
using AutoFixture;

namespace AmqpTools.Test {
    public class BaseTest : IDisposable {
        protected UnitTestFixture testFixture;
        protected readonly Fixture autoFixture;

        protected BaseTest() {
            testFixture = new UnitTestFixture();
            autoFixture = new Fixture();
        }



        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            testFixture.TearDown();
        }
    }
}
