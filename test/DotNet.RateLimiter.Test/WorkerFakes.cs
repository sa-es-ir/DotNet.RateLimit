using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace DotNet.RateLimiter.Test;

public class FakeFunctionContext : FunctionContext
{
    internal const string BindingsFeatureTypeName =
        "Microsoft.Azure.Functions.Worker.Context.Features.IFunctionBindingsFeature";

    public FakeFunctionContext()
    {
        var bindingsInterface = typeof(FunctionContext).Assembly.GetType(BindingsFeatureTypeName)
            ?? throw new InvalidOperationException($"Could not find {BindingsFeatureTypeName}.");

        Bindings = (FakeFunctionBindingsFeatureProxy)DispatchProxy.Create(
            bindingsInterface, typeof(FakeFunctionBindingsFeatureProxy));
        Features = new FakeInvocationFeatures(Bindings);
        Items = new Dictionary<object, object>();
    }

    public FakeFunctionBindingsFeatureProxy Bindings { get; }

    public BindingContext BindingContextValue { get; set; } = new FakeBindingContext();

    public FunctionDefinition FunctionDefinitionValue { get; set; } = new FakeFunctionDefinition();

    public override string InvocationId => string.Empty;

    public override string FunctionId => string.Empty;

    public override TraceContext TraceContext => null!;

    public override BindingContext BindingContext => BindingContextValue;

    public override RetryContext RetryContext => null!;

    public override IServiceProvider InstanceServices { get; set; } = null!;

    public override FunctionDefinition FunctionDefinition => FunctionDefinitionValue;

    public override IDictionary<object, object> Items { get; set; }

    public override IInvocationFeatures Features { get; }
}

/// <summary>
/// Dynamic proxy for the worker's internal function bindings feature, used to capture the
/// invocation result written by the middleware.
/// </summary>
public class FakeFunctionBindingsFeatureProxy : DispatchProxy
{
    private readonly IReadOnlyDictionary<string, object> _triggerMetadata = new Dictionary<string, object>();
    private readonly IReadOnlyDictionary<string, object> _inputData = new Dictionary<string, object>();
    private readonly IDictionary<string, object> _outputBindingData = new Dictionary<string, object>();

    public object? InvocationResult { get; set; }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        switch (targetMethod?.Name)
        {
            case "get_InvocationResult":
                return InvocationResult;
            case "set_InvocationResult":
                InvocationResult = args?[0];
                return null;
            case "get_TriggerMetadata":
                return _triggerMetadata;
            case "get_InputData":
                return _inputData;
            case "get_OutputBindingData":
                return _outputBindingData;
            default:
                return null;
        }
    }
}

public class FakeInvocationFeatures : IInvocationFeatures
{
    private readonly Dictionary<Type, object> _features = new();
    private readonly object? _bindings;

    public FakeInvocationFeatures(object? bindings)
    {
        _bindings = bindings;
    }

    public T Get<T>()
    {
        if (_bindings is not null &&
            typeof(T).FullName == FakeFunctionContext.BindingsFeatureTypeName)
        {
            return (T)_bindings;
        }

        return _features.TryGetValue(typeof(T), out var value) ? (T)value : default!;
    }

    public void Set<T>(T instance)
    {
        if (instance is null)
            _features.Remove(typeof(T));
        else
            _features[typeof(T)] = instance;
    }

    public IEnumerator<KeyValuePair<Type, object>> GetEnumerator() => _features.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class FakeBindingContext : BindingContext
{
    public FakeBindingContext(IReadOnlyDictionary<string, object?>? bindingData = null)
    {
        BindingData = bindingData ?? new Dictionary<string, object?>();
    }

    public override IReadOnlyDictionary<string, object?> BindingData { get; }
}

public class FakeFunctionDefinition : FunctionDefinition
{
    public string PathToAssemblyValue { get; set; } = string.Empty;

    public string EntryPointValue { get; set; } = string.Empty;

    public string IdValue { get; set; } = string.Empty;

    public string NameValue { get; set; } = string.Empty;

    public override ImmutableArray<FunctionParameter> Parameters => ImmutableArray<FunctionParameter>.Empty;

    public override string PathToAssembly => PathToAssemblyValue;

    public override string EntryPoint => EntryPointValue;

    public override string Id => IdValue;

    public override string Name => NameValue;

    public override IImmutableDictionary<string, BindingMetadata> InputBindings =>
        ImmutableDictionary<string, BindingMetadata>.Empty;

    public override IImmutableDictionary<string, BindingMetadata> OutputBindings =>
        ImmutableDictionary<string, BindingMetadata>.Empty;
}

public class FakeHttpRequestData : HttpRequestData
{
    private NameValueCollection _query = new();

    public FakeHttpRequestData(FunctionContext functionContext, string method = "GET", Uri? url = null)
        : base(functionContext)
    {
        MethodValue = method;
        UrlValue = url ?? new Uri("http://localhost/");
        Headers = new HttpHeadersCollection();
    }

    public string MethodValue { get; set; }

    public Uri UrlValue { get; set; }

    public NameValueCollection QueryValue
    {
        get => _query;
        set => _query = value;
    }

    public override Stream Body { get; } = new MemoryStream();

    public override HttpHeadersCollection Headers { get; }

    public override IReadOnlyCollection<IHttpCookie> Cookies => Array.Empty<IHttpCookie>();

    public override Uri Url => UrlValue;

    public override IEnumerable<ClaimsIdentity> Identities => Array.Empty<ClaimsIdentity>();

    public override string Method => MethodValue;

    public override NameValueCollection Query => _query;

    public override HttpResponseData CreateResponse() => new FakeHttpResponseData(FunctionContext);
}

public class FakeHttpResponseData : HttpResponseData
{
    public FakeHttpResponseData(FunctionContext functionContext)
        : base(functionContext)
    {
        Headers = new HttpHeadersCollection();
        Body = new MemoryStream();
    }

    public override HttpStatusCode StatusCode { get; set; }

    public override HttpHeadersCollection Headers { get; set; }

    public override Stream Body { get; set; }

    public override HttpCookies Cookies => null!;
}
