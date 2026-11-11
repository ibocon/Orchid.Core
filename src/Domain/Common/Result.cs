using System.Diagnostics.CodeAnalysis;

namespace Orchid.Core.Domain.Common;

/// <summary>
/// 작업의 성공 또는 실패 결과를 캡슐화하는 함수형 Result 기본 클래스입니다.
/// </summary>
public class Result
{
    /// <summary>
    /// <see cref="Result"/> 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="isSuccess">성공 여부</param>
    /// <param name="error">오류 객체</param>
    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error != null && error != Error.None)
        {
            throw new InvalidOperationException("Successful result cannot have an error.");
        }

        if (!isSuccess && (error == null || error == Error.None))
        {
            throw new InvalidOperationException("Failure result must have an error.");
        }

        IsSuccess = isSuccess;
        Error = error ?? Error.None;
    }

    /// <summary>
    /// 작업 성공 여부를 반환합니다.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// 작업 실패 여부를 반환합니다.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// 발생한 오류를 반환합니다.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// 성공 결과를 생성합니다.
    /// </summary>
    /// <returns>성공 상태의 <see cref="Result"/></returns>
    public static Result Success()
    {
        return new(true, Error.None);
    }

    /// <summary>
    /// 실패 결과를 생성합니다.
    /// </summary>
    /// <param name="error">발생한 오류</param>
    /// <returns>실패 상태의 <see cref="Result"/></returns>
    public static Result Failure(Error error)
    {
        return new(false, error);
    }

    /// <summary>
    /// 오류 메시지로부터 실패 결과를 생성합니다.
    /// </summary>
    /// <param name="errorMessage">오류 메시지</param>
    /// <returns>실패 상태의 <see cref="Result"/></returns>
    public static Result Failure(string errorMessage)
    {
        return new(false, Error.General(errorMessage));
    }
}

/// <summary>
/// 결과 값을 포함하는 제네릭 함수형 Result 클래스입니다.
/// </summary>
/// <typeparam name="T">결과 값의 형식</typeparam>
[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Functional Result pattern")]
public sealed class Result<T> : Result
{
    private readonly T? _value;

    /// <summary>
    /// <see cref="Result{T}"/> 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="value">결과 값</param>
    /// <param name="isSuccess">성공 여부</param>
    /// <param name="error">오류 객체</param>
    private Result(T? value, bool isSuccess, Error? error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// 성공 시의 결과 값을 반환합니다. 실패 시 <see cref="InvalidOperationException"/>을 발생시킵니다.
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Cannot access Value on a failure result. Error: {Error.Message}");

    /// <summary>
    /// 값을 포함하는 성공 결과를 생성합니다.
    /// </summary>
    /// <param name="value">성공 결과 값</param>
    /// <returns>성공 상태의 <see cref="Result{T}"/></returns>
    public static Result<T> Success(T value)
    {
        return new(value, true, Error.None);
    }

    /// <summary>
    /// 오류를 포함하는 실패 결과를 생성합니다.
    /// </summary>
    /// <param name="error">발생한 오류</param>
    /// <returns>실패 상태의 <see cref="Result{T}"/></returns>
    public static new Result<T> Failure(Error error)
    {
        return new(default, false, error);
    }

    /// <summary>
    /// 오류 메시지로부터 실패 결과를 생성합니다.
    /// </summary>
    /// <param name="errorMessage">오류 메시지</param>
    /// <returns>실패 상태의 <see cref="Result{T}"/></returns>
    public static new Result<T> Failure(string errorMessage)
    {
        return new(default, false, Error.General(errorMessage));
    }
}
