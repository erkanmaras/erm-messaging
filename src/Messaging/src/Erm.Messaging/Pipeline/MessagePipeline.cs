using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Erm.Messaging.Pipeline;

public class MessagePipeline<TContext> : IMessagePipeline<TContext> where TContext : IMessageContext
{
    private static readonly Type MiddlewareInterfaceType = typeof(IMessagePipelineMiddleware<TContext>);
    private readonly NextDelegate<TContext> _executor;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IList<IMessagePipelineMiddleware<TContext>> _middlewares;

    public MessagePipeline(IList<IMessagePipelineMiddleware<TContext>> middlewares)
    {
        _middlewares = middlewares;
        _executor = CreatePipelineExecutor(_middlewares.ToArray()) ?? ((_, _) => throw new InvalidOperationException("No middleware configured!"));
    }

    public Task Invoke(TContext context, IEnvelope envelope)
    {
        return _executor.Invoke(context, envelope);
    }

    private static Type GetMiddlewareInterfaceType(Type type)
    {
        return type.GetInterfaces().First(x => x == MiddlewareInterfaceType);
    }

    private static NextDelegate<TContext>? CreatePipelineExecutor(IReadOnlyList<IMessagePipelineMiddleware<TContext>> middlewares)
    {
        NextDelegate<TContext>? expression = null;
        var middlewaresCount = middlewares.Count - 1;

        for (var i = middlewaresCount; i >= 0; i--)
        {
            var middleware = middlewares[i];
            var middlewareInterfaceType = GetMiddlewareInterfaceType(middleware.GetType());
            if (middlewareInterfaceType == null)
            {
                throw new Exception("Middleware must implement IMessageHandlerMiddleware");
            }

            // Select the method on the type which was implemented from the middleware interface.
            var methodInfo = middleware.GetType().GetInterfaceMap(middlewareInterfaceType).TargetMethods.FirstOrDefault();
            if (methodInfo == null)
            {
                throw new Exception("Middleware must implement IMessageHandlerMiddleware and Invoke method.");
            }

            var contextType = typeof(TContext);
            var envelopeType = typeof(IEnvelope);
            var contextParameter = Expression.Parameter(contextType, $"context{i}");
            var envelopeParameter = Expression.Parameter(envelopeType, $"messageEnvelope{i}");

            if (i == middlewaresCount)
            {
                var doneDelegate = CreateDoneDelegate(contextType, envelopeType, i);
                expression = CreateMiddlewareCallDelegate(middleware, methodInfo, contextParameter, envelopeParameter, doneDelegate);
                continue;
            }

            expression = CreateMiddlewareCallDelegate(middleware, methodInfo, contextParameter, envelopeParameter, expression);
        }

        return expression;
    }

    private static NextDelegate<TContext> CreateMiddlewareCallDelegate(
        IMessagePipelineMiddleware<TContext> currentBehavior,
        MethodInfo methodInfo,
        ParameterExpression contextParameter,
        ParameterExpression envelopeParameter,
        Delegate? previous)
    {
        var body = Expression.Call(Expression.Constant(currentBehavior), methodInfo, contextParameter, envelopeParameter, Expression.Constant(previous));
        var lambdaExpression = Expression.Lambda(typeof(NextDelegate<TContext>), body, contextParameter, envelopeParameter);
        return (NextDelegate<TContext>)lambdaExpression.Compile();
    }

    private static NextDelegate<TContext> CreateDoneDelegate(Type contextType, Type envelopeType, int i)
    {
        var contextParameter = Expression.Parameter(contextType, $"context{i + 1}");
        var messageParameter = Expression.Parameter(envelopeType, $"messageEnvelope{i + 1}");
        return (NextDelegate<TContext>)Expression.Lambda(typeof(NextDelegate<TContext>), Expression.Constant(Task.CompletedTask), contextParameter, messageParameter).Compile();
    }
}