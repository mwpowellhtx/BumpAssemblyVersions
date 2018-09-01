using System;

namespace Bav
{
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Provides some Extension Methods for further introspection.
    /// </summary>
    internal static class ExceptionExtensionMethods
    {
        /// <summary>
        /// We are expecting some details following up from the
        /// <see cref="TypeInitializationException"/>.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="outputHelper"></param>
        /// <param name="verify"></param>
        internal static void VerifyThrownExeption<T>(this Exception ex
            , ITestOutputHelper outputHelper, Action<T, ITestOutputHelper> verify = null)
            where T : Exception
        {
            var tex = ex as T;
            Assert.NotNull(tex);
            verify?.Invoke(tex, outputHelper);
        }

        /// <summary>
        /// Verifies that the <typeparamref name="T"/> <see cref="Exception"/> is Thrown by the
        /// <paramref name="action"/>. May provide follow up inspection using
        /// <paramref name="verify"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="verify"></param>
        internal static void Throws<T>(this Action action, Action<T> verify = null)
            where T : Exception
        {
            Assert.NotNull(action);
            var ex = Assert.Throws<T>(action);
            verify?.Invoke(ex);
        }
    }
}
