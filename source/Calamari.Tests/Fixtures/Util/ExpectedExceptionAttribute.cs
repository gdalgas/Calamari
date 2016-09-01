using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace Calamari.Tests.Fixtures.Util
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ExpectedExceptionAttribute : NUnitAttribute, IWrapTestMethod
    {
        private readonly Type _expectedExceptionType;
        public string ExpectedMessage;

        public ExpectedExceptionAttribute(Type type = null)
        {
            _expectedExceptionType = type;
        }

        public TestCommand Wrap(TestCommand command)
        {
            return new ExpectedExceptionCommand(command, _expectedExceptionType, ExpectedMessage);
        }

        private class ExpectedExceptionCommand : DelegatingTestCommand
        {
            private readonly Type _expectedType;
            private readonly string _expectedMessage;

            public ExpectedExceptionCommand(TestCommand innerCommand, Type expectedType, string expectedMessage)
                : base(innerCommand)
            {
                _expectedType = expectedType;
                _expectedMessage = expectedMessage;
            }

            public override TestResult Execute(TestExecutionContext context)
            {
                Type caughtType = null;
                string message = null;

                try
                {
                    innerCommand.Execute(context);
                }
                catch (Exception ex)
                {
                    if (ex is NUnitException)
                        ex = ex.InnerException;
                    caughtType = ex.GetType();
                    message = ex.Message;
                }

                var expectedTypeName = _expectedType == null ? "an exception" : _expectedType.Name;

                if ((_expectedType != null && caughtType == _expectedType) || (_expectedType == null && caughtType != null))
                {
                    if (_expectedMessage == null || _expectedMessage == message)
                    {
                        context.CurrentResult.SetResult(ResultState.Success);
                    }
                    else
                    {
                        string.Format("Expected message to be {0} but got {1}", _expectedMessage, message);
                    }
                }
                else if (caughtType != null)
                    context.CurrentResult.SetResult(ResultState.Failure,
                        string.Format("Expected {0} but got {1}", expectedTypeName, caughtType.Name));
                else
                    context.CurrentResult.SetResult(ResultState.Failure,
                        string.Format("Expected {0} but no exception was thrown", expectedTypeName));

                return context.CurrentResult;
            }
        }
    }
}