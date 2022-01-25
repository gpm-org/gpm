namespace gpm.Core.Util;

// https://stackoverflow.com/a/57707700/16407587
public struct AsyncOut<T, OUT>
{
    private readonly T _returnValue;
    private readonly OUT _result;

    public AsyncOut(T returnValue, OUT result)
    {
        _returnValue = returnValue;
        _result = result;
    }

    public T Out(out OUT result)
    {
        result = _result;
        return _returnValue;
    }

    public T ReturnValue => _returnValue;

    public static implicit operator AsyncOut<T, OUT>((T returnValue, OUT result) tuple) =>
        new(tuple.returnValue, tuple.result);
}
